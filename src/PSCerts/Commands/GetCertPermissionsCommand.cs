using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using PSCerts.Util;

namespace PSCerts.Commands
{
    [Cmdlet(VerbsCommon.Get, "CertPermissions")]
    [OutputType(typeof(List<CertAccessRule>))]
    public class GetCertPermissionsCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Alias("Cert")]
        public X509Certificate2 Certificate { get; set; }
        
        protected override void ProcessRecord()
        {
            try
            {
                var privateKeyFile = PrivateKeyHelper.GetPrivateKey(Certificate);
                var privateKeyInfo = new FileInfo(privateKeyFile);
                var acl = privateKeyInfo.GetAccessControl(AccessControlSections.All);

                var rules = acl.GetAccessRules(true, true, typeof(SecurityIdentifier));

                var perms = rules.AsList<FileSystemAccessRule>()
                    .Select(r => new CertAccessRule(r))
                    .ToList();
            
                WriteObject(perms, false);
            }
            catch (Exception e)
            {
                var error = ErrorHelper.CreateError(e);
                ThrowTerminatingError(error);
            }
        }
    }
}
