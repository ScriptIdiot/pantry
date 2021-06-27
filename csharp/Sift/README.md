# Sift

`Sift` is a .NET utility to Sift through shares and identify files based on keywords and/or extensions.

```

            ~ Sift ~
         />_________________________________
[########[]_________________________________|
         \>
                        ~ mez0 ~

```

## Usage

The available options:

```
OPTIONS:
   -targets       Targets to check
   -username      Username to authenticate with
   -domain        Domain to authenticate with
   -password      Password to authenticate with
   -keywords      Keywords to look for in file names (Default: None)
   -extensions    Extensions to look for in file names (Default:  "ps1", "docx", "docm", "xlsx", "xlsm", "xls", "hta", "vba", "vbs")
   -jitter        Random sleep between 0 and X
   -tcp           Enable/Disable the default TCP check
   -randomise     Loop through targets randomly
```

Targets can take in any of the following:

- File path

```
-targets="c:\users\admin\desktop\computers.txt"
```

- Comma separated list of IPs or subnets, example:

```
-targets="192.168.0.2,192.168.0.3"
-targets="192.168.0.0/24,192.168.1.0/24"
```

- Dash separated:

```
-targets="192.168.0.12-192.168.0.252"
```

- Or automatic discovery with AD:

```
-targets="auto"
```

The authentication is handled by the `Sift.Helpers.Network.Authenticate()` method, which uses [LogonUser](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-logonusera):

```csharp
public static IntPtr Authenticate()
{
    IntPtr hToken = IntPtr.Zero;
    bool bLogonUser = NativeMethods.LogonUser(Arguments.Username, Arguments.Domain, Arguments.Password, Enums.DwLogonType.LOGON32_LOGON_NEW_CREDENTIALS, Enums.DwLogonProvider.LOGON32_PROVIDER_WINNT50, ref hToken);
    if (!bLogonUser)
    {
        Logger.Print(Logger.STATUS.ERROR, "LogonUser(): ", String.Format("FAILURE ({0})", Logger.GetLastWindowsError()));
    }
    Logger.Print(Logger.STATUS.VERBOSE, "LogonUser() == ", Logger.GetLastWindowsError());
    return hToken;
}
```

The two important things here are:

- **LOGON32_LOGON_NEW_CREDENTIALS**:

> This logon type allows the caller to clone its current token and specify new credentials for outbound connections. The new logon session has the same local identifier but uses different credentials for other network connections. This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.

- **LOGON32_PROVIDER_WINNT50**:

> Use the negotiate logon provider.

The flags are all Booleans and alter the behaviour slightly:

- **Jitter**: Specify an Int32 which will be converted into milliseconds and used to generate the upper value of a random Int32. This will then be used as a sleep between each request made to the network. Note, originally a paralleled for loop was used here, but it was removed because it didn't respect the jitter. This can be reimplemented in the `SharpShares.Helpers.Network.GetTargetsWithOpenPorts()` method by replacing the `foreach` with:

```csharp
Parallel.ForEach(targets, target =>
{
    
});
```

And again in `SharpShares.Helpers.Execution.Execute()`. To be fair, there should probably be a `parallel` flag.

- **TCP**: Enable or disable the `TcpClient()` connection to 445 (saves from listing shares on servers with SMB listening).

- **Randomise**: Re-jig the `List<String>` to be random, self-explanatory.
- **Keywords:** By default none are used. However, if set, this will look in the FILENAME NOT THE CONTENTS.
- **Extensions**: By default: "ps1", "docx", "docm", "xlsx", "xlsm", "xls", "hta", "vba", "vbs"

## Example

**Without credentials**:

```
PS C:\> .\Sift.exe -targets="10.10.11.38"

            ~ Sift ~
         />_________________________________
[########[]_________________________________|
         \>
                        ~ mez0 ~

[22:37:10] ==> Total targets: 1
[22:37:10] ==> TCP: True

[22:37:10] |_ Username: NULL
[22:37:10] |_ Password: NULL

[22:37:10] ==> Connecting to SMB on 1 host(s):
[22:37:10] |_ 10.10.11.38 (TRAZOKU): OPEN

[22:37:10] ==> Targets with SMB Open: 1

[22:37:10] ==> Request shares on 1 host(s) as Zalem\mez0

[22:37:10] ==> Shares (10.10.11.38):
[22:37:10] |_ ADMIN$
[22:37:10] |_ C$
[22:37:10] |_ D$
[22:37:10] |_ E$
[22:37:10] |_ F$
[22:37:10] |_ G$
[22:37:10] |_ H$
[22:37:10] |_ IPC$

[22:37:10] ==> Searching for: ps1, docx, docm, xlsx, xlsm, xls, hta, vba, vbs
[22:37:10] |_ Processing: ADMIN$
[22:37:12] |_ Processing: C$
[22:37:12] |_ Processing: D$
[22:37:12] |_ Processing: E$
[22:37:13] |_ Processing: F$
[22:37:13] |_ Processing: G$
\\10.10.11.38\C$\users\dev\desktop\script-with-credentials.ps1
```
