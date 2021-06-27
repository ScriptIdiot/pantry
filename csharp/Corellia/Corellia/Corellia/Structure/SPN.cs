namespace Corellia.Structure
{
    public class SPN
    {
        public string Username { get; set; }
        public string Domain { get; set; }
        public string Hash { get; set; }
        public string EncType { get; set; }
        public string ServicePrincipalName { get; set; }
        public byte[] Token { get; set; }
    }
}