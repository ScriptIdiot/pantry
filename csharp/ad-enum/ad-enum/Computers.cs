using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Text;

namespace ad_enum
{
	class Computers
	{
		public static Dictionary<String, String> Get(String Domain)
		{
			Console.WriteLine("[*] Starting Computer Enumeration!");

			DirectorySearcher ds = null;
			DirectoryEntry de = null;

			Dictionary<String,String> Computers = new Dictionary<String, String>();

			try
			{
				de = new DirectoryEntry("LDAP://" + Domain);

				ds = new DirectorySearcher(de);

				ds.Filter = "(&(objectCategory=Computer))";
			}
			catch(Exception e)
			{
				Console.WriteLine("[!] " + e.Message);
				return Computers;
			}

			foreach (SearchResult r in ds.FindAll())
			{
				try
				{
					String ComputerName = r.GetDirectoryEntry().Name;
					if (ComputerName.StartsWith("CN="))
					{
						ComputerName = ComputerName.Remove(0, "CN=".Length);

						IPAddress[] IPs = Dns.GetHostAddresses(ComputerName);
						String IP = null;
						if (IPs.Length >= 1)
						{
							IP = IPs[0].ToString();
						}
						else
						{
							IP = "Unresolved";
						}

						if (!Computers.Keys.Contains(ComputerName))
						{
							Computers.Add(ComputerName,IP);
							Console.WriteLine(String.Format("[+] {0} ({1})",ComputerName,IP));
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("[!] " + e.Message);
					return Computers;
				}
			}
			return Computers;
		}
	}
}
