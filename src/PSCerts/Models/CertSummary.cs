using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;

namespace PSCerts
{
    [DebuggerDisplay("{Subject} ({Thumbprint})")]
    public class CertSummary
    {
        public StoreLocation Location { get; set; }
        public string StoreName { get; set; }
        public string FriendlyName => Certificate?.FriendlyName;
        public string Thumbprint => Certificate?.Thumbprint;
        public string Subject => Certificate?.SubjectName.ToString();
        public X509Certificate2 Certificate { get; set; }
        public bool HasPrivateKey => PrivateKey != null;
        public FileInfo PrivateKey { get; set; }
        public List<AuthorizationRule> Permissions { get; set; }

        public CertSummary(X509Certificate2 certificate)
        {
            Certificate = certificate;
        }
        
        public CertSummary(X509Store store, X509Certificate2 certificate)
        {
            Location = store.Location;
            StoreName = store.Name;
            Certificate = certificate;
        }

        public CertSummary(X509Certificate2 certificate, StoreLocation location, StoreName storeName)
        {
            Certificate = certificate;
            Location = location;
            StoreName = storeName.ToString();
        }
    }
}
