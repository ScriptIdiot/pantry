#pragma comment(lib, "crypt32.lib")

#include <stdio.h>
#include <iostream>
#include <fstream>
#include <vector>
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

int main(int argc, char** argv)
{
    std::string path;
    if (argc != 2)
    {
        printf(".\\build_payload.exe <shellcode>\n");
        return -1;
    }

    DATA_BLOB blob_encrypted;
    DATA_BLOB blob_decrypted;

    path = argv[1];

    printf("%s\n", path.c_str());

    int payloadSz = 0;
    unsigned char* payload = ReadBytes(path, &payloadSz);

    DATA_BLOB blob_input =
    {
        payloadSz + 1,
        (BYTE*)payload
    };

    printf("Original Payload Size: %d\n\n", payloadSz);

    pretty_print("Plain", blob_input);

    if (!CryptProtectData(&blob_input, NULL, NULL, NULL, NULL, CRYPTPROTECT_LOCAL_MACHINE | CRYPTPROTECT_UI_FORBIDDEN, &blob_encrypted))
    {
        printf("CryptProtectData(): %d\n", GetLastError());
        return -1;
    }

    pretty_print("Encrypted", blob_encrypted);

    if (!CryptUnprotectData(&blob_encrypted, NULL, NULL, NULL, NULL, CRYPTPROTECT_VERIFY_PROTECTION, &blob_decrypted))
    {
        printf("CryptUnprotectData(): %d\n", GetLastError());
        return -1;
    }

    pretty_print("Decrypted", blob_decrypted);

    LocalFree(blob_encrypted.pbData);
    LocalFree(blob_decrypted.pbData);

    printf("Original Payload Size: %d\n\n", blob_decrypted.cbData);
    return 0;
}

