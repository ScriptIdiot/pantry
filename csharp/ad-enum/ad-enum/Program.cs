using System;
using System.Collections.Generic;

namespace ad_enum
{
    class Program
    {
        static void Main(string[] args)
        {

            String Domain = Forests.CurrentDomain();
            Console.WriteLine();

            String DomainFQDN = Forests.CurrentDomainFQDN(Domain);

            Console.WriteLine();

            String Forest = Forests.CurrentForest();

            Console.WriteLine();

            List<String> AllDomains = Forests.AllDomains();

            Console.WriteLine();

            List<String> DomainControllers = Forests.DomainControllers();

            Console.WriteLine();

            Forests.Trusts();

            Console.WriteLine();

            List<String> EnumeratedUsers = Users.Get(Domain);
            
            Console.WriteLine();

            Dictionary<String, String> EnumeratedComputers = Computers.Get(Domain);

            Console.WriteLine();
        }
    }
}