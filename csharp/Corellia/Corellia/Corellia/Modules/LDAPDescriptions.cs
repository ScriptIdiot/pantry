using Corellia.Interfaces;
using Corellia.Utilities;
using System;
using System.Collections.Generic;

namespace Corellia.Modules
{
    public class LDAPDescriptions : IModules
    {
        public Enums.Mode Mode => Enums.Mode.Domain;

        public static List<Structure.User> GetLDAPDescriptions()
        {
            List<Structure.User> data = Network.LDAP.GetUserDescriptions();
            foreach (Structure.User user in data)
            {
                Logger.Print(Logger.STATUS.GOOD, user.Username + ": " + user.Description);
            }
            Console.WriteLine();
            return data;
        }
    }
}