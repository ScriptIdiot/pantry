using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpExec
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Dictionary<String, String> Arguments = Utils.GetArgs(args);

            if (Arguments.Count() != 5)
            {
                Utils.Help();
            }
            string UNC;
            if (Arguments["Share"] == null || Arguments["Share"] == "")
            {
                UNC = String.Format("\\\\{0}\\ADMIN$", Arguments["Host"], Arguments["Share"]);
            }
            else
            {
                UNC = String.Format("\\\\{0}\\{1}", Arguments["Host"], Arguments["Share"]);
            }

            if (UNC == "")
            {
                Utils.Help();
            }

            Logger.Print(Logger.STATUS.INFO, "Using: " + UNC);
            if (Arguments["Username"] == null && Arguments["Password"] == null)
            {
                Logger.Print(Logger.STATUS.INFO, "Using current session: " + Utils.GetCurrentUser());
                ConnectionManager.Execute(UNC, null, null, Arguments["FilePath"], Arguments["Host"]);
            }
            else
            {
                Logger.Print(Logger.STATUS.INFO, "Using credentials: " + Arguments["Username"] + ":" + Arguments["Password"]);
                ConnectionManager.Execute(UNC, Arguments["Username"], Arguments["Password"], Arguments["FilePath"], Arguments["Host"]);
            }
        }
    }
}