using Corellia.PInvoke;
using System;

namespace Corellia.Utilities
{
    internal class Authenticate
    {
        public static IntPtr SetLogonToken()
        {
            IntPtr hToken = IntPtr.Zero;
            bool bLogonUser = NativeMethods.LogonUser
                (
                    Config.Username,
                    Config.Domain,
                    Config.Password,
                    Enums.DwLogonType.LOGON32_LOGON_NEW_CREDENTIALS,
                    Enums.DwLogonProvider.LOGON32_PROVIDER_WINNT50,
                    ref hToken
                );

            if (!bLogonUser)
            {
                Logger.Print(Logger.STATUS.VERBOSE, "LogonUser(): ", String.Format("FAILURE ({0})", Logger.GetLastWindowsError()));
            }
            Logger.Print(Logger.STATUS.VERBOSE, "LogonUser() == ", Logger.GetLastWindowsError());
            Logger.Print(Logger.STATUS.VERBOSE, "hToken: " + hToken.ToInt64());
            return hToken;
        }
    }
}