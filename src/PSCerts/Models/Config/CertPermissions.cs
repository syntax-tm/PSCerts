using System.Security.AccessControl;
using Newtonsoft.Json;

namespace PSCerts.Config
{
    public class CertPermissions
    {
        [JsonProperty("identity", Required = Required.Always)]
        public string Identity { get; set; }
        [JsonProperty("rights", Required = Required.Always)]
        public FileSystemRights FileSystemRights { get; set; }
        [JsonProperty("access")]
        public AccessControlType AccessType { get; set; } = AccessControlType.Allow;
    }
}
