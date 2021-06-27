using SharpRunAs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Hosting;

namespace CSharpRunAs
{
    class Program
    {
        public static void Main(string[] args)
        {
            Dictionary<String,String> Arguments = Utils.GetArgs(args);

            if(Arguments.Count() != 4)
            {
                Console.WriteLine(@"[*] Usage: .\CSharpRunAs.exe <Domain> <Username> <Password> <Path2Exe>");
                Environment.Exit(1);
            }

            Console.WriteLine("[*] Current PID: " + Utils.CurrentPid());
            Console.WriteLine("[*] Authenticating as " + String.Format("{0}\\{1}", Arguments["Domain"], Arguments["Username"]));
            Console.WriteLine("[*] Will use " + Arguments["ExePath"] + " for execution!");
            ImpersonationManager.Execute(Arguments);
        }
    }
}