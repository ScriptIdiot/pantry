using System;
using System.Collections.Generic;

namespace Sift
{
    public class Sift
    {
        public static void Main(string[] args)
        {
            Utilities.Banner.Show();
            if (args.Length == 0)
            {
                Helpers.Arguments.Help();
                return;
            }
            else
            {
                if (args[0] == "-h" || args[0] == "--help" || args[0] == "-help")
                {
                    Helpers.Arguments.Help();
                    return;
                }
            }

            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (Helpers.Arguments.ParseArguments(args))
            {
                List<String> targets = Helpers.SubnetParser.GetScope(Helpers.Arguments.Targets);

                if (targets.Count == 0)
                {
                    Logger.Print(Logger.STATUS.ERROR, "No targets found!");
                    return;
                }
                else if (targets.Count > 0)
                {
                    Logger.Print(Logger.STATUS.GOOD, "Total targets: ", targets.Count.ToString());
                }

                if (Helpers.Arguments.Jitter != 0)
                {
                    Logger.Print(Logger.STATUS.GOOD, "Random jitter between: ", String.Format("0 - {0}(s)", Helpers.Arguments.Jitter));
                }

                if (Helpers.Arguments.TCP)
                {
                    Logger.Print(Logger.STATUS.GOOD, "TCP: True");
                }
                else
                {
                    Logger.Print(Logger.STATUS.GOOD, "TCP: False");
                }

                Console.WriteLine();

                if (!Utilities.Creds.CheckIfCredsPresent())
                {
                    Logger.Print(Logger.STATUS.INFO, "Username: ", "NULL");
                    Logger.Print(Logger.STATUS.INFO, "Password: ", "NULL");
                    Helpers.Arguments.Username = "";
                    Helpers.Arguments.Domain = "";
                    Helpers.Arguments.Password = "";
                }
                else
                {
                    Logger.Print(Logger.STATUS.INFO, "Username: ", Helpers.Arguments.Domain + "\\" + Helpers.Arguments.Username);
                    Logger.Print(Logger.STATUS.INFO, "Password: ", Helpers.Arguments.Password);
                }

                Console.WriteLine();

                if (Helpers.Arguments.TCP)
                {
                    List<String> targetsWithSmb = Helpers.Network.GetTargetsWithOpenPorts(targets);
                    if (targetsWithSmb.Count != 0)
                    {
                        Logger.Print(Logger.STATUS.GOOD, "Targets with SMB Open: ", targetsWithSmb.Count.ToString());
                        Console.WriteLine();
                        Helpers.Execution.Execute(targetsWithSmb);
                    }
                    else
                    {
                        Logger.Print(Logger.STATUS.ERROR, "No targets with SMB open were found");
                    }
                }
                else
                {
                    Helpers.Execution.Execute(targets);
                }
            }

            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;
            Logger.Print(Logger.STATUS.GOOD, "Done!");
            Logger.Print(Logger.STATUS.INFO, "Execution time: ", Convert.ToInt32(elapsedMs) + "ms");
        }
    }
}