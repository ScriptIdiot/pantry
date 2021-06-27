#include "pch.h"

LPVOID Basic(HANDLE hProcess, LPVOID buf, int bufSize, DWORD protectionType)
{
	LPVOID pAddress = VirtualAllocEx(hProcess, nullptr, bufSize, MEM_RESERVE | MEM_COMMIT, protectionType);
	if (pAddress == NULL) {
		printf("[!] VirtualAllocEx(): %d\n", ::GetLastError());
		return NULL;
	}
	return pAddress;
}

LPVOID Sections(HANDLE hProcess, LPVOID buf, int bufSize, DWORD protectionType)
{
	HANDLE					hSection;
	NTSTATUS				status;
	LARGE_INTEGER			largeInt;
	PVOID					localSectionBase = NULL;
	PVOID					remoteSectionBase = NULL;
	SIZE_T					viewSize = 0;
	_NtCreateSection		NtCreateSection = (_NtCreateSection)ResolveFunction((char*)"NtCreateSection", (char*)"ntdll.dll", TRUE);
	_NtMapViewOfSection		NtMapViewOfSection = (_NtMapViewOfSection)ResolveFunction((char*)"NtMapViewOfSection", (char*)"ntdll.dll", TRUE);
	_NtUnmapViewOfSection	NtUnmapViewOfSection = (_NtUnmapViewOfSection)ResolveFunction((char*)"NtUnmapViewOfSection", (char*)"ntdll.dll", TRUE);

	if (NtCreateSection == NULL) {
		printf("[!] ResolveFunction()\n");
		return NULL;
	}
	if (NtMapViewOfSection == NULL) {
		printf("[!] ResolveFunction()\n");
		return NULL;
	}
	if (NtUnmapViewOfSection == NULL) {
		printf("[!] ResolveFunction()\n");
		return NULL;
	}

	OBJECT_ATTRIBUTES objectAttr = { sizeof(objectAttr) };

	largeInt.HighPart = 0;
	largeInt.LowPart = bufSize;

	status = NtCreateSection(&hSection, SECTION_MAP_READ | SECTION_MAP_WRITE | SECTION_MAP_EXECUTE, NULL, (PLARGE_INTEGER)&largeInt, PAGE_EXECUTE_READWRITE, SEC_COMMIT, NULL);
	if (status != S_OK) {
		printf("[!] NtCreateSection(): %d\n", ::GetLastError());
		return NULL;
	}
	printf("  |_ NtCreateSection(): %p\n", hSection);

	status = NtMapViewOfSection(hSection, GetCurrentProcess(), &localSectionBase, 0, 0, 0, &viewSize, (SECTION_INHERIT)2, 0, PAGE_READWRITE);
	if (status != S_OK) {
		printf("[!] NtCreateSection(): %d\n", ::GetLastError());
		return NULL;
	}
	printf("  |_ NtMapViewOfSection(): %p\n", localSectionBase);

	status = NtMapViewOfSection(hSection, hProcess, &remoteSectionBase, 0, 0, 0, &viewSize, (SECTION_INHERIT)2, 0, protectionType);
	if (status != S_OK) {
		printf("[!] NtCreateSection(): %d\n", ::GetLastError());
		return NULL;
	}
	printf("  |_ NtMapViewOfSection(): %p\n", remoteSectionBase);

	RtlMoveMemory(localSectionBase, buf, bufSize);
	::CloseHandle(hSection);
	return remoteSectionBase;
}

LPVOID FileMapping(HANDLE hProcess, LPVOID buf, int bufSize, DWORD protectionType)
{
	_MapViewOfFile3 MapViewOfFile3 = (_MapViewOfFile3)ResolveFunction((char*)"MapViewOfFile3", (char*)"kernelbase.dll", TRUE);
	if (MapViewOfFile3 == NULL) {
		printf("[!] ResolveFunction()\n");
		return NULL;
	}

	HANDLE hFileMap = ::CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_EXECUTE_READWRITE, 0, (DWORD)bufSize, NULL);
	if (hFileMap == NULL)
	{
		printf("[!] CreateFileMapping(): %d\n", ::GetLastError());
		return NULL;
	}
	printf("  |_ CreateFileMapping(): %p\n", hFileMap);
	LPVOID lpMapAddress = MapViewOfFile3(hFileMap, GetCurrentProcess(), NULL, 0, 0, 0, PAGE_READWRITE, NULL, 0);
	if (lpMapAddress == NULL)
	{
		printf("[!] MapViewOfFile3(): %d\n", ::GetLastError());
		return NULL;
	}
	printf("  |_ MapViewOfFile3(): %p\n", lpMapAddress);

	memcpy((PVOID)lpMapAddress, buf, bufSize);

	LPVOID lpMapAddressRemote = MapViewOfFile3(hFileMap, hProcess, NULL, 0, 0, 0, protectionType, NULL, 0);
	if (lpMapAddressRemote == NULL)
	{
		printf("[!] MapViewOfFile3(): %d\n", ::GetLastError());
		return NULL;
	}
	printf("  |_ MapViewOfFile3(): %p\n", lpMapAddressRemote);

	UnmapViewOfFile(hFileMap);
	CloseHandle(hFileMap);
	return lpMapAddressRemote;
}

LPVOID HandleAllocation(HANDLE hProcess, LPVOID buf, int bufSize, DWORD protectionType, int allocationType)
{
	LPVOID pAddress = nullptr;

	switch (allocationType) {
	case ALLOCATEWITHBASIC:
		pAddress = Basic(hProcess, buf, bufSize, PAGE_EXECUTE_READWRITE); // When I WriteProcessMemory in Basic(), I get access violations :o
		if (!WriteProcessMemory(hProcess, pAddress, buf, bufSize, NULL)) {
			printf("[!] WriteProcessMemory(): %d\n", ::GetLastError());
			return NULL;
		}
		break;
	case ALLOCATEWITHSECTIONS:
		pAddress = Sections(hProcess, buf, bufSize, PAGE_EXECUTE_READWRITE);
		break;
	case ALLOCATEWITHMAPPING:
		pAddress = FileMapping(hProcess, buf, bufSize, PAGE_EXECUTE_READWRITE);
		break;
	default:
		pAddress = nullptr;
	}
	return pAddress;
}