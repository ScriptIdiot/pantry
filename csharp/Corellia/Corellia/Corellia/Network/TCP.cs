using Corellia.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Corellia.Network
{
    internal class TCP
    {
        private static readonly Dictionary<String, String> ERRORS = new Dictionary<string, string>
        {
            { "Host failed to respond", "A connection attempt failed because the connected party did not properly respond after a period of time" },
            { "Host refused the connection", "No connection could be made because the target machine actively refused it" }
        };

        public static string Send(String target)
        {
            using (TcpClient Scan = new TcpClient())
            {
                try
                {
                    Scan.Connect(target, 445);
                    return "";
                }
                catch (Exception e)
                {
                    Logger.Print(Logger.STATUS.VERBOSE, "Error from " + target + ": ", e.Message);
                    foreach (KeyValuePair<string, string> errors in ERRORS)
                    {
                        if (e.Message.Contains(errors.Value))
                        {
                            return errors.Key;
                        }
                    }
                    return e.Message;
                }
            }
        }
    }
}