using Corellia.Utilities;
using System;

namespace Corellia
{
    internal class Corellia
    {
        private static void Main(string[] args)
        {
            Banner.Show();
            if (args.Length != 0)
            {
                if (args[0] == "-h" || args[0] == "--help" || args[0] == "-help")
                {
                    Arguments.Help();
                    return;
                }
            }

            // Todo: Remove the following hard-coded settings
            Config.Kerberoast = false;
            Config.GPPPassword = true;
            Config.LDAPDescriptions = true;
            Config.Spray = "";

            if (Arguments.ParseArguments(args))
            {
                if (!Config.CheckCredArgs())
                {
                    Logger.Print(Logger.STATUS.ERROR, "If creds are required, please use domain, username and password!");
                    return;
                }
                else
                {
                    Execute();
                }
            }
            else
            {
                Execute();
            }
        }
        private static void Execute()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            ModuleHandler.ExecuteDomain();

            watch.Stop();
            long elapsedSeconds = watch.ElapsedMilliseconds / 1000;
            Logger.Print(Logger.STATUS.INFO, "Execution time: ", Convert.ToInt32(elapsedSeconds) + "s");
            Console.ReadLine();
        }
    }
}