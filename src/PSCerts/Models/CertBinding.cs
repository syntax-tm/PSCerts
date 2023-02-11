using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Web.Administration;

namespace PSCerts
{
    public class CertBinding
    {
        public Site Site { get; internal set; }
        public ApplicationPool AppPool { get; internal set; }
        public string AppPoolIdentity { get; internal set; }
        public StoreLocation Location { get; internal set; }
        public StoreName StoreName { get; internal set; }
        public X509Certificate2 Certificate { get; internal set; }
        public FileSecurity PrivateKeySecurity { get; internal set; }
        public Binding Binding { get; internal set; }
    }
}
