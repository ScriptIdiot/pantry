using Corellia.Utilities;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace Corellia.Network
{
    internal class LDAP
    {
        public static string DetermineLDAPServer()
        {
            string server;
            if (String.IsNullOrEmpty(Config.DomainController))
            {
                server = Config.Domain;
            }
            else
            {
                server = Config.DomainController;
            }
            return server;
        }

        public static Int32 VerifyPasswordLength()
        {
            Int32 minpwdlength = 0;
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, Config.Domain))
                {
                    using (DirectoryEntry domainEntry = new DirectoryEntry("LDAP://" + Config.DomainController, Config.Username, Config.Password))
                    {
                        minpwdlength = Convert.ToInt32(domainEntry.Properties["minpwdlength"].Value);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.ERROR, e.Message);
            }
            return minpwdlength;
        }

        public static bool Authenticate(string user, string spray)
        {
            bool status;
            try
            {
                using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, Config.Domain))
                {
                    status = principalContext.ValidateCredentials(user, spray, ContextOptions.Negotiate);
                }
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.VERBOSE, "Error: ", e.Message);
                status = false;
            }
            return status;
        }

        public static Structure.Spray GetUsers()
        {
            List<String> users = new List<String>();
            List<String> skipped = new List<String>();
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, Config.Domain))
                {
                    using (DirectoryEntry domainEntry = new DirectoryEntry("LDAP://" + Config.DomainController, Config.Username, Config.Password))
                    {
                        DirectorySearcher directorySearcher = new DirectorySearcher(domainEntry)
                        {
                            Filter = "(&(objectCategory=Person)((sAMAccountName=*)(badPwdCount=*))(!(userAccountControl:1.2.840.113556.1.4.803:=2)))",
                            PageSize = 1000
                        };
                        directorySearcher.PropertiesToLoad.Add("sAMAccountName");
                        directorySearcher.PropertiesToLoad.Add("badPwdCount");
                        directorySearcher.SearchScope = SearchScope.Subtree;
                        SearchResultCollection results = directorySearcher.FindAll();
                        if (results != null)
                        {
                            foreach (SearchResult result in results)
                            {
                                String user = "";
                                try
                                {
                                    user = result.Properties["sAMAccountName"][0].ToString();
                                    Int32 badPwdCount = Convert.ToInt32(result.Properties["badPwdCount"][0].ToString());

                                    if (!String.IsNullOrEmpty(user))
                                    {
                                        if (badPwdCount >= 2)
                                        {
                                            Logger.Print(Logger.STATUS.VERBOSE, user + " has >= 2 bad password attempts");
                                            if (!skipped.Contains(user))
                                            {
                                                skipped.Add(user);
                                            }
                                        }
                                        else
                                        {
                                            if (!users.Contains(user))
                                            {
                                                users.Add(user);
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Print(Logger.STATUS.VERBOSE, "Failed to get username: ", e.Message);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.VERBOSE, "Failed getting users from LDAP: ", e.Message);
            }
            if (users.Count > 1)
            {
                users = users.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            }
            return new Structure.Spray
            {
                UsersList = users,
                SkippedList = skipped
            };
        }

        public static List<Structure.User> GetUserDescriptions()
        {
            List<Structure.User> data = new List<Structure.User>();
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, Config.Domain))
                {
                    using (DirectoryEntry domainEntry = new DirectoryEntry("LDAP://" + Config.DomainController, Config.Username, Config.Password))
                    {
                        DirectorySearcher directorySearcher = new DirectorySearcher(domainEntry)
                        {
                            Filter = "(&(objectCategory=Person)((sAMAccountName=*))(!(userAccountControl:1.2.840.113556.1.4.803:=2)))",
                            PageSize = 1000
                        };
                        directorySearcher.PropertiesToLoad.Add("sAMAccountName");
                        directorySearcher.PropertiesToLoad.Add("badPwdCount");
                        directorySearcher.PropertiesToLoad.Add("description");
                        directorySearcher.SearchScope = SearchScope.Subtree;
                        SearchResultCollection results = directorySearcher.FindAll();
                        if (results != null)
                        {
                            foreach (SearchResult result in results)
                            {
                                ResultPropertyValueCollection descriptionCollection = result.Properties["description"];
                                string description;
                                string userName;
                                try
                                {
                                    description = descriptionCollection[0].ToString();
                                    userName = result.Properties["sAMAccountName"][0].ToString();
                                }
                                catch
                                {
                                    continue;
                                }

                                if (descriptionCollection.Count > 0)
                                {
                                    Structure.User user = new Structure.User
                                    {
                                        Username = userName,
                                        Description = description
                                    };
                                    if (!data.Contains(user))
                                    {
                                        data.Add(user);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.VERBOSE, "Failed getting users from LDAP: ", e.Message);
            }
            return data;
        }
    }
}