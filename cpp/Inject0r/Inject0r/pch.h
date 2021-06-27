// pch.h: This is a precompiled header file.
// Files listed below are compiled only once, improving build performance for future builds.
// This also affects IntelliSense performance, including code completion and many code browsing features.
// However, files listed here are ALL re-compiled if any one of them is updated between builds.
// Do not add files here that you will be updating frequently as this negates the performance advantage.

#ifndef PCH_H
#define PCH_H

// add headers that you want to pre-compile here

#include <Windows.h>
#include <TlHelp32.h>
#include <iostream>

#include "Allocator.h"
#include "Helpers.h"

#if defined(_M_X64)
#define ARCH 64
#else
#define ARCH 32
#endif

typedef LRESULT(CALLBACK* SUBCLASSPROC)(
	HWND      hWnd,
	UINT      uMsg,
	WPARAM    wParam,
	LPARAM    lParam,
	UINT_PTR  uIdSubclass,
	DWORD_PTR dwRefData);

typedef struct _SUBCLASS_CALL {
	SUBCLASSPROC pfnSubclass;    // subclass procedure
	WPARAM       uIdSubclass;    // unique subclass identifier
	DWORD_PTR    dwRefData;      // optional ref data
} SUBCLASS_CALL, PSUBCLASS_CALL;

typedef struct _SUBCLASS_FRAME {
	UINT                    uCallIndex;   // index of next callback to call
	UINT                    uDeepestCall; // deepest uCallIndex on stack
	struct _SUBCLASS_FRAME* pFramePrev;  // previous subclass frame pointer
	struct _SUBCLASS_HEADER* pHeader;     // header associated with this frame
} SUBCLASS_FRAME, PSUBCLASS_FRAME;

typedef struct _SUBCLASS_HEADER {
	UINT           uRefs;        // subclass count
	UINT           uAlloc;       // allocated subclass call nodes
	UINT           uCleanup;     // index of call node to clean up
	DWORD          dwThreadId;   // thread id of window we are hooking
	SUBCLASS_FRAME* pFrameCur;   // current subclass frame pointer
	SUBCLASS_CALL  CallArray[1]; // base of packed call node array
} SUBCLASS_HEADER, * PSUBCLASS_HEADER;

typedef struct _PROCESS_INSTRUMENTATION_CALLBACK_INFORMATION
{
	ULONG Version;
	ULONG Reserved;
	PVOID Callback;
} PROCESS_INSTRUMENTATION_CALLBACK_INFORMATION, * PPROCESS_INSTRUMENTATION_CALLBACK_INFORMATION;

typedef enum _PROCESSINFOCLASS {
	ProcessBasicInformation = 0,
	ProcessQuotaLimits = 1,
	ProcessIoCounters = 2,
	ProcessVmCounters = 3,
	ProcessTimes = 4,
	ProcessBasePriority = 5,
	ProcessRaisePriority = 6,
	ProcessDebugPort = 7,
	ProcessExceptionPort = 8,
	ProcessAccessToken = 9,
	ProcessLdrInformation = 10,
	ProcessLdtSize = 11,
	ProcessDefaultHardErrorMode = 12,
	ProcessIoPortHandlers = 13,
	ProcessPooledUsageAndLimits = 14,
	ProcessWorkingSetWatch = 15,
	ProcessUserModeIOPL = 16,
	ProcessEnableAlignmentFaultFixup = 17,
	ProcessPriorityClass = 18,
	ProcessWx86Information = 19,
	ProcessHandleCount = 20,
	ProcessAffinityMask = 21,
	ProcessPriorityBoost = 22,
	ProcessDeviceMap = 23,
	ProcessSessionInformation = 24,
	ProcessForegroundInformation = 25,
	ProcessWow64Information = 26,
	ProcessImageFileName = 27,
	ProcessLUIDDeviceMapsEnabled = 28,
	ProcessBreakOnTermination = 29,
	ProcessDebugObjectHandle = 30,
	ProcessDebugFlags = 31,
	ProcessHandleTracing = 32,
	ProcessIoPriority = 33,
	ProcessExecuteFlags = 34,
	ProcessTlsInformation = 35,
	ProcessCookie = 36,
	ProcessImageInformation = 37,
	ProcessCycleTime = 38,
	ProcessPagePriority = 39,
	ProcessInstrumentationCallback = 40, // that's what we need
	ProcessThreadStackAllocation = 41,
	ProcessWorkingSetWatchEx = 42,
	ProcessImageFileNameWin32 = 43,
	ProcessImageFileMapping = 44,
	ProcessAffinityUpdateMode = 45,
	ProcessMemoryAllocationMode = 46,
	ProcessGroupInformation = 47,
	ProcessTokenVirtualizationEnabled = 48,
	ProcessConsoleHostProcess = 49,
	ProcessWindowInformation = 50,
	MaxProcessInfoClass
} PROCESSINFOCLASS;

