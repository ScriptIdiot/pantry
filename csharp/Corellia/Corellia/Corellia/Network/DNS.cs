using Corellia.Utilities;
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Corellia.Network
{
    internal class DNS
    {
        public static string GetHostnameFromIP(string IP)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(IP);

                return hostEntry.HostName.ToUpper();
            }
            catch
            {
                return "";
            }
        }

        public static string GetIPFromHostname(string hostname)
        {
            try
            {
                string ip = Dns.GetHostAddresses(hostname)[0].ToString();
                return ip;
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.ERROR, "Failed to find IP of Domain: " + e.Message);
                return "";
            }
        }

        public static String GetCurrentDomain()
        {
            string domain = "";
            try
            {
                if (IsInDomain())
                {
                    domain = IPGlobalProperties.GetIPGlobalProperties().DomainName.ToString();
                }
                else
                {
                    domain = "WORKGROUP";
                }
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.ERROR, e.Message);
            }
            return domain;
        }

        public static bool IsInDomain()
        {
            int result = PInvoke.NativeMethods.NetGetJoinInformation(null, out IntPtr pDomain, out _);
            if (pDomain != IntPtr.Zero)
            {
                PInvoke.NativeMethods.NetApiBufferFree(pDomain);
            }
            if (result == PInvoke.Consts.SUCCESS)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}