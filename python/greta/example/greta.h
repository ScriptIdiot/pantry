#pragma once

#include <vector>
#include <string>

#define	REGISTRY 1
#define VARIABLE 2

#define HKLM 6
#define HKCU 7

typedef int BOOL;

typedef struct _GRETA
{
	int type;							// Enum: Type
	int hive;							// Enum: Hive
	std::string subkey;					//Registry Key, e.g: SOFTWARE\MICROSOFT\CRYPTOGRAPHY
	std::string valuename;				// Registry Value: e.g: MachineGuid
	std::string envvar;					// Environmental Variable
	int keySz;							// Size of expected key
	std::vector<unsigned char> payload;	// Encrypted data
	BOOL bStatus;						// did it work?
	std::string errormsg;				// what went wrong?!
	int fnv;							// the super secret hash!
} GRETA, *PGRETA;

GRETA Greta();