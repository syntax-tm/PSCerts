﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

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
        
        public static X509Certificate2 FindCertificate(string thumbprint)
        {
            if (string.IsNullOrWhiteSpace(thumbprint)) throw new ArgumentNullException(nameof(thumbprint));

            using var ps = PowerShell.Create();

            ps.AddCommand("Get-ChildItem");
            ps.AddParameter("Path", "Cert:\\*");
            ps.AddParameter("Recurse");
            ps.AddCommand("Where-Object");
            ps.AddParameter("Property", nameof(X509Certificate2.Thumbprint));
            ps.AddParameter("Like");
            ps.AddParameter("Value", $"*{thumbprint}*");
            ps.AddCommand("Sort-Object");
            ps.AddParameter("Property", nameof(X509Certificate2.HasPrivateKey));
            ps.AddParameter("Descending");
            ps.AddCommand("Select-Object");
            ps.AddParameter("First", 1);

            var result = ps.Invoke<X509Certificate2>()?.FirstOrDefault();

            return result;
        }

        public static List<CertSummaryItem> GetCertSummary(bool hasPrivateKey = false)
        {
            var items = new List<CertSummaryItem>();
            var locations = new [] { StoreLocation.LocalMachine, StoreLocation.CurrentUser };
            var stores = Enum.GetValues(typeof(StoreName)).Cast<StoreName>();
            var certStores = from l in locations
                             from s in stores
                             select new { Location = l, Store = s };

            foreach (var certStore in certStores)
            {
                try
                {
                    using var store = new X509Store(certStore.Store, certStore.Location);
                    store.Open(OpenFlags.ReadOnly);

                    foreach (var cert in store.Certificates)
                    {
                        var summaryItem = new CertSummaryItem(store, cert);

                        if (PrivateKeyHelper.TryGetPrivateKey(cert, out var privateKeyFile))
                        {
                            var privateKeyInfo = new FileInfo(privateKeyFile);
                            summaryItem.PrivateKey = privateKeyInfo;

                            var acl = FileSystemHelper.GetAccessControl(privateKeyFile);

                            try
                            {
                                var rules = acl.GetAccessRules(true, true, typeof(NTAccount));
                                var perms = CertAccessRule.Create(rules);
                                summaryItem.Permissions = perms;
                            }
                            catch { }
                        }

                        if (hasPrivateKey && !summaryItem.HasPrivateKey)
                        {
                            continue;
                        }

                        items.Add(summaryItem);
                    }
                }
                catch (Exception e)
                {
                    PowerShellHelper.Error(e);
                }
            }

            return items;
        }
    }
}
