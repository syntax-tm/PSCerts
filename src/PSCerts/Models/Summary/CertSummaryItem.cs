using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace PSCerts.Summary
{
    [DebuggerDisplay("{Subject} ({Thumbprint})")]
    public class CertSummaryItem
    {
        public StoreLocation Location { get; internal set; }
        public string Store { get; set; }
        public string DisplayName { get; }
        public string FriendlyName { get; }
        public string Thumbprint { get; }
        public string Subject { get; }
        public bool HasPrivateKey => PrivateKey != null;
        public FileInfo PrivateKey { get; internal set; }
        public List<CertAccessRule> Permissions { get; internal set; }
        public X509Certificate2 Certificate { get; }

        public CertSummaryItem(X509Certificate2 certificate)
        {
            Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
            FriendlyName = certificate.FriendlyName;
            Thumbprint = certificate.Thumbprint;
            Subject = certificate.SubjectName.Format(false);
            DisplayName = !string.IsNullOrWhiteSpace(FriendlyName)
                ? FriendlyName
                : Subject;
        }

        public CertSummaryItem(X509Store store, X509Certificate2 certificate)
            : this (certificate)
        {
            Location = store.Location;
            Store = store.Name;
        }

        public CertSummaryItem(X509Certificate2 certificate, StoreLocation location, StoreName storeName)
            : this (certificate)
        {
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
