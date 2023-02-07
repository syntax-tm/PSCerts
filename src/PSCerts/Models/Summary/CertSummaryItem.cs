using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace PSCerts.Summary
{
    [DebuggerDisplay("{Subject} ({Thumbprint})")]
    public class CertSummaryItem
    {
        public StoreLocation Location { get; set; }
        public string Store { get; set; }
        public string FriendlyName => Certificate?.FriendlyName;
        public string Thumbprint => Certificate?.Thumbprint;
        public string Subject => Certificate?.SubjectName.Format(false);
        public bool HasPrivateKey => PrivateKey != null;
        public FileInfo PrivateKey { get; set; }
        public List<CertAccessRule> Permissions { get; set; }
        public X509Certificate2 Certificate { get; set; }

        public CertSummaryItem(X509Certificate2 certificate)
        {
            Certificate = certificate;
        }

        public CertSummaryItem(X509Store store, X509Certificate2 certificate)
        {
            Location = store.Location;
            Store = store.Name;
            Certificate = certificate;
        }

        public CertSummaryItem(X509Certificate2 certificate, StoreLocation location, StoreName storeName)
        {
            Certificate = certificate;
            Location = location;
            Store = storeName.ToString();
        }

        public string GetPermissionsAsText(bool multiLine = true)
        {
            if (Permissions == null) return string.Empty;

            var perms = new List<string>();
            foreach (var ar in Permissions)
            {
                perms.Add(ar.ToString());
            }
            var separator = multiLine ? Environment.NewLine : ", ";

            return string.Join(separator, perms);
        }
    }
}
