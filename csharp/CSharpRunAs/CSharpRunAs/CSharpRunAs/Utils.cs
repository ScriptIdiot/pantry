using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpRunAs
{
    class Utils
    {
        public static void ErrorMsg()
        {
            Win32Exception errorMessage = new Win32Exception(Marshal.GetLastWin32Error());
            Console.WriteLine("[!] " + String.Format("{0} (Error Code: {1})", errorMessage.Message, errorMessage.NativeErrorCode.ToString()));
        }
        public static Int32 CurrentPid()
        {
            return (Int32)Process.GetCurrentProcess().Id;
        }
        public static Boolean CheckPID(Int32 PID)
        {
            Process p = Process.GetProcessById(PID);
            if (p.HasExited == false)
            {
                Console.WriteLine("[+] PID " + PID + " is running!");
                Console.WriteLine("[+] Start Time: " + p.StartTime);
                return true;
            }
            else
            {
                Console.WriteLine("[!] PID" + PID + "is not running!");
                return false;
            }
        }
        public static Dictionary<String,String> GetArgs(string[] args)
        {
            Dictionary<String, String> Arguments = new Dictionary<String, String>();
            if(args.Length == 4)
            {
                Arguments.Add("Domain", args[0]);
                Arguments.Add("Username", args[1]);
                Arguments.Add("Password", args[2]);
                Arguments.Add("ExePath", args[3]);
            }
            return Arguments;
        }
    }
}
