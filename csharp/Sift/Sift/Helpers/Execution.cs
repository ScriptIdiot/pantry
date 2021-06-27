using Sift.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace Sift.Helpers
{
    internal class Execution
    {
        private static List<String> output = new List<string>();

        public static void Execute(List<String> targets)
        {
            if (!Creds.CheckIfCredsPresent())
            {
                Logger.Print(Logger.STATUS.GOOD, String.Format("Request shares on {0} host(s) as " + Environment.UserDomainName + "\\" + Environment.UserName, targets.Count));
                Console.WriteLine();
                foreach (String target in targets)
                {
                    if (Arguments.Jitter != 0)
                    {
                        Generic.Jitter();
                    }
                    CheckShare(target);
                }
            }
            else
            {
                Logger.Print(Logger.STATUS.GOOD, String.Format("Request shares on {0} host(s) as {1}", targets.Count, Arguments.Domain + "\\" + Arguments.Username));
                Console.WriteLine();
                IntPtr hToken = Network.Authenticate();
                if (hToken != IntPtr.Zero)
                {
                    using (WindowsImpersonationContext context = WindowsIdentity.Impersonate(hToken))
                    {
                        foreach (String target in targets)
                        {
                            Generic.Jitter();
                            CheckShare(target);
                        }
                    }
                    PInvoke.NativeMethods.CloseHandle(hToken);
                }
            }
            Console.WriteLine();
            return;
        }

        private static void CheckShare(string target)
        {
            Logger.Print(Logger.STATUS.GOOD, String.Format("Shares ({0}): ", target));
            List<String> shares = Network.EnumNetShares(target);
            foreach (string share in shares)
            {
                Logger.Print(Logger.STATUS.INFO, share);
            }
            Console.WriteLine();
            ReadDataInShares(target, shares);
            return;
        }

        private static void ReadDataInShares(String target, List<String> shares)
        {
            Logger.Print(Logger.STATUS.GOOD, "Searching for: " + String.Join(", ", Arguments.Extensions.ToArray()));

            if (Arguments.Keywords.Count > 0)
            {
                Logger.Print(Logger.STATUS.GOOD, "Searching for: " + String.Join(", ", Arguments.Keywords.ToArray()));
            }

            List<string> allFiles = new List<string>();

            foreach (string share in shares)
            {
                Logger.Print(Logger.STATUS.INFO, "Processing: " + share);
                string path = string.Format("\\\\{0}\\{1}", target, share);
                DirSearch(path);
                allFiles.AddRange(output);
            }

            if (!string.IsNullOrEmpty(Arguments.Outfile))
            {
                List<String> sorted = allFiles.Distinct().OrderBy(q => q).ToList();
                File.WriteAllLines(Arguments.Outfile, sorted);
            }

            return;
        }

        private static void DirSearch(string sDir)
        {
            Boolean useKeywords = false;
            if (Arguments.Keywords.Count > 0)
            {
                useKeywords = true;
            }

            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        if (useKeywords)
                        {
                            if (CheckExtension(f))
                            {
                                if (CheckKeywords(f))
                                {
                                    Console.WriteLine(f);
                                    output.Add(f);
                                }
                            }
                        }
                        else
                        {
                            if (CheckExtension(f))
                            {
                                Console.WriteLine(f);
                                output.Add(f);
                            }
                        }
                    }
                    DirSearch(d);
                }
            }
            catch
            {
                return;
            }

            return;
        }

        private static Boolean CheckExtension(string f)
        {
            foreach (string ext in Arguments.Extensions)
            {
                if (f.EndsWith(ext))
                {
                    return true;
                }
            }
            return false;
        }

        private static Boolean CheckKeywords(string f)
        {
            f = f.ToLower();
            foreach (string keyword in Arguments.Keywords)
            {
                if (f.Contains(keyword.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}