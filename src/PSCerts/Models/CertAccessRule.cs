using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using Newtonsoft.Json;
using PSCerts.Util;

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

        [JsonProperty("accessRule")]
        public FileSystemAccessRule AccessRule { get;}

        public NTAccount NTAccount { get; }
        public SecurityIdentifier SID { get; }
        public IdentityReference IdentityReference { get; }

        public bool IsAllow => AccessType == AccessControlType.Allow;
        public bool IsDeny => AccessType == AccessControlType.Deny;
        public bool IsInherited => AccessRule.IsInherited;
        public string FileSystemRightsString => FileSystemHelper.GetShortFileSystemRightsString(FileSystemRights);
        public string IdentityDisplayString
        {
            get
            {
                var identity = Identity;

                identity = identity.Replace($@"{Environment.MachineName}\", string.Empty);
                identity = identity.Replace(@"NT AUTHORITY\", string.Empty);
                identity = identity.Replace(@"BUILTIN\", string.Empty);

                return identity;
            }
        }

        public CertAccessRule(FileSystemAccessRule accessRule)
        {
            AccessRule = accessRule ?? throw new ArgumentNullException(nameof(accessRule));
            IdentityReference = accessRule.IdentityReference;
            NTAccount = accessRule.IdentityReference as NTAccount;
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
            var typeChar = IsAllow ? "+" : "-";
            sb.AppendFormat("{0} {1} {2}", typeChar, FileSystemRights, Identity);
            return sb.ToString();
        }
    }
}
