using Corellia.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Corellia.Utilities
{
    internal class Logger
    {
        public enum STATUS
        {
            GOOD = 0,
            ERROR = 1,
            INFO = 2,
            VERBOSE = 3,
            NOTIFY = 4
        };

        public static void Print(STATUS status, String msg, String highlight = "")
        {
            if (status == STATUS.GOOD)
            {
                Console.Write(GetCurrentTime());
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" ==> ");
                Console.ResetColor();
                Console.Write(msg);
                if (highlight != "")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(highlight);
                    Console.ResetColor();
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine();
                }
            }
            else if (status == STATUS.ERROR)
            {
                Console.Write(GetCurrentTime());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" ==> ");
                Console.ResetColor();
                Console.Write(msg);
                if (highlight != "")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(highlight);
                    Console.ResetColor();

                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine();
                }
            }
            else if (status == STATUS.INFO)
            {
                Console.Write(GetCurrentTime());
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(" --> ");
                Console.ResetColor();
                Console.Write(msg);
                if (highlight != "")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(highlight);
                    Console.ResetColor();
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine();
                }
            }
            else if (status == STATUS.VERBOSE)
            {
                if (Config.Verbose)
                {
                    Console.Write(GetCurrentTime());
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(" ==> ");
                    Console.ResetColor();
                    Console.Write(msg);
                    if (highlight != "")
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(highlight);
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine();
                    }
                }
            }
            else if (status == STATUS.NOTIFY)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("[*] ");
                Console.ResetColor();
                Console.Write(msg);
                if (highlight != "")
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(highlight);
                    Console.ResetColor();
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine();
                }
            }
        }

        public static String GetLastWindowsError()
        {
            Win32Exception win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
            return string.Format("{0} ({1})", win32Exception.Message, win32Exception.NativeErrorCode);
        }

        private static String GetCurrentTime()
        {
            return "[" + DateTime.Now.ToString("HH:mm:ss") + "]";
        }

        public static void PrintSPNs(List<SPN> spns)
        {
            foreach (SPN spn in spns)
            {
                Console.Write("--> ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Username: ");
                Console.ResetColor();
                Console.WriteLine(spn.Username);

                Console.Write("--> ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Domain: ");
                Console.ResetColor();
                Console.WriteLine(spn.Domain);

                Console.Write("--> ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Service Principal Name: ");
                Console.ResetColor();
                Console.WriteLine(spn.ServicePrincipalName);

                Console.Write("--> ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Encryption Type: ");
                Console.ResetColor();
                Console.WriteLine(spn.EncType);

                Console.Write("--> ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Hash: ");
                Console.ResetColor();
                Console.WriteLine(spn.Hash);

                Console.WriteLine();
            }
            Console.Write("--> ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("SPNs Found: ");
            Console.ResetColor();
            Console.WriteLine(spns.Count);

            Console.WriteLine();

            return;
        }
    }
}