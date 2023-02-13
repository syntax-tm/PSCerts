using System.Security.AccessControl;
using Newtonsoft.Json;

namespace PSCerts.Config
{
    public class CertPermissions : IValidate
    {
        [JsonProperty("identity", Required = Required.Always)]
        public string Identity { get; set; }
        [JsonProperty("rights", Required = Required.Always)]
        public FileSystemRights FileSystemRights { get; set; }
        [JsonProperty("access")]
        public AccessControlType AccessType { get; set; } = AccessControlType.Allow;

        public FileSystemAccessRule ToAccessRule()
        {
            return new (Identity, FileSystemRights, AccessType);
        }

        public ValidationResult Validate()
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(Identity)) result.Add($"{nameof(Identity)} is required.");

            return result;
        }
    }
}
