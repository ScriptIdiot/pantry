using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Corellia.PInvoke
{
    public class NativeMethods
    {
        [DllImport("netapi32", CharSet = CharSet.Unicode)]
        public static extern int NetShareEnum
            (
                string lpServerName,
                int dwLevel,
                out IntPtr lpBuffer,
                int dwPrefMaxLen,
                out int entriesRead,
                out int totalEntries,
                ref int hResume
            );

        [DllImport("Netapi32.dll", SetLastError = true)]
        public static extern int NetApiBufferFree(IntPtr Buffer);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool LogonUser
        (
            String pszUserName,
            String pszDomain,
            String pszPassword,
            Enums.DwLogonType logonType,
            Enums.DwLogonProvider logonProvider,
            ref IntPtr phToken
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetComputerName
        (
            StringBuilder lpBuffer,
            ref int nSize
        );

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int NetGetJoinInformation(string server, out IntPtr domain, out Enums.NetJoinStatus status);
    }
}