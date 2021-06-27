using System;
using System.Runtime.InteropServices;

namespace CSharpRunAs
{
    class NativeMethods
    {
        public const uint GENERIC_ALL = 0x10000000;
        public enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }
        public enum dwLogonProvider : int
        {
            LOGON32_PROVIDER_DEFAULT = 1,
            LOGON32_PROVIDER_WINNT50 = 2,
            LOGON32_PROVIDER_WINNT40 = 3
        }
        public enum dwLogonType : int
        {
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK = 3,
            LOGON32_LOGON_BATCH = 4,
            LOGON32_LOGON_SERVICE = 5,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
            LOGON32_LOGON_NEW_CREDENTIALS = 9
        }
        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [Flags]
        public enum dwLogonFlags
        {
            LOGON_WITH_PROFILE = 0x00000001,
            LOGON_NETCREDENTIALS_ONLY = 0x00000002
        }

        [Flags]
        public enum dwCreationFlags
        {
            CREATE_SUSPENDED = 0x00000004,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NO_WINDOW = 0x08000000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessInformation
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct StartupInformation
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public ushort wShowWindow;
            public short cbReserved2;
            public int lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessWithLogonW
            (
                String lpUsername,
                String lpDomain,
                String lpPassword,
                dwLogonFlags logonFlags,
                String lpApplicationName,
                String lpCommandLine,
                dwCreationFlags creationFlags,
                IntPtr lpEnvironment,
                String lpCurrentDirectory,
                ref StartupInformation lpStartupInfo,
                out ProcessInformation lpProcessInformation
            );
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessWithTokenW
            (
                IntPtr hToken, 
                dwLogonFlags logonFlags, 
                String lpApplicationName, 
                String lpCommandLine, 
                dwCreationFlags creationFlags, 
                IntPtr lpEnvironment, 
                String lpCurrentDirectory, 
                [In] ref StartupInformation lpStartupInfo, 
                out ProcessInformation lpProcessInformation
            );
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateTokenEx
            (
                IntPtr hExistingToken,
                uint dwDesiredAccess,
                ref SECURITY_ATTRIBUTES lpTokenAttributes,
                SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
                TOKEN_TYPE TokenType,
                ref IntPtr phNewToken
            );
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool LogonUser
            (
                String pszUserName,
                String pszDomain,
                String pszPassword,
                dwLogonType logonType,
                dwLogonProvider logonProvider,
                ref IntPtr phToken
            );
    }
}
