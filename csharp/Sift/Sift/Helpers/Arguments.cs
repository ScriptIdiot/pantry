using System;
using System.Collections.Generic;
using System.Linq;

namespace Sift.Helpers
{
    internal class Arguments
    {
        public static String Targets = "";
        public static String Username = null;
        public static String Domain = null;
        public static String Password = null;
        public static String Outfile = "";
        public static Boolean TCP = true;
        public static Int32 Jitter = 0;
        public static Boolean Verbose = false;
        public static Boolean Randomise = false;
        public static List<String> Extensions = new List<string> { "ps1", "docx", "docm", "xlsx", "xlsm", "xls", "hta", "vba", "vbs" };
        public static List<String> Keywords = new List<string>();
        public static List<String> IncludeShares = new List<string>();
        public static List<String> ExcludeShares = new List<string>();
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
                if (argument.Key == "-targets")
                {
                    Targets = argument.Value.Replace("\"", "");
                }
                if (argument.Key == "-username")
                {
                    Username = argument.Value.Replace("\"", "");
                }
                if (argument.Key == "-domain")
                {
                    Domain = argument.Value.Replace("\"", "");
                }
                if (argument.Key == "-password")
                {
                    Password = argument.Value.Replace("\"", "");
                }
                if (argument.Key == "-outfile")
                {
                    Outfile = argument.Value.Replace("\"", "");
                }
                if (argument.Key == "-tcp")
                {
                    TCP = ParseBoolean(argument.Key, argument.Value);
                    if (parseError)
                    {
                        return false;
                    }
                }
                if (argument.Key == "-jitter")
                {
                    try
                    {
                        Jitter = Int32.Parse(argument.Value);
                    }
                    catch (Exception e)
                    {
                        Logger.Print(Logger.STATUS.ERROR, "Failed to parse -sleep: " + e.Message);
                        return false;
                    }
                }
                if (argument.Key == "-verbose")
                {
                    Verbose = ParseBoolean(argument.Key, argument.Value);
                    if (parseError)
                    {
                        return false;
                    }
                }
                if (argument.Key == "-randomise")
                {
                    Randomise = ParseBoolean(argument.Key, argument.Value);
                    if (parseError)
                    {
                        return false;
                    }
                }
                if (argument.Key == "-extensions")
                {
                    Extensions = ParseList(argument.Key, argument.Value);
                    if (parseError)
                    {
                        return false;
                    }
                }
                if (argument.Key == "-keywords")
                {
                    Keywords = ParseList(argument.Key, argument.Value);
                    if (parseError)
                    {
                        return false;
                    }
                }
                if (argument.Key == "-shares")
                {
                    IncludeShares = ParseList(argument.Key, argument.Value);
                    if (parseError)
                    {
                        return false;
                    }
                }
                if (argument.Key == "-eshares")
                {
                    ExcludeShares = ParseList(argument.Key, argument.Value);
                    if (parseError)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void Help()
        {
            Console.WriteLine("PS> Sift.exe <arguments>");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("OPTIONS:");
            Console.ResetColor();
            Console.WriteLine("   {0,-15}Targets to check", "-targets");
            Console.WriteLine("   {0,-15}Username to authenticate with", "-username");
            Console.WriteLine("   {0,-15}Domain to authenticate with", "-domain");
            Console.WriteLine("   {0,-15}Password to authenticate with", "-password");
            Console.WriteLine("   {0,-15}Keywords to look for in file names (Default: None)", "-keywords");
            Console.WriteLine("   {0,-15}Extensions to look for in file names (Default:  \"ps1\", \"docx\", \"docm\", \"xlsx\", \"xlsm\", \"xls\", \"hta\", \"vba\", \"vbs\")", "-extensions");
            Console.WriteLine("   {0,-15}Specify the shares to look through", "-shares");
            Console.WriteLine("   {0,-15}Specify the shares to AVOID", "-eshares");
            Console.WriteLine("   {0,-15}Random sleep between 0 and X", "-jitter");
            Console.WriteLine("   {0,-15}Enable/Disable the default TCP check", "-tcp");
            Console.WriteLine("   {0,-15}Loop through targets randomly", "-randomise");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("EXAMPLES:");
            Console.ResetColor();
            Console.WriteLine("   PS> Sift.exe -targets=\"192.168.0.0/24\" -username=\"administrator\" -password=\"Password1\" -domain=\"lab.local\"");
            Console.WriteLine("   PS> Sift.exe -targets=\"192.168.0.12\" -username=\"administrator\" -password=\"Password1\" -domain=\"lab.local\"");
            Console.WriteLine("   PS> Sift.exe -targets=\"auto\" -username=\"administrator\" -password=\"Password1\" -domain=\"lab.local\"");
            Console.WriteLine("   PS> Sift.exe -targets=\"c:\\temp\\computers.txt\" -username=\"administrator\" -password=\"Password1\" -domain=\"lab.local\"");
            Console.WriteLine("   PS> Sift.exe -targets=\"192.168.0.12-192.168.0.24\" -username=\"administrator\" -password=\"Password1\" -domain=\"lab.local\"");
            Console.WriteLine("   PS> Sift.exe -targets=\"192.168.0.12-192.168.0.24\" -username=\"administrator\" -password=\"Password1\" -domain=\"lab.local\" -shares=\"DEV\"");
            Console.WriteLine("   PS> Sift.exe -targets=\"192.168.0.12-192.168.0.24\" -username=\"administrator\" -password=\"Password1\" -domain=\"lab.local\" -eshares=\"c$\"");
            Console.WriteLine("   PS> Sift.exe -targets=\"192.168.0.12-192.168.0.24\" -username=\"administrator\" -password=\"Password1\" -domain=\"lab.local\" -extensions=\"ps1,hta,vba,vbs\"");
            Console.WriteLine("   PS> Sift.exe -targets=\"192.168.0.12-192.168.0.24\" -username=\"administrator\" -password=\"Password1\" -domain=\"lab.local\" -keywords=\"passwords\"");
            Console.WriteLine("   PS> Sift.exe -targets=\"192.168.0.12-192.168.0.24\" -username=\"administrator\" -password=\"Password1\" -domain=\"lab.local\" -randomise=\"true\"");
            Console.WriteLine();
        }

        private static Dictionary<String, String> GetDictionaryFromArguments(string[] args)
        {
            Dictionary<String, String> parsedArgs = new Dictionary<String, String>();
            String seperator = "|";
            if (args.Length == 0)
            {
                Help();
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

        private static List<String> ParseList(String arg, String value)
        {
            List<String> parsed = new List<string>();
            try
            {
                parsed = value.Split(',').ToList();
            }
            catch
            {
                Logger.Print(Logger.STATUS.ERROR, "Failed to split extensions on commas!");
            }
            return parsed;
        }
    }
}