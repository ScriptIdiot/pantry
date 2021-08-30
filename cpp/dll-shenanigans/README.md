# DLL Shenanigans

This solution is supporting code for [Exploring DLL Loads](https://mez0.cc/posts/exploring-dll-loads/).

The projects:

1. [dark-loader](./dark-loader): A project that wraps [bats3c/DarkLoadLibrary](https://github.com/bats3c/DarkLoadLibrary) and allows for quick testing of both modes offered.
```
PS> .\dark-loader.exe <mode> <path>
PS> .\dark-loader.exe 1 c:\file.dll
PS> .\dark-loader.exe 2 c:\file.dll

MODES:
    - 1: LOAD_LOCAL_FILE: Load a DLL from a buffer.
    - 2: LOAD_MEMORY: Load a DLL from the file system.
```
2. [debug-dll](./debug-dll): A small DLL that pops `MessageBoxA`.
3. [reflective-debug-dll](./reflective-debug-dll): Also pops `MessageBoxA`... but reflective!
4. [on-disk-loader](./on-disk-loader): Load a DLL from disk into the current process, or create a new one to load into
```
PS> .\on-disk-loader.exe <mode> <path>
PS> .\on-disk-loader.exe 1 c:\file.dll
PS> .\on-disk-loader.exe 2 c:\file.dll

MODES:
    - 1: Local
    - 2: Remote
```
5. [in-mem-loader](./-mem-loader): Load a Reflective DLL from disk into the current process, or create a new one to load into
```
PS> .\in-mem-loader.exe <mode> <path>
PS> .\in-mem-loader.exe 1 c:\file.dll
PS> .\in-mem-loader.exe 2 c:\file.dll

MODES:
    - 1: Local
    - 2: Remote
```
6. [load-dll-from-shellcode](./load-dll-from-shellcode): Load [monoxgas/sRDI](https://github.com/monoxgas/sRDI).

```
PS> .\load-dll-from-shellcode.exe <mode> <path>
PS> .\load-dll-from-shellcode 1 c:\shellcode.bin
PS> .\load-dll-from-shellcode 2 c:\shellcode.bin

MODES:
    - 1: Local
    - 2: Remote
```