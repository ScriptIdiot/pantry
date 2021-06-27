#ifndef Helpers_H
#define Helpers_H

FARPROC ResolveFunction(char* function, char* dll, BOOL Load = FALSE);
int GetProcessByName(wchar_t* name);
BOOL SetPrivilege(wchar_t* lpszPrivilege, BOOL bEnablePrivilege);
PROCESS_INFORMATION CreateProcessUnderExplorer();
DWORD ReadBytes(char* path, LPVOID* shellcode);
DWORD GetCurrentIntegrity();
#endif