// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		MessageBoxA(nullptr, "DLL_PROCESS_ATTACH", "DLL_PROCESS_ATTACH", MB_OK);
		break;
	case DLL_THREAD_ATTACH:
		MessageBoxA(nullptr, "DLL_THREAD_ATTACH", "DLL_THREAD_ATTACH", MB_OK);
		break;
	case DLL_THREAD_DETACH:
		MessageBoxA(nullptr, "DLL_THREAD_DETACH", "DLL_THREAD_DETACH", MB_OK);
		break;
	case DLL_PROCESS_DETACH:
		MessageBoxA(nullptr, "DLL_PROCESS_DETACH", "DLL_PROCESS_DETACH", MB_OK);
		break;
	}
	return TRUE;
}