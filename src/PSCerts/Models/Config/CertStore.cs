using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace PSCerts.Config
{
    public class CertStore
    {
        [JsonProperty("location", Required = Required.Always)]
        public StoreLocation Location { get; set; }

        [JsonProperty("store", Required = Required.Always)]
        public StoreName Store { get; set; }
    }
}