typedef enum EXECUTIONTYPE {
	EXECUTEWITHOG = 0,
	EXECUTEWITHPROPAGATE = 1,
	EXECUTEWITHCALLBACKS = 2,
} EXECUTIONTYPE;

typedef enum ALLOCATIONTYPE {
	ALLOCATEWITHBASIC = 0,
	ALLOCATEWITHSECTIONS = 1,
	ALLOCATEWITHMAPPING = 2,
	ALLOCATEWITHENVIRON = 3,
} ALLOCATIONTYPE;

typedef struct _UNICODE_STRING
{
	USHORT Length;
	USHORT MaximumLength;
	PWSTR  Buffer;
} UNICODE_STRING, * PUNICODE_STRING;

typedef enum _SECTION_INHERIT
{
	ViewShare = 1,
	ViewUnmap = 2
} SECTION_INHERIT, * PSECTION_INHERIT;

typedef struct _OBJECT_ATTRIBUTES
{
	ULONG           Length;
	HANDLE          RootDirectory;
	PUNICODE_STRING ObjectName;
	ULONG           Attributes;
	PVOID           SecurityDescriptor;
	PVOID           SecurityQualityOfService;
} OBJECT_ATTRIBUTES, * POBJECT_ATTRIBUTES;

typedef NTSTATUS(NTAPI* _NtSetInformationProcess)
(
	IN HANDLE               ProcessHandle,
	IN PROCESS_INFORMATION_CLASS ProcessInformationClass,
	IN PVOID                ProcessInformation,
	IN ULONG                ProcessInformationLength
	);

typedef PVOID(NTAPI* _MapViewOfFile3)
(
	HANDLE                 FileMapping,
	HANDLE                 Process,
	PVOID                  BaseAddress,
	ULONG64                Offset,
	SIZE_T                 ViewSize,
	ULONG                  AllocationType,
	ULONG                  PageProtection,
	MEM_EXTENDED_PARAMETER* ExtendedParameters,
	ULONG                  ParameterCount
	);

typedef NTSTATUS(WINAPI* _NtCreateSection)
(
	OUT PHANDLE SectionHandle,
	IN ACCESS_MASK DesiredAccess,
	IN POBJECT_ATTRIBUTES ObjectAttributes OPTIONAL,
	IN PLARGE_INTEGER MaximumSize OPTIONAL,
	IN ULONG SectionPageProtection,
	IN ULONG AllocationAttributes,
	IN HANDLE FileHandle OPTIONAL
	);

typedef NTSTATUS(WINAPI* _NtMapViewOfSection)
(
	IN HANDLE SectionHandle,
	IN HANDLE ProcessHandle,
	IN OUT PVOID BaseAddress,
	IN ULONG ZeroBits,
	IN SIZE_T CommitSize,
	IN OUT PLARGE_INTEGER SectionOffset OPTIONAL,
	IN OUT PSIZE_T ViewSize,
	IN SECTION_INHERIT InheritDisposition,
	IN ULONG AllocationType,
	IN ULONG Win32Protect
	);

typedef NTSTATUS(WINAPI* _NtUnmapViewOfSection)
(
	IN HANDLE ProcessHandle,
	IN PVOID BaseAddress
	);

#endif //PCH_H
