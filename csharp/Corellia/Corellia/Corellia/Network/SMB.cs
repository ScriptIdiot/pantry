using Corellia.PInvoke;
using Corellia.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Corellia.Network
{
    internal class SMB
    {
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
                            msg = si.NetName;
                            if (!output.Contains(msg))
                            {
                                output.Add(msg);
                            }
                            int nStructSize = Marshal.SizeOf(typeof(Structs.SHARE_INFO_1));
                            currentPtr += nStructSize;
                        }
                        else
                        {
                            Structs.SHARE_INFO_2 si = (Structs.SHARE_INFO_2)Marshal.PtrToStructure(currentPtr, typeof(Structs.SHARE_INFO_2));
                            Logger.Print(Logger.STATUS.VERBOSE, "Share: ", si.shi2_netname);
                            string msg = "";
                            msg = si.shi2_netname;
                            if (!output.Contains(msg))
                            {
                                output.Add(msg);
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

        public static List<String> GetPathTotalACLs(string target, string share)
        {
            string path = string.Format(@"\\{0}\{1}", target, share);
            List<string> acls = new List<string>();
            try
            {
                AuthorizationRuleCollection rules = Directory.GetAccessControl(path).GetAccessRules(true, true, typeof(SecurityIdentifier));
                foreach (AuthorizationRule rule in rules)
                {
                    var filesystemAccessRule = (FileSystemAccessRule)rule;
                    string srule = filesystemAccessRule.FileSystemRights.ToString();
                    if (srule == "268435456")
                    {
                        srule = "FullControl";
                    }
                    else if (srule == "-536805376")
                    {
                        srule = "Modify, Synchronize";
                    }
                    else if (srule == "-1610612736")
                    {
                        srule = "ReadAndExecute, Synchronize";
                    }

                    if (!acls.Contains(srule))
                    {
                        acls.Add(srule);
                    }
                }
                if (rules.Count != 0)
                {
                    Logger.Print(Logger.STATUS.VERBOSE, "ACL Count: ", rules.Count.ToString());
                }
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.VERBOSE, "Error getting ACL for " + path + ": ", e.Message);
            }
            return acls;
        }
    }
}