# CSharpRunAs
Run As... In C#...

This project uses:
1. [LogonUser](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-logonusera)
2. [DuplicateTokenEx](https://docs.microsoft.com/en-us/windows/win32/api/securitybaseapi/nf-securitybaseapi-duplicatetokenex)
3. [CreateProcessWithToken](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-createprocesswithtokenw)

## Usage

```
powershell
C:\CSharpRunAs.exe <domain> <user> <password> <binary>
```

Dont do what I did, make sure that the binary is reachable from the new user and don't leave it sitting on the Administrator's desktop and get annoyed when it doesnt work :)