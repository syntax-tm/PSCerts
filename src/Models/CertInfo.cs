using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PSCerts.Models
{
    [DebuggerDisplay("{FriendlyName} ({Thumbprint})")]
    public class CertInfo
    {
        public string FriendlyName => Certificate.FriendlyName;
        public string Thumbprint => Certificate.Thumbprint;
        public X500DistinguishedName SubjectName => Certificate.SubjectName;
        public X509Certificate2 Certificate { get; set; }
        public bool HasPrivateKey => PrivateKey != null;
        public FileInfo PrivateKey { get; set; }
        public List<AuthorizationRule> Permissions { get; set; }
    }
}
