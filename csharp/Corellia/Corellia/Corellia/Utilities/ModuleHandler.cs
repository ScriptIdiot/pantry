using System;
using System.Collections.Generic;

namespace Corellia.Utilities
{
    internal class ModuleHandler
    {
        public static bool ExecuteDomain()
        {
            if (!Config.SetDomainName())
            {
                return false;
            }
            Logger.Print(Logger.STATUS.NOTIFY, "Domain Name: " + Config.Domain);

            if (!Config.SetDomainController())
            {
                return false;
            }

            Logger.Print(Logger.STATUS.NOTIFY, "Domain Controller: " + Config.DomainController);

            if (Config.CredsPresent())
            {
                Logger.Print(Logger.STATUS.NOTIFY, string.Format("Authentication: {0}\\{1}:{2}", Config.Domain, Config.Username, Config.Password));
            }
            else
            {
                Logger.Print(Logger.STATUS.NOTIFY, string.Format("Authentication: {0}\\{1}", Config.Domain, Environment.UserName));
            }

            Console.WriteLine();

            List<Structure.SPN> spns = new List<Structure.SPN>();
            List<Structure.GPPPassword> gpppasswords = new List<Structure.GPPPassword>();
            List<Structure.User> descriptions = new List<Structure.User>();

            if (Config.Kerberoast)
            {
                Logger.Print(Logger.STATUS.NOTIFY, "Kerberoasting: Enabled");
                Console.WriteLine();
                spns = Modules.Kerberoast.Roast();
            }

            if (Config.GPPPassword)
            {
                Logger.Print(Logger.STATUS.NOTIFY, "GPPassword: Enabled");
                Console.WriteLine();
                gpppasswords = Modules.GPPPassword.GetGPPPassword();
            }

            if (Config.LDAPDescriptions)
            {
                Logger.Print(Logger.STATUS.NOTIFY, "User Descriptions: Enabled");
                Console.WriteLine();
                descriptions = Modules.LDAPDescriptions.GetLDAPDescriptions();
            }

            if (!String.IsNullOrEmpty(Config.Spray))
            {
                Logger.Print(Logger.STATUS.NOTIFY, "Spraying: " + Config.Spray);
                Console.WriteLine();
                Modules.PassSpray.Spray();
            }
            HTMLBuilder.BuildReport(spns, gpppasswords, descriptions);
            return true;
        }
    }
}