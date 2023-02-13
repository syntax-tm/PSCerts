using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using PSCerts.Summary;

namespace PSCerts.Util
{
    public static class CertHelper
    {
        public static CertType GetCertType(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            ext = ext.Replace('-', '_');
            ext = ext.Trim('.');

            var typeFound = Enum.TryParse<CertType>(ext, true, out var result);
            return typeFound
                ? result
                : throw new ArgumentException($"File extension '{ext}' is not a valid {nameof(CertType)}.", nameof(fileName));
        }

        public static bool IsPasswordRequired(string fileName)
        {
            var certType = GetCertType(fileName);
            return certType.HasFlag(CertType.HasPrivateKey);
        }

        public static void Import(X509Certificate2 certificate, StoreLocation location, StoreName storeName)
        {
            X509Store store = null;
            try
            {
                store = new (storeName, location);
                store.Open(OpenFlags.ReadWrite);

                store.Add(certificate);
            }
            finally
            {
                store?.Close();
            }
        }

        public static X509Certificate2 FindCertificate(X509Store store, string thumbprint)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (string.IsNullOrWhiteSpace(thumbprint)) throw new ArgumentNullException(nameof(thumbprint));
            
            var matches = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

            return matches[0];
        }

        public static CertSummary GetCertSummary(IList<StoreLocation> locations, IList<StoreName> stores, bool pkOnly = false)
        {
            var summary = new CertSummary();
            
            foreach (var location in locations)
            foreach (var storeName in stores)
            {
                var store = new X509Store(storeName, location);

                try
                {
                    store.Open(OpenFlags.ReadOnly);

                    foreach (var cert in store.Certificates)
                    {
                        try
                        {
                            var summaryItem = new CertSummaryItem(store, cert);

                            if (PrivateKeyHelper.TryGetPrivateKey(cert, out var privateKeyFile))
                            {
                                var privateKeyInfo = new FileInfo(privateKeyFile);
                                summaryItem.PrivateKey = privateKeyInfo;

                                var acl = FileSystemHelper.GetAccessControl(privateKeyFile);
                                var rules = acl.GetAccessRules(true, true, typeof(SecurityIdentifier));
                                var perms = CertAccessRule.Create(rules);

                                summaryItem.Permissions = perms;
                            }
                            else if (pkOnly)
                            {
                                continue;
                            }

                            summary.Items.Add(summaryItem);
                        }
                        catch
                        {
                            // TODO: log these exceptions to either verbose or warning stream
                        }
                    }
                }
                finally
                {
                    store.Close();
                }
            }

            return summary;
        }
    }
}
