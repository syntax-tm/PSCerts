using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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

        public override string ToString()
        {
            var sb = new StringBuilder();
            var store = $@"{Location}\{Store}";

            if (store.Length > 30)
            {
                sb.Append(store.Substring(0, 27));
                sb.Append("...");
            }
            else
            {
                sb.Append(store.PadRight(30));
            }
            
            sb.Append(" ");

            if (DisplayName.Length > 30)
            {
                sb.Append(DisplayName.Substring(0, 27));
                sb.Append("...");
            }
            else
            {
                sb.Append(DisplayName.PadRight(30));
            }
            
            sb.Append(" ");
            
            sb.Append(Thumbprint);
            
            sb.Append(" ");
            
            sb.Append(HasPrivateKey ? "w/ Key" : "      ");

            if (Permissions != null && Permissions.Any())
            {
                var maxRights = Permissions.Max(p => p.FileSystemRights.ToString().Length);
                foreach (var permission in Permissions)
                {
                    var typeChar = permission.IsAllow ? "+" : "-";
                    sb.Append("\n  ");
                    sb.Append(typeChar);
                    sb.Append(" ");
                    sb.Append($"{permission.FileSystemRights.ToString().PadRight(maxRights + 1)}");
                    sb.Append(permission.Identity);
                }
            }

            return sb.ToString();
        }
    }
}
