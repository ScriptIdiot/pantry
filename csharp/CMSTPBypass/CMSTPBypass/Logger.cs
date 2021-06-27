using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CMSTPBypass
{
    class Logger
    {
        public enum STATUS
        {
            GOOD = 0,
            ERROR = 1,
            INFO = 2,
            DEBUG = 3,
            TITLE = 4,
        };

        public static void Print(STATUS status, String msg)
        {
            if (status == STATUS.GOOD)
            {
                Console.WriteLine("[+] " + msg);
            }
            else if (status == STATUS.ERROR)
            {
                Console.WriteLine("[!] " + msg);
            }
            else if (status == STATUS.INFO)
            {
                Console.WriteLine("[*] " + msg);
            }
            else if (status == STATUS.DEBUG)
            {
                Console.WriteLine("\t|-> " + msg);
            }
            else if (status == STATUS.TITLE)
            {
                Console.WriteLine("|--> " + msg);
            }
        }
        public static String GetLastWindowsError()
        {
            Win32Exception win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
            return string.Format("{0} ({1})", win32Exception.Message, win32Exception.NativeErrorCode);
        }
    }
}