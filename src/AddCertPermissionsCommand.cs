using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace PSCerts
{
    [Cmdlet(VerbsCommon.Add, "CertPermissions")]
    [OutputType(typeof(AuthorizationRuleCollection))]
    public class GetCertPermissionsCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public X509Certificate2 Certificate { get; set; }
        
        [Parameter]
        public SwitchParameter Explicit { get; set; }

        [Parameter]
        public SwitchParameter Inherited { get; set; }

        protected override void ProcessRecord()
        {
            var privateKeyFile = PrivateKeyHelper.GetPrivateKey(Certificate);
            var privateKeyInfo = new FileInfo(privateKeyFile);
            var acl = privateKeyInfo.GetAccessControl();

            var rules = acl.GetAccessRules(Explicit.IsPresent, Inherited.IsPresent, typeof(NTAccount));
            
            WriteObject(rules, false);
        }
    }
}
