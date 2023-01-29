using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;

namespace PSCerts
{
    [Cmdlet(VerbsCommon.Add, "CertPermissions", DefaultParameterSetName = PROPS_PARAM_SET)]
    [OutputType(typeof(FileSecurity))]
    public class AddCertPermissionsCommand : PSCmdlet
    {
        private const string PROPS_PARAM_SET = nameof(PROPS_PARAM_SET);
        private const string PROPS_DENY_PARAM_SET = nameof(PROPS_DENY_PARAM_SET);
        private const string ACCESS_RULE_PARAM_SET = nameof(ACCESS_RULE_PARAM_SET);
        
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
                   ValueFromPipelineByPropertyName = true, ParameterSetName = PROPS_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
                   ValueFromPipelineByPropertyName = true, ParameterSetName = PROPS_DENY_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
                   ValueFromPipelineByPropertyName = true, ParameterSetName = ACCESS_RULE_PARAM_SET)]
        public X509Certificate2 Certificate { get; set; }
        
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ACCESS_RULE_PARAM_SET)]
        public FileSystemAccessRule FileSystemAccessRule { get; set; }
        
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = PROPS_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = PROPS_DENY_PARAM_SET)]
        [Alias("Account","Name","User","UserName")]
        public string Identity { get; set; }
        
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = PROPS_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = PROPS_DENY_PARAM_SET)]
        [Alias("Rights","Permissions")]
        public FileSystemRights FileSystemRights { get; set; }
        
        [Parameter(Mandatory = true, Position = 3, ParameterSetName = PROPS_PARAM_SET)]
        [Alias("Access")]
        public AccessControlType AccessType { get; set; } = AccessControlType.Allow;
        
        [Parameter(Position = 3, ParameterSetName = PROPS_DENY_PARAM_SET)]
        public SwitchParameter Deny { get; set; }

        protected override void ProcessRecord()
        {
            var privateKeyFile = PrivateKeyHelper.GetPrivateKey(Certificate);
            var privateKeyInfo = new FileInfo(privateKeyFile);
            var acl = privateKeyInfo.GetAccessControl();

            var rule = ParameterSetName switch
            {
                PROPS_PARAM_SET       => new (Identity, FileSystemRights, AccessType),
                PROPS_DENY_PARAM_SET  => new (Identity, FileSystemRights, AccessControlType.Deny),
                ACCESS_RULE_PARAM_SET => FileSystemAccessRule,
                _                     => throw new ArgumentException($"Unknown {nameof(ParameterSetName)} {ParameterSetName}.")
            };

            acl.AddAccessRule(rule);
            
            WriteObject(acl);
        }
    }
}
