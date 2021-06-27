namespace Corellia.Utilities
{
    public class Config
    {
        public static string DomainController { get; set; }
        public static string Domain { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static string Spray { get; set; }
        public static bool Kerberoast { get; set; }
        public static bool GPPPassword { get; set; }
        public static bool LDAPDescriptions { get; set; }
        public static bool Verbose { get; set; }
        public static Interfaces.Enums.Mode Mode { get; set; }

        public static bool SetDomainController()
        {
            Config.DomainController = Network.DNS.GetIPFromHostname(Config.Domain);
            if (!string.IsNullOrEmpty(Config.DomainController))
            {
                return true;
            }
            else
            {
                Config.DomainController = Network.DNS.GetIPFromHostname(Config.Domain);
                if (string.IsNullOrEmpty(Config.DomainController))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool SetDomainName()
        {
            if (string.IsNullOrEmpty(Config.Domain))
            {
                Config.Domain = Network.DNS.GetCurrentDomain();
                if (string.IsNullOrEmpty(Config.Domain))
                {
                    return false;
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Check if the creds are correctly set because all three are required if authentication is being used
        /// </summary>
        /// <returns>Returns a bool. true == all consistent, false if not.</returns>
        public static bool CheckCredArgs()
        {
            // If any of these strings are set
            if (!string.IsNullOrEmpty(Config.Domain) || !string.IsNullOrEmpty(Config.Username) || !string.IsNullOrEmpty(Config.Password))
            {
                // make sure theyre all set
                if (string.IsNullOrEmpty(Config.Domain) || string.IsNullOrEmpty(Config.Username) || string.IsNullOrEmpty(Config.Password))
                {
                    // return false because something is null
                    return false;
                }
            }
            // return true because theyre all on or off
            return true;
        }

        /// <summary>
        /// Check if creds are being used. This is primarily used to determine whether or not impersonation will be used
        /// </summary>
        /// <returns>Returns a bool. True if creds are used, false if not.</returns>
        public static bool CredsPresent()
        {
            if (string.IsNullOrEmpty(Config.Username) && string.IsNullOrEmpty(Config.Password))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}