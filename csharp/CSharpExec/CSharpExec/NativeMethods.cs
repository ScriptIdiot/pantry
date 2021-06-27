using System;
using System.Runtime.InteropServices;

namespace CSharpExec
{
    internal class NativeMethods
    {
        public enum ShareResponses
        {
            NO_ERROR = 0,
            ERROR_ACCESS_DENIED = 5,
            ERROR_ALREADY_ASSIGNED = 85,
            ERROR_BAD_DEVICE = 1200,
            ERROR_BAD_NET_NAME = 67,
            ERROR_BAD_PROVIDER = 1204,
            ERROR_CANCELLED = 1223,
            ERROR_EXTENDED_ERROR = 1208,
            ERROR_INVALID_ADDRESS = 487,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_INVALID_PASSWORD = 1216,
            ERROR_MORE_DATA = 234,
            ERROR_NO_MORE_ITEMS = 259,
            ERROR_NO_NET_OR_BAD_PATH = 1203,
            ERROR_NO_NETWORK = 1222,
            ERROR_SESSION_CREDENTIAL_CONFLICT = 1219,
            ERROR_BAD_PROFILE = 1206,
            ERROR_CANNOT_OPEN_PROFILE = 1205,
            ERROR_DEVICE_IN_USE = 2404,
            ERROR_NOT_CONNECTED = 2250,
            ERROR_OPEN_FILES = 2401
        }

        public enum ResourceScope
        {
            CONNECTED = 0x00000001,
            GLOBALNET = 0x00000002,
            REMEMBERED = 0x00000003,
        }

        public enum ResourceType
        {
            ANY = 0x00000000,
            DISK = 0x00000001,
            PRINT = 0x00000002,
        }

        public enum ResourceDisplayType
        {
            GENERIC = 0x00000000,
            DOMAIN = 0x00000001,
            SERVER = 0x00000002,
            SHARE = 0x00000003,
            FILE = 0x00000004,
            GROUP = 0x00000005,
            NETWORK = 0x00000006,
            ROOT = 0x00000007,
            SHAREADMIN = 0x00000008,
            DIRECTORY = 0x00000009,
            TREE = 0x0000000A,
            NDSCONTAINER = 0x0000000A,
        }

        [Flags]
        public enum ResourceUsage
        {
            CONNECTABLE = 0x00000001,
            CONTAINER = 0x00000002,
            NOLOCALDEVICE = 0x00000004,
            SIBLING = 0x00000008,
            ATTACHED = 0x00000010,
        }

        [Flags]
        public enum Connect
        {
            UPDATE_PROFILE = 0x00000001,
            INTERACTIVE = 0x00000008,
            PROMPT = 0x00000010,
            REDIRECT = 0x00000080,
            LOCALDRIVE = 0x00000100,
            COMMANDLINE = 0x00000800,
            CMD_SAVECRED = 0x00001000,
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NETRESOURCE
        {
            public ResourceScope dwScope = 0;
            public ResourceType dwType = 0;
            public ResourceDisplayType dwDisplayType = 0;
            public ResourceUsage dwUsage = 0;

            public string lpLocalName = "";
            public string lpRemoteName = "";
            public string lpComment = "";
            public string lpProvider = "";
        }

        public enum dwDesiredAccess
        {
            STANDARD_RIGHTS_REQUIRED = 0xF0000,
            SERVICE_QUERY_CONFIG = 0x00001,
            SERVICE_CHANGE_CONFIG = 0x00002,
            SERVICE_QUERY_STATUS = 0x00004,
            SERVICE_ENUMERATE_DEPENDENTS = 0x00008,
            SERVICE_START = 0x00010,
            SERVICE_STOP = 0x00020,
            SERVICE_PAUSE_CONTINUE = 0x00040,
            SERVICE_INTERROGATE = 0x00080,
            SERVICE_USER_DEFINED_CONTROL = 0x00100,

            SERVICE_ALL_ACCESS =
                (STANDARD_RIGHTS_REQUIRED | SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG | SERVICE_QUERY_STATUS | SERVICE_ENUMERATE_DEPENDENTS | SERVICE_START | SERVICE_STOP | SERVICE_PAUSE_CONTINUE
                 | SERVICE_INTERROGATE | SERVICE_USER_DEFINED_CONTROL)
        }

        public enum dwServiceType
        {
            SERVICE_KERNEL_DRIVER = 0x00000001,
            SERVICE_FILE_SYSTEM_DRIVER = 0x00000002,
            SERVICE_WIN32_OWN_PROCESS = 0x00000010,
            SERVICE_WIN32_SHARE_PROCESS = 0x00000020,
            SERVICE_INTERACTIVE_PROCESS = 0x00000100
        }

        public enum dwStartType
        {
            SERVICE_AUTO_START = 0x00000002,
            SERVICE_BOOT_START = 0x00000000,
            SERVICE_DEMAND_START = 0x00000003,
            SERVICE_DISABLED = 0x00000004,
            SERVICE_SYSTEM_START = 0x00000001
        }

        public enum dwErrorControl
        {
            SERVICE_ERROR_CRITICAL = 0x00000003,
            SERVICE_ERROR_IGNORE = 0x00000000,
            SERVICE_ERROR_NORMAL = 0x00000001,
            SERVICE_ERROR_SEVERE = 0x00000002
        }

        public enum SVCAccessRights : uint
        {
            SC_MANAGER_CONNECT = 0x00001,
            SC_MANAGER_CREATE_SERVICE = 0x00002,
            SC_MANAGER_ENUMERATE_SERVICE = 0x00004,
            SC_MANAGER_LOCK = 0x00008,
            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,

            SC_MANAGER_ALL_ACCESS =
                additionalRights.STANDARD_RIGHTS_REQUIRED | SC_MANAGER_CONNECT | SC_MANAGER_CREATE_SERVICE | SC_MANAGER_ENUMERATE_SERVICE | SC_MANAGER_LOCK | SC_MANAGER_QUERY_LOCK_STATUS
                | SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_READ = additionalRights.STANDARD_RIGHTS_READ | SC_MANAGER_ENUMERATE_SERVICE | SC_MANAGER_QUERY_LOCK_STATUS,

            GENERIC_WRITE = additionalRights.STANDARD_RIGHTS_WRITE | SC_MANAGER_CREATE_SERVICE | SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_EXECUTE = additionalRights.STANDARD_RIGHTS_EXECUTE | SC_MANAGER_CONNECT | SC_MANAGER_LOCK,

            GENERIC_ALL = SC_MANAGER_ALL_ACCESS,
        }

        public enum additionalRights : uint
        {
            //https://docs.microsoft.com/en-us/windows/win32/services/service-security-and-access-rights
            DELETE = 0x00010000,

            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,
            STANDARD_RIGHTS_ALL = 0x001F0000,
            SPECIFIC_RIGHTS_ALL = 0x0000FFFF,
            ACCESS_SYSTEM_SECURITY = 0x01000000,
            MAXIMUM_ALLOWED = 0x02000000,
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,
            DESKTOP_READOBJECTS = 0x00000001,
            DESKTOP_CREATEWINDOW = 0x00000002,
            DESKTOP_CREATEMENU = 0x00000004,
            DESKTOP_HOOKCONTROL = 0x00000008,
            DESKTOP_JOURNALRECORD = 0x00000010,
            DESKTOP_JOURNALPLAYBACK = 0x00000020,
            DESKTOP_ENUMERATE = 0x00000040,
            DESKTOP_WRITEOBJECTS = 0x00000080,
            DESKTOP_SWITCHDESKTOP = 0x00000100,
            WINSTA_ENUMDESKTOPS = 0x00000001,
            WINSTA_READATTRIBUTES = 0x00000002,
            WINSTA_ACCESSCLIPBOARD = 0x00000004,
            WINSTA_CREATEDESKTOP = 0x00000008,
            WINSTA_WRITEATTRIBUTES = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS = 0x00000040,
            WINSTA_ENUMERATE = 0x00000100,
            WINSTA_READSCREEN = 0x00000200,
            WINSTA_ALL_ACCESS = 0x0000037F
        }

        [DllImport("Mpr.dll")]
        public static extern ShareResponses WNetUseConnection
        (
            IntPtr hwndOwner,
            NETRESOURCE lpNetResource,
            string lpPassword,
            string lpUserID,
            Connect dwFlags,
            string lpAccessName,
            string lpBufferSize,
            string lpResult
        );

        [DllImport("mpr.dll")]
        public static extern ShareResponses WNetCancelConnection(string lpName, bool bForce);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateService
            (
                IntPtr hSCManager,
                string lpServiceName,
                string lpDisplayName,
                dwDesiredAccess desiredAccess,
                dwServiceType serviceType,
                dwStartType startType,
                dwErrorControl errorControl,
                string lpBinaryPathName,
                string lpLoadOrderGroup,
                IntPtr lpdwTagId,
                string lpDependencies,
                string lpServiceStartName,
                string lpPassword
            );

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager
            (
                string machineName,
                string databaseName,
                SVCAccessRights accessRights
            );

        [DllImport("advapi32", SetLastError = true)]
        public static extern bool StartService
            (
                IntPtr hService,
                int dwNumServiceArgs,
                string lpServiceArgVectors
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, dwDesiredAccess desiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DeleteService(IntPtr hService);
    }
}