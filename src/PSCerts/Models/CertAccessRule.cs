using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using Newtonsoft.Json;

namespace PSCerts
{
    public class CertAccessRule
    {
        [JsonProperty("identity")]
        public string Identity { get; }

        [JsonProperty("rights")]
        public FileSystemRights FileSystemRights => AccessRule.FileSystemRights;

        [JsonProperty("access")]
        public AccessControlType AccessType => AccessRule.AccessControlType;

        [JsonProperty("isInherited")]
        public bool IsInherited => AccessRule.IsInherited;

        [JsonProperty("accessRule")]
        public FileSystemAccessRule AccessRule { get;}

        [JsonProperty("sid")]
        public SecurityIdentifier SID { get; }

        public CertAccessRule(FileSystemAccessRule accessRule)
        {
            AccessRule = accessRule ?? throw new ArgumentNullException(nameof(accessRule));
            SID = accessRule.IdentityReference as SecurityIdentifier;
            Identity = SID?.GetAccountName() ?? accessRule.IdentityReference.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            //var inheritedSymbol = IsInherited ? " (I)" : "";
            //sb.Append("{");
            sb.AppendFormat("[{0}] {1}", AccessType.ToString(), Identity);
            //sb.Append(Identity);
            //sb.Append("}");

            return sb.ToString();
        }
    }
}
