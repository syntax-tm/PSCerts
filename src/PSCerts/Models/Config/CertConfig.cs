using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PSCerts.Util;

namespace PSCerts.Config
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CertConfig : IValidate
    {
        [JsonProperty("cert", Required = Required.Always)]
        public string CertFile { get; set; }

        [JsonProperty("password")]
        public CertPassword Password { get; set; }

        [JsonProperty("exportable")]
        public bool Exportable { get; set; }

        [JsonProperty("stores", Required = Required.Always)]
        public List<CertStore> Stores { get; set; } = new();

        [JsonProperty("permissions")]
        public List<CertPermissions> Permissions { get; set; } = new();

        public string CertFileName { get; }
        public string CertFileExtension { get; }
        public bool HasPassword => Password != null;
        public bool HasPermissions => Permissions != null && Permissions.Any();
        public string Thumbprint { get; private set; }
        public string PrivateKey { get; private set; }
        public bool HasPrivateKey { get; private set; }
        public X509Certificate2 Certificate { get; private set; }

        [JsonConstructor]
        public CertConfig(string certFile)
        {
            if (string.IsNullOrWhiteSpace(certFile)) throw new ArgumentNullException(nameof(certFile));

            var path = FileSystemHelper.ResolvePath(certFile);
            CertFile = path.FullName;
            CertFileName = Path.GetFileName(CertFile);
            CertFileExtension = Path.GetExtension(CertFile);
        }

        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            
            if (!File.Exists(CertFile)) result.Add($@"Certificate file '{CertFile}' does not exist.");
            
            if (CertHelper.IsPasswordRequired(CertFile))
            {
                if (!HasPassword) result.Add($@"Password is required for {CertFileExtension} files.");
                result.Add(Password.Validate());
            }

            if (!Stores.Any()) result.Add($"One or more {nameof(Stores)} are required.");
            return result;
        }

        public void Import()
        {
            var password = Password.GetValue();

            foreach (var certStore in Stores)
            {
                try
                {
                    using var store = new X509Store(certStore.Store, certStore.Location);
                    store.Open(OpenFlags.ReadWrite);
                    
                    var flags = GetStorageFlags(store.Location);
                    var cert = new X509Certificate2(CertFile, password, flags);
                    Thumbprint = cert.Thumbprint;

                    store.Certificates.Add(cert);

                    if (cert.HasPrivateKey)
                    {
                        HasPrivateKey = PrivateKeyHelper.TryGetPrivateKey(cert, out var pk);
                        PrivateKey = pk;
                    }
                }
                catch (Exception e)
                {
                    PowerShellHelper.Error(e);
                }
            }
        }

        private X509KeyStorageFlags GetStorageFlags(StoreLocation location)
        {
            var flags = location == StoreLocation.CurrentUser
                ? X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet
                : X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet;

            if (Exportable)
                flags |= X509KeyStorageFlags.Exportable;

            return flags;
        }
    }
}
