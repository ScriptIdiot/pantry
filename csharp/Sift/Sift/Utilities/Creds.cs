using Sift.Helpers;
using System;

namespace Sift.Utilities
{
    internal class Creds
    {
        public static Boolean CheckIfCredsPresent()
        {
            if (String.IsNullOrEmpty(Arguments.Domain) && String.IsNullOrEmpty(Arguments.Username) && String.IsNullOrEmpty(Arguments.Password))
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