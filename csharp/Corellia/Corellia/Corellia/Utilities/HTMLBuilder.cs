using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Corellia.Utilities
{
    internal class HTMLBuilder
    {
        protected static StringBuilder html = new StringBuilder();

        private static void AddTitleTag()
        {
            html.Append(@"<title>Corellia: " + Config.Domain + "</title>\n");
        }

        private static void AddStyle()
        {
            html.Append(@"<style>
th, td,tr {
    padding: 15px;
    text-align: left;
    word-wrap: break-all;
}
table-layout: fixed; width: 100%;
</style>");
            html.Append("\n");
        }

        private static void AddHeadTags()
        {
            html.Append("<!DOCTYPE html>\n");
            html.Append("<head>\n");
            AddTitleTag();
            AddStyle();
            html.Append("</head>\n");
        }

        private static string BuildSpnTable(List<Structure.SPN> spns = null)
        {
            StringBuilder tbl = new StringBuilder();
            if (spns == null)
            {
                return "";
            }

            tbl.Append("<table>\n");

            tbl.Append("<tr>\n");
            tbl.Append("<th> Username </th>\n");
            tbl.Append("<th> Domain </th>\n");
            tbl.Append("<th> SPN </th>\n");
            tbl.Append("<th> Encryption Type </th>\n");
            tbl.Append("<th> Hash </th>\n");
            tbl.Append("</tr>\n");

            foreach (Structure.SPN spn in spns)
            {
                tbl.Append("<tr>");
                tbl.Append("<td>" + spn.Username + "</td>\n");
                tbl.Append("<td>" + spn.Domain + "</td>\n");
                tbl.Append("<td>" + spn.ServicePrincipalName + "</td>\n");
                tbl.Append("<td>" + spn.EncType + "</td>\n");
                tbl.Append("<td>" + spn.Hash + "</td>\n");
                tbl.Append("</tr>\n");
            }
            tbl.Append("</table>\n");
            return tbl.ToString();
        }

        private static string BuildUsersTable(List<Structure.User> users = null)
        {
            StringBuilder tbl = new StringBuilder();
            if (users == null)
            {
                return "";
            }

            tbl.Append("<table>\n");

            tbl.Append("<tr>\n");
            tbl.Append("<th> Username </th>\n");
            tbl.Append("<th> Description </th>\n");
            tbl.Append("</tr>\n");

            foreach (Structure.User user in users)
            {
                tbl.Append("<tr>");
                tbl.Append("<td>" + user.Username + "</td>\n");
                tbl.Append("<td>" + user.Description + "</td>\n");
                tbl.Append("</tr>\n");
            }
            tbl.Append("</table>\n");
            return tbl.ToString();
        }

        private static void AddBody(List<Structure.SPN> spns, List<Structure.GPPPassword> gpppasswords, List<Structure.User> descriptions)
        {
            html.Append("<body>\n");
            html.Append("<h1>Kerberoasting:</h1>\n");
            html.Append(BuildSpnTable(spns));
            // not too sure what to do with gpppasswords just yet

            html.Append("<h1>Users:</h1>\n");
            html.Append(BuildUsersTable(descriptions));
            html.Append("</body>\n");
        }

        public static void BuildReport(List<Structure.SPN> spns, List<Structure.GPPPassword> gpppasswords, List<Structure.User> descriptions)
        {
            AddHeadTags();
            AddBody(spns, gpppasswords, descriptions);
            html.Append("</html>");
            string outfile = "corellia.html";
            Console.WriteLine(outfile);
            File.WriteAllText(outfile, html.ToString());
            return;
        }
    }
}