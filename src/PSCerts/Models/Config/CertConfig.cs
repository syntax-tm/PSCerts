using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using PSCerts.Util;

namespace PSCerts.Config
{
    public class CertConfig
    {
        [JsonProperty("cert", Required = Required.Always)]
        public string CertFile { get; set; }

        public string CertFileName => Path.GetFileName(CertFile);

        [JsonProperty("password")]
        public CertPassword Password { get; set; } = null;

        [JsonProperty("exportable")]
        public bool Exportable { get; set; }

        [JsonProperty("stores", Required = Required.Always)]
        public List<CertStore> Stores { get; set; } = new();

        [JsonProperty("permissions")]
        public List<CertPermissions> Permissions { get; set; } = new();

        public bool HasPassword => Password != null;
        
        [JsonConstructor]
        public CertConfig(string certFile)
        {
            if (string.IsNullOrWhiteSpace(certFile)) throw new ArgumentNullException(nameof(certFile));

            var path = FileSystemHelper.ResolvePath(certFile);
            CertFile = path.FullName;
        }
    }
}
