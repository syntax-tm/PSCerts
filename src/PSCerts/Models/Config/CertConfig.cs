using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using PSCerts.Util;

namespace PSCerts.Config
{
    public class CertConfig : IValidate
    {
        [JsonProperty("cert", Required = Required.Always)]
        public string CertFile { get; set; }

        [JsonIgnore]
        public string CertFileName => Path.GetFileName(CertFile);
        
        [JsonIgnore]
        public string CertFileExtension => Path.GetExtension(CertFileName);

        [JsonProperty("password")]
        public CertPassword Password { get; set; }

        [JsonProperty("exportable")]
        public bool Exportable { get; set; }

        [JsonProperty("stores", Required = Required.Always)]
        public List<CertStore> Stores { get; set; } = new();

        [JsonProperty("permissions")]
        public List<CertPermissions> Permissions { get; set; } = new();

        [JsonIgnore]
        public bool HasPassword => Password != null;

        [JsonIgnore]
        public bool HasPermissions => Permissions != null && Permissions.Any();

        [JsonIgnore]
        public string Thumbprint { get; private set; }

        [JsonConstructor]
        public CertConfig(string certFile)
        {
            if (string.IsNullOrWhiteSpace(certFile)) throw new ArgumentNullException(nameof(certFile));

            var path = FileSystemHelper.ResolvePath(certFile);
            CertFile = path.FullName;
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
            
            return result;
        }

        public void Import()
        {
            var password = Password.GetValue();

            foreach (var certStore in Stores)
            {
                using var store = new X509Store(certStore.Store, certStore.Location);
                store.Open(OpenFlags.ReadWrite);
                
                var flags = X509KeyStorageFlags.PersistKeySet;

                if (store.Location == StoreLocation.CurrentUser)
                    flags |= X509KeyStorageFlags.UserKeySet;
                else
                    flags |= X509KeyStorageFlags.MachineKeySet;

                if (Exportable)
                    flags |= X509KeyStorageFlags.Exportable;

                var cert = new X509Certificate2(CertFile, password, flags);

                Thumbprint = cert.Thumbprint;

                store.Certificates.Add(cert);

                store.Close();
            }
        }
    }
}
