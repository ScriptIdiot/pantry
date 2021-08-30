#include <stdio.h>
#include <windows.h>
#include "pebutils.h"
#include "darkloadlibrary.h"

DWORD read_bytes_from_file(LPCWSTR path, LPVOID* buf)
{
	HANDLE hFile;
	DWORD  size, readAmount = 0;

	hFile = CreateFileW(path, GENERIC_READ, 0, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

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

void local_load_dll(LPCWSTR lpwBuffer, int mode)
{
	DWORD dwFlags = 0;
	LPVOID lpFileBuffer = NULL;
	DWORD dwLen = 0;
	LPWSTR lpwName = NULL;

	switch (mode)
	{
	case 1:
		printf("  :: Setting: LOAD_LOCAL_FILE\n");
		dwFlags = LOAD_LOCAL_FILE;
		break;
	case 2:
		printf("  :: Setting: LOAD_MEMORY\n");
		dwFlags = LOAD_MEMORY;
		break;
	}

	if (dwFlags == 0)
	{
		return;
	}

	if (mode == 2)
	{
		printf("  :: Reading bytes (replicating in-memory loading)!\n");
		dwLen = read_bytes_from_file(lpwBuffer, &lpFileBuffer);
		lpwBuffer = NULL;
		lpwName = L"DarkLoadLibraryDebugging";
		if (dwLen > 0)
		{
			printf("  :: DLL Size: %ld\n", dwLen);
			printf("  :: Loading in as: %ws\n", lpwName);
		}
		else
		{
			printf("[!] Failed to load bytes from!");
			return;
		}
	}

	if (mode == 1)
	{
		printf("  :: Letting DarkLoadLibrary read from disk!\n");
	}

	GETPROCESSHEAP pGetProcessHeap = (GETPROCESSHEAP)GetFunctionAddress(IsModulePresent(L"Kernel32.dll"), (char*)"GetProcessHeap");
	HEAPFREE pHeapFree = (HEAPFREE)GetFunctionAddress(IsModulePresent(L"Kernel32.dll"), (char*)"HeapFree");

	PDARKMODULE DarkModule = DarkLoadLibrary(dwFlags, lpwBuffer, lpFileBuffer, dwLen, lpwName);

	if (DarkModule->bSuccess == FALSE)
	{
		printf("[!] %S\n", DarkModule->ErrorMsg);
		pHeapFree(pGetProcessHeap(), 0, DarkModule->ErrorMsg);
		pHeapFree(pGetProcessHeap(), 0, DarkModule);
		return;
	}

	printf("  :: Module: %p\n", (HMODULE)DarkModule->ModuleBase);

	if (pHeapFree(pGetProcessHeap(), 0, DarkModule->ErrorMsg))
	{
		printf("  :: Freed DarkModule->ErrorMsg\n");
	}
	else
	{
		printf("[!] Failed to free DarkModule->ErrorMsg!\n");
	}
	if (pHeapFree(pGetProcessHeap(), 0, DarkModule))
	{
		printf("  :: Freed DarkModule\n");
	}
	else
	{
		printf("[!] Failed to free DarkModule\n");
	}

	return;
}

int wmain(int argc, wchar_t* argv[])
{
	LPCWSTR path;
	int mode = 0;

	if (argc == 3)
	{
		mode = _wtoi(argv[1]);
		path = argv[2];
	}
	else
	{
		printf("PS> .\\dark-loader.exe <mode> <path>\n");
		printf("PS> .\\dark-loader.exe 1 c:\\file.dll\n");
		printf("PS> .\\dark-loader.exe 2 c:\\file.dll\n");
		printf("\nMODES:\n");
		printf("    - 1: LOAD_LOCAL_FILE: Load a DLL from the file system.\n");
		printf("    - 2: LOAD_MEMORY: Load a DLL from a buffer.\n");
		return -1;
	}

	printf("|> Loading: %ws\n", path);

	local_load_dll(path, mode);

	return 0;
}