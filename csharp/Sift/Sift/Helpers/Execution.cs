using Sift.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Sift.Helpers
{
    internal class Execution
    {
        private static readonly List<String> output = new List<string>();

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
            List<String> shares = new List<string>();

            if (Arguments.IncludeShares.Count > 0)
            {
                shares = Arguments.IncludeShares;
            }
            else
            {
                shares = Network.EnumNetShares(target);

            }

            for (int i = 0; i < shares.Count; i++)
            {
                foreach(string eshare in Arguments.ExcludeShares)
                {
                    if(eshare.ToLower().Equals(shares[i].ToLower()))
                    {
                        Logger.Print(Logger.STATUS.INFO, "Skipping: " + shares[i]);
                        shares.RemoveAt(i);
                    }
                }
            }

            if(shares.Count == 0)
            {
                Logger.Print(Logger.STATUS.ERROR, "No shares to look at!");
                return;
            }

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

            Console.WriteLine();

            List<string> allFiles = new List<string>();

            Parallel.ForEach(shares, share =>
            {
                string path = string.Format("\\\\{0}\\{1}", target, share);
                DirSearch(path);
                allFiles.AddRange(output);
            });

            if (!string.IsNullOrEmpty(Arguments.Outfile))
            {
                List<String> sorted = allFiles.Distinct().OrderBy(q => q).ToList();
                File.WriteAllLines(Arguments.Outfile, sorted);
            }

            return;
        }

        private static void DirSearch(string sDir)
        {
            // This: https://docs.microsoft.com/en-US/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree#robust-programming
            Stack<string> dirs = new Stack<string>(20);

            Boolean useKeywords = false;
            if (Arguments.Keywords.Count > 0)
            {
                useKeywords = true;
            }

            if (!Directory.Exists(sDir))
            {
                throw new ArgumentException();
            }
            dirs.Push(sDir);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch
                {
                    continue;
                }

                string[] files;
                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch
                {
                    continue;
                }

                foreach (string file in files)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(file);
                        string f = fi.FullName;
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
                    catch
                    {
                        continue;
                    }
                }
                foreach (string str in subDirs)
                    dirs.Push(str);
            }

            return;
        }

        private static Boolean CheckExtension(string f)
        {
            foreach (string ext in Arguments.Extensions)
            {
                if (f.ToLower().EndsWith(ext.ToLower()))
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
                if (f.ToLower().Contains(keyword.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}