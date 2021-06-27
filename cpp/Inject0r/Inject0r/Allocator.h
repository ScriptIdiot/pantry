#ifndef Allocator_H
#define Allocator_H

LPVOID Basic(HANDLE hProcess, LPVOID buf, int bufSize, DWORD protectionType);
LPVOID FileMapping(HANDLE hProcess, LPVOID buf, int bufSize, DWORD protectionType);
LPVOID Sections(HANDLE hProcess, LPVOID buf, int bufSize, DWORD protectionType);
LPVOID HandleAllocation(HANDLE hProcess, LPVOID buf, int bufSize, DWORD protectionType, int allocationType);
#endif