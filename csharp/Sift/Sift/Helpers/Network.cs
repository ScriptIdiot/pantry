using Sift.PInvoke;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Sift.Helpers
{
    internal class Network
    {
        public static List<String> GetTargetsWithOpenPorts(List<String> targets)
        {
            Logger.Print(Logger.STATUS.GOOD, String.Format("Connecting to SMB on {0} host(s):", targets.Count));
            List<String> targetsWithSmb = new List<String>();

            foreach (string target in targets)
            {
                Generic.Jitter();
                if (Send(target))
                {
                    if (!targetsWithSmb.Contains(target))
                    {
                        String hostname = GetHostname(target);
                        if (!String.IsNullOrEmpty(hostname))
                        {
                            Logger.Print(Logger.STATUS.INFO, String.Format("{0} ({1}): ", target, hostname), "OPEN");
                        }
                        else
                        {
                            Logger.Print(Logger.STATUS.INFO, target + ": ", "OPEN");
                        }
                        targetsWithSmb.Add(target);
                    }
                }
            }
            Console.WriteLine();
            return targetsWithSmb;
        }

        public static List<String> EnumNetShares(string target)
        {
            List<String> output = new List<String>();
            int level = 2;
            int hResume = 0;
            IntPtr pBuffer = IntPtr.Zero;

            try
            {
                Logger.Print(Logger.STATUS.VERBOSE, "Accessing shares on: ", target);
                int nRet = NativeMethods.NetShareEnum(target, level, out pBuffer, -1, out int entriesRead, out int entriesread, ref hResume);
                Logger.Print(Logger.STATUS.VERBOSE, "NetShareEnum() returned: ", nRet.ToString());
                if (nRet == Consts.ERROR_ACCESS_DENIED)
                {
                    level = 1;
                    Logger.Print(Logger.STATUS.VERBOSE, "Dropping access level to 1");
                    nRet = NativeMethods.NetShareEnum(target, level, out pBuffer, -1, out entriesRead, out entriesread, ref hResume);
                    Logger.Print(Logger.STATUS.VERBOSE, "NetShareEnum()'s second attempt returned: ", nRet.ToString());
                }

                if (Consts.NO_ERROR == nRet && entriesRead > 0)
                {
                    Logger.Print(Logger.STATUS.VERBOSE, "NetShareEnum() == NO_ERROR");
                    Type t = (2 == level) ? typeof(Structs.SHARE_INFO_2) : typeof(Structs.SHARE_INFO_1);
                    int offset = Marshal.SizeOf(t);
                    IntPtr currentPtr = pBuffer;
                    for (int i = 0; i < entriesread; i++)
                    {
                        if (1 == level)
                        {
                            Structs.SHARE_INFO_1 si = (Structs.SHARE_INFO_1)Marshal.PtrToStructure(currentPtr, typeof(Structs.SHARE_INFO_1));
                            Logger.Print(Logger.STATUS.VERBOSE, "Share: ", si.NetName);
                            string msg = "";

                            if (!output.Contains(si.NetName))
                            {
                                output.Add(msg);
                            }
                            int nStructSize = Marshal.SizeOf(typeof(Structs.SHARE_INFO_1));
                            currentPtr += nStructSize;
                        }
                        else
                        {
                            Structs.SHARE_INFO_2 si = (Structs.SHARE_INFO_2)Marshal.PtrToStructure(currentPtr, typeof(PInvoke.Structs.SHARE_INFO_2));
                            Logger.Print(Logger.STATUS.VERBOSE, "Share: ", si.shi2_netname);
                            if (!output.Contains(si.shi2_netname))
                            {
                                output.Add(si.shi2_netname);
                            }
                            int nStructSize = Marshal.SizeOf(typeof(Structs.SHARE_INFO_2));
                            currentPtr += nStructSize;
                        }
                    }
                }
                else
                {
                    Logger.Print(Logger.STATUS.VERBOSE, "NetShareEnum() == ", Logger.GetLastWindowsError());
                }
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.VERBOSE, "Error from " + target + ": ", e.Message);
            }
            finally
            {
                if (IntPtr.Zero != pBuffer)
                {
                    NativeMethods.NetApiBufferFree(pBuffer);
                }
            }
            return output;
        }

        private static Boolean Send(String target)
        {
            using (TcpClient Scan = new TcpClient())
            {
                try
                {
                    Scan.Connect(target, 445);
                    return true;
                }
                catch (Exception e)
                {
                    Logger.Print(Logger.STATUS.VERBOSE, "Error from " + target + ": ", e.Message);
                    return false;
                }
            }
        }

        private static string GetHostname(string target)
        {
            int maxCapacity = Consts.MAX_COMPUTERNAME_LENGTH + 1;
            StringBuilder nameBuilder = new StringBuilder(maxCapacity, maxCapacity);
            Boolean bGetComputerName = NativeMethods.GetComputerName(nameBuilder, ref maxCapacity);

            if (bGetComputerName)
            {
                return nameBuilder.ToString();
            }
            else
            {
                Logger.Print(Logger.STATUS.VERBOSE, "Error getting hostname for " + target + ": ", Logger.GetLastWindowsError());
                return "";
            }
        }

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

        public static List<string> GetAdComputers()
        {
            List<string> ComputerNames = new List<string>();

            try
            {
                DirectoryEntry entry = new DirectoryEntry(Arguments.Domain);
                if (Utilities.Creds.CheckIfCredsPresent())
                {
                    entry.Username = Arguments.Username;
                    entry.Password = Arguments.Password;
                }
                DirectorySearcher mySearcher = new DirectorySearcher(entry)
                {
                    Filter = ("(objectClass=computer)"),
                    SizeLimit = int.MaxValue,
                    PageSize = int.MaxValue
                };

                foreach (SearchResult resEnt in mySearcher.FindAll())
                {
                    string ComputerName = resEnt.GetDirectoryEntry().Name;
                    if (ComputerName.StartsWith("CN="))
                    {
                        ComputerName = ComputerName.Remove(0, "CN=".Length);
                    }

                    ComputerNames.Add(ComputerName);
                }

                mySearcher.Dispose();
                entry.Dispose();
                return ComputerNames;
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.ERROR, "Failed to get AD Computers: " + e.Message);
                return ComputerNames;
            }
        }
    }
}