# Inject0r

A few ways of doing injection split off into Allocation and Execution.

**<u>Allocation:</u>**

1. VirtualAllocEx :ice_cream:
2. Sections :ice_cream:
3. [MapViewOfFile3](https://splintercod3.blogspot.com/p/weaponizing-mapping-injection-with.html)

**<u>Execution:</u>**

1. CreateRemoteThread :ice_cream:
2. [PROPagate](https://modexp.wordpress.com/2018/08/23/process-injection-propagate/)
3. [NtSetInformationProcess Callbacks](https://splintercod3.blogspot.com/p/weaponizing-mapping-injection-with.html)

## Usage

```
|-> Name: Inject0r
|-> Author: Mez-0

Usage:
PS C:\> Inject0r.exe <execution> <allocation> <bin>

Example:
PS C:\> Inject0r.exe 1 1 c:\payload.bin

Allocation Methods:
|-> 0 == VirtualAllocEx, WriteProcessMemory
|-> 1 == NtCreateSection, NtMapViewOfSection, NtUnmapViewOfSection
|-> 2 == MapViewOfFile3, CreateFileMapping, UnmapViewOfFile

Execution Methods:
|-> 0 == CreateRemoteThread
|-> 1 == Propagate (Injects into EXPLORER.EXE)
|-> 2 == NtSetInformationProcess Callbacks
```

