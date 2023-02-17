using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using PSCerts.Util;

namespace PSCerts.Commands
{
    [Cmdlet(VerbsCommon.Get, "CertPermissions")]
    [OutputType(typeof(List<CertAccessRule>))]
    public class GetCertPermissionsCommand : CmdletBase
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Alias("Cert", "InputObject")]
        public X509Certificate2 Certificate { get; set; }
        
        protected override void ProcessRecord()
        {
            try
            {
                var privateKeyFile = PrivateKeyHelper.GetPrivateKey(Certificate);
                var rules = FileSystemHelper.GetAccessRules(privateKeyFile);
                var perms = CertAccessRule.Create(rules);
            
                WriteObject(perms, false);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
        }
    }
}
