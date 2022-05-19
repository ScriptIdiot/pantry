#include <windows.h>
#include <stdio.h>
#include <tlhelp32.h>
#include <vector>
#include "minhook/MinHook.h"

typedef PVOID (NTAPI* _RtlAllocateHeap)
(
    PVOID  HeapHandle,
    ULONG  Flags,
    SIZE_T Size
);

typedef PVOID (NTAPI* _RtlReAllocateHeap)
(
    PVOID HeapHandle,
    ULONG Flags,
    PVOID MemoryPointer,
    ULONG Size
);

typedef BOOLEAN (NTAPI* _RtlFreeHeap)
(
    PVOID HeapHandle,
    ULONG Flags,
    PVOID BaseAddress
);

_RtlAllocateHeap pRtlAllocateHeap = nullptr;
_RtlReAllocateHeap pRtlReAllocateHeap = nullptr;
_RtlFreeHeap pRtlFreeHeap = nullptr;

typedef struct _HEAP
{
    HANDLE hHeap;
    PVOID pAllocated;
    ULONG ulFlags;
    SIZE_T Size;
} HEAP, *PHEAP;

std::vector<HEAP> heaps = {};

DWORD GlobalThreadId = GetCurrentThreadId();

VOID Xor(unsigned char* lpData, DWORD cbData)
{
    for (int i = 0; i < cbData; i++)
    {
        lpData[i] = lpData[i] ^ 0xff;
    }
}

int EncryptHeap()
{
    {
        HEAPLIST32 heapList;

        HANDLE hHeapSnap = CreateToolhelp32Snapshot(TH32CS_SNAPHEAPLIST, GetCurrentProcessId());

        heapList.dwSize = sizeof(HEAPLIST32);

        if (hHeapSnap == INVALID_HANDLE_VALUE)
        {
            printf("CreateToolhelp32Snapshot failed (%d)\n", GetLastError());
            return 1;
        }

        LPVOID pHeapBase = HeapAlloc(GetProcessHeap(), 0, 14);

        if (pHeapBase == NULL)
        {
            return -1;
            
        }

        memcpy(pHeapBase, "10.10.11.205\0", 14);

        BOOL bFirstHeap = Heap32ListFirst(hHeapSnap, &heapList);

        if (bFirstHeap == FALSE)
        {
            CloseHandle(hHeapSnap);
            return -1;
        }

        do
        {
            HEAPENTRY32 he;
            ZeroMemory(&he, sizeof(HEAPENTRY32));
            he.dwSize = sizeof(HEAPENTRY32);

            if (Heap32First(&he, GetCurrentProcessId(), heapList.th32HeapID))
            {
                do
                {
                    if (HeapLock(he.hHandle)) {
                        PROCESS_HEAP_ENTRY heapEntry;
                        heapEntry.lpData = NULL;
                        while (HeapWalk(he.hHandle, &heapEntry) != FALSE) {
                            if ((heapEntry.wFlags & PROCESS_HEAP_ENTRY_BUSY) && heapEntry.cbData > 0)
                            {
                                if (pHeapBase == heapEntry.lpData)
                                {
                                    //Xor((unsigned char*)heapEntry.lpData, heapEntry.cbData);
                                }
                            }
                        }
                        if (HeapUnlock(he.hHandle) == FALSE) {
                            printf("HeapUnlock %ld\n", GetLastError());
                        }
                    }
                    he.dwSize = sizeof(HEAPENTRY32);
                } while (Heap32Next(&he));
            }
            heapList.dwSize = sizeof(HEAPLIST32);
        } while (Heap32ListNext(hHeapSnap, &heapList));

        CloseHandle(hHeapSnap);
    }

    return 0;
}

PVOID NTAPI RtlAllocateHeapHook(PVOID HeapHandle, ULONG Flags, SIZE_T Size)
{
    PVOID pAllocated = pRtlAllocateHeap(HeapHandle, Flags, Size);

    HEAP heap;
    heap.pAllocated = pAllocated;
    heap.ulFlags = Flags;
    heap.Size = Size;
    heap.hHeap = HeapHandle;

    return pAllocated;
}
 
PVOID NTAPI RtlReAllocateHeapHook(PVOID HeapHandle, ULONG Flags, PVOID MemoryPointer, ULONG Size)
{
    return pRtlReAllocateHeap(HeapHandle, Flags, MemoryPointer, Size);
}

BOOLEAN NTAPI RtlFreeHeapHook(PVOID HeapHandle, ULONG Flags, PVOID BaseAddress)
{
    return pRtlFreeHeap(HeapHandle, Flags, BaseAddress);
}

int main()
{
    MH_STATUS status;

    if (MH_Initialize() != MH_OK) {
        return -1;
    }

    status = MH_CreateHookApi(
        L"ntdll",
        "RtlAllocateHeap",
        RtlAllocateHeapHook,
        reinterpret_cast<LPVOID*>(&pRtlAllocateHeap)
    );

    status = MH_CreateHookApi(
        L"ntdll",
        "RtlReAllocateHeap",
        RtlReAllocateHeapHook,
        reinterpret_cast<LPVOID*>(&pRtlReAllocateHeap)
    );

    status = MH_CreateHookApi(
        L"ntdll",
        "RtlFreeHeap",
        RtlFreeHeapHook,
        reinterpret_cast<LPVOID*>(&pRtlFreeHeap)
    );

    status = MH_EnableHook(MH_ALL_HOOKS);

    if (status == MH_OK)
    {
        printf("Hooks Enabled!\n");
    }
    else
    {
        printf("Hooks failed to start!\n");
        return -1;
    }

    EncryptHeap();

    
    return 0;
}