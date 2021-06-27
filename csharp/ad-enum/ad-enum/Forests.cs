using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace ad_enum
{
    class Forests
    {
        public static String CurrentDomain()
        {
            Console.WriteLine("[*] Looking for the current Domain!");
            String domain = "";
            if (IsInDomain())
            {
                domain = IPGlobalProperties.GetIPGlobalProperties().DomainName.ToString(); // dont really like grabbing the DNS Suffix tbh
                Console.WriteLine("[+] " + domain);
            }
            else
            {
                domain = "WORKGROUP"; // If its not, set it to WORKGROUP to keep my sanity.
                Console.WriteLine("[!] Couldnt find domain, using " + domain);
            }
            return domain;
        }

        public static String CurrentForest()
        {
            Console.WriteLine("[*] Looking for the current Forest!");

            String F = "";
            try
            {
                F = Forest.GetCurrentForest().ToString();
                Console.WriteLine("[+] " + F);
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] " + e.Message);
            }

            return F;
        }

        public static String CurrentDomainFQDN(String DomainName)
        {
            Console.WriteLine("[*] Looking for Domain FQDN!");
            string D = "";
            try {
                DirectoryContext objContext = new DirectoryContext(DirectoryContextType.Domain, DomainName);
                Domain objDomain = Domain.GetDomain(objContext);
                D = objDomain.Name;
                Console.WriteLine("[+] " + D);
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] " + e.Message);
            }
            return D;
        }

        public static List<String> AllDomains()
        {
            Console.WriteLine("[*] Looking for ALL Domains!");
            List<String> Domains = new List<String>();
            DomainCollection myDomains;

            try
            {
                Forest currentForest = Forest.GetCurrentForest();
                myDomains = currentForest.Domains;
            }
            catch(Exception e) 
            {
                Console.WriteLine("[!] " + e.Message);
                return Domains;
            }

            foreach (Domain objDomain in myDomains)
            {
                try
                {
                    String D = objDomain.Name.ToString();
                    if (!Domains.Contains(D))
                    {
                        Domains.Add(D);
                        Console.WriteLine("[+] " + D);
                    }
                }
                catch (Exception e)
                {
                    continue;
                }

            }
            return Domains;
        }

        public static List<String> DomainControllers()
        {
            Console.WriteLine("[*] Looking for Domain Controllers!");
            List<String> Controllers = new List<String>();

            try
            {
                Domain domain = Domain.GetCurrentDomain();
                foreach (DomainController dc in domain.DomainControllers)
                {
                    String domaincontroller = dc.Name;
                    if (!Controllers.Contains(domaincontroller))
                    {
                        Controllers.Add(domaincontroller);
                        Console.WriteLine("[+] " + domaincontroller);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("[!] " + e.Message);
            }
            return Controllers;
        }

        public static void Trusts()
        {
            Console.WriteLine("[*] Looking for Trusts!");
            try
            {

                using (Forest forest = Forest.GetCurrentForest())
                {
                    Console.WriteLine("[+] Forest: " + forest.Name);
                    Console.WriteLine("[+] Domains: ");

                    foreach (Domain domain in forest.Domains)
                    {
                        Console.WriteLine(String.Format("   -> " + domain.Name));
                        Console.WriteLine("[*] Checking Trusts!");
                        TrustRelationshipInformationCollection domainTrusts = domain.GetAllTrustRelationships();
                        if (domainTrusts.Count == 0)
                        {
                            Console.WriteLine("[!] None found!");
                        }
                        else
                        {
                            Console.WriteLine("[+] Trusts identified!");
                            foreach (TrustRelationshipInformation trust in domainTrusts)
                            {
                                DirectoryContext x = new DirectoryContext(DirectoryContextType.Domain, trust.TargetName);
                                Console.WriteLine(String.Format("[+] {0} -> {1} ({2}", trust.SourceName, trust.TargetName, trust.TrustType.ToString()));
                            }
                        }
                        domain.Dispose();
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("[!] " + e.Message);
            }
        }

        private static bool IsInDomain()
        {
            Win32.NetJoinStatus status = Win32.NetJoinStatus.NetSetupUnknownStatus;
            IntPtr pDomain = IntPtr.Zero;
            int result = Win32.NetGetJoinInformation(null, out pDomain, out status);
            if (pDomain != IntPtr.Zero)
            {
                Win32.NetApiBufferFree(pDomain);
            }
            if (result == Win32.ErrorSuccess)
            {
                return status == Win32.NetJoinStatus.NetSetupDomainName;
            }
            else
            {
                throw new Exception("Domain Info Get Failed", new Win32Exception());
            }
        }
        internal class Win32
        {
            public const int ErrorSuccess = 0;

            [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int NetGetJoinInformation(string server, out IntPtr domain, out NetJoinStatus status);

            [DllImport("Netapi32.dll")]
            public static extern int NetApiBufferFree(IntPtr Buffer);

            public enum NetJoinStatus
            {
                NetSetupUnknownStatus = 0,
                NetSetupUnjoined,
                NetSetupWorkgroupName,
                NetSetupDomainName
            }
        }
    }
}
