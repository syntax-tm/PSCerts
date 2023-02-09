using System;
using System.Collections.Generic;
using System.Linq;
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

        public static CertAccessRule Create(FileSystemAccessRule accessRule)
        {
            return new CertAccessRule(accessRule);
        }

        public static List<CertAccessRule> Create(AuthorizationRuleCollection rules)
        {
            var perms = rules
                .AsList<FileSystemAccessRule>()
                .Select(Create)
                .ToList();
            return perms;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[{0}] {1}", AccessType.ToString(), Identity);
            return sb.ToString();
        }
    }
}
