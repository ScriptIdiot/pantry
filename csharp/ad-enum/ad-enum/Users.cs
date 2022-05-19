using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace ad_enum
{
    class Users
    {
        public static List<String> Get(String Domain)
        {
            Console.WriteLine("[*] Starting User Enumeration!");

            List<String> Users = new List<String>();
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, Domain))
                {
                    using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                    {
                        foreach (var r in searcher.FindAll())
                        {
                            DirectoryEntry d = r.GetUnderlyingObject() as DirectoryEntry;
                            var UserDE = d.Properties["userPrincipalName"].Value;
                            if (UserDE != null)
                            {
                                String User = UserDE.ToString();
                                if (!Users.Contains(User))
                                {
                                    Console.WriteLine("[+] " + User);
                                    Users.Add(User);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("[!] " + e.Message);
            }
            return Users;
        }
    }
}
