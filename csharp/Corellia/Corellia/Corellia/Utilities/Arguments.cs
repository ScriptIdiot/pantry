using System;
using System.Collections.Generic;
using System.Linq;

namespace Corellia.Utilities
{
    internal class Arguments
    {
        private static Boolean parseError = false;

        public static bool ParseArguments(string[] args)
        {
            Dictionary<String, String> parsedArgs = GetDictionaryFromArguments(args);
            if (parsedArgs.Count == 0)
            {
                return false;
            }
            foreach (KeyValuePair<String, String> argument in parsedArgs)
            {
                if (argument.Key == "-h" || argument.Key == "--help")
                {
                    Help();
                    return false;
                }
                if (argument.Key == "-domaincontroller" || argument.Key == "-dc")
                {
                    Config.DomainController = argument.Value.Replace("\"", "");
                }
                if (argument.Key == "-username" || argument.Key == "-u")
                {
                    Config.Username = argument.Value.Replace("\"", "");
                }
                if (argument.Key == "-domain" || argument.Key == "-d")
                {
                    Config.Domain = argument.Value.Replace("\"", "");
                }
                if (argument.Key == "-password" || argument.Key == "-p")
                {
                    Config.Password = argument.Value.Replace("\"", "");
                }
                if (argument.Key == "-verbose" || argument.Key == "-v")
                {
                    Config.Verbose = ParseBoolean(argument.Key, argument.Value);
                    if (parseError)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static Dictionary<String, String> GetDictionaryFromArguments(string[] args)
        {
            Dictionary<String, String> parsedArgs = new Dictionary<String, String>();
            String seperator = "|";
            if (args.Length == 0)
            {
                return parsedArgs;
            }
            if (args[0] == "-h" || args[0] == "--help")
            {
                Help();
                return parsedArgs;
            }
            String sArgs = String.Join(seperator, args);

            try
            {
                parsedArgs = sArgs.Split('|').Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.ERROR, "Failed to parse arguments: " + e.Message);
            }
            return parsedArgs;
        }

        private static Boolean ParseBoolean(String arg, String value)
        {
            if (value.ToLower().Contains("true"))
            {
                return true;
            }
            else if (value.ToLower().Contains("false"))
            {
                return false;
            }
            else
            {
                Logger.Print(Logger.STATUS.ERROR, String.Format("Unrecognised argument for {0}: ", arg) + value);
                parseError = true;
                return false;
            }
        }

        public static void Help()
        {
            Console.WriteLine();
            Console.WriteLine("PS> Corellia.exe <arguments>");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("OPTIONS:");
            Console.ResetColor();
            Console.WriteLine("   {0,-15}Username to authenticate with", "-username");
            Console.WriteLine("   {0,-15}Domain to authenticate with", "-domain");
            Console.WriteLine("   {0,-15}Password to authenticate with", "-password");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("EXAMPLES:");
            Console.ResetColor();
            Console.WriteLine("   PS> Corellia.exe -username=\"administrator\" -password=\"Password1\" -domain=\"lab.local\"");
            Console.WriteLine();
        }
    }
}