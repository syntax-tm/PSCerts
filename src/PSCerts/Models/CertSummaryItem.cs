using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PSCerts
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
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            var store = $@"{Location}\{Store}";

            sb.Append(store.Truncate(30, true));
            sb.Append(" ");
            sb.Append(DisplayName.Truncate(30, true));
            
            var pk = HasPrivateKey ? $"{Fg.BrightCyan}w/ Key{Style.Reset}" : string.Empty;

            sb.AppendFormat(" {0} {1,6}", Thumbprint, pk);

            if (Permissions != null && Permissions.Any())
            {
                var maxRights = Permissions.Max(p => p.FileSystemRightsString.Length);
                foreach (var permission in Permissions)
                {
                    var ruleType = permission.IsAllow ? $"{Fg.Green} + " : $"{Fg.Red} - ";
                    sb.AppendLine();
                    sb.Append(ruleType);

                    var format = @"{0," + maxRights + @"}";

                    sb.AppendFormat(format, permission.FileSystemRightsString);
                    sb.AppendFormat("{0} {1}", Style.Reset, permission.Identity);
                }
            }

            return sb.ToString();
        }
    }
}
