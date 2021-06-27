using CSharpRunAs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharpRunAs
{
    class ImpersonationManager
    {
        public static void Execute(Dictionary<String,String> Arguments)
        {
            IntPtr hLogonUser = IntPtr.Zero;
            Boolean isLoggedOn = NativeMethods.LogonUser
                (
                    Arguments["Username"],
                    Arguments["Domain"],
                    Arguments["Password"],
                    NativeMethods.dwLogonType.LOGON32_LOGON_NETWORK,
                    NativeMethods.dwLogonProvider.LOGON32_PROVIDER_DEFAULT,
                    ref hLogonUser
                );

            Console.WriteLine("[*] Attempting to get a token for the specified user...");

            if (isLoggedOn)
            {
                Console.WriteLine("[+] Got Token: " + hLogonUser.ToInt32());

                NativeMethods.SECURITY_ATTRIBUTES securityAttribs = new NativeMethods.SECURITY_ATTRIBUTES();
                securityAttribs.bInheritHandle = true;
                securityAttribs.Length = Marshal.SizeOf(securityAttribs);
                securityAttribs.lpSecurityDescriptor = IntPtr.Zero;

                NativeMethods.ProcessInformation processInformation = new NativeMethods.ProcessInformation();
                NativeMethods.StartupInformation startupInformation = new NativeMethods.StartupInformation();

                IntPtr hTokenDuplicate = IntPtr.Zero;

                Console.WriteLine("[+] Attempting to duplicate the token...");

                Boolean isDuped = NativeMethods.DuplicateTokenEx
                    (
                        hLogonUser,
                        NativeMethods.GENERIC_ALL,
                        ref securityAttribs,
                        NativeMethods.SECURITY_IMPERSONATION_LEVEL.SecurityDelegation,
                        NativeMethods.TOKEN_TYPE.TokenPrimary,
                        ref hTokenDuplicate
                    );
                if (isDuped)
                {
                    Console.WriteLine("[+] Got Token: " + hTokenDuplicate.ToInt32());
                    Boolean isCreated = NativeMethods.CreateProcessWithTokenW
                        (
                            hTokenDuplicate,
                            0,
                            Arguments["ExePath"],
                            null,
                            NativeMethods.dwCreationFlags.CREATE_NO_WINDOW,
                            IntPtr.Zero,
                            null,
                            ref startupInformation,
                            out processInformation
                        );
                    if (isCreated)
                    {
                        Int32 newPID = (Int32)processInformation.dwProcessId;
                        Console.WriteLine("[+] New Process PID: " + processInformation.dwProcessId);
                        Utils.CheckPID(newPID);
                    }
                    else
                    {
                        Console.WriteLine("[!] Got error from: CreateProcessWithTokenW()");
                        Utils.ErrorMsg();
                    }
                }
                else
                {
                    Console.WriteLine("[!] Got error from: DuplicateTokenEx()");
                    Utils.ErrorMsg();
                }
            }
            else
            {
                Console.WriteLine("[!] Got error from: LogonUser()");
                Utils.ErrorMsg();
            }
        }
    }
}
