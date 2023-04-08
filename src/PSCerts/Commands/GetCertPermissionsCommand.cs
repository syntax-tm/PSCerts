using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using PSCerts.Util;

namespace PSCerts.Commands
{
    [Cmdlet(VerbsCommon.Get, "CertPermissions", DefaultParameterSetName = CERT_PARAM_SET)]
    [OutputType(typeof(List<CertAccessRule>))]
    public class GetCertPermissionsCommand : CmdletBase
    {
        private const string CERT_PARAM_SET = nameof(CERT_PARAM_SET);
        private const string FIND_PARAM_SET = nameof(FIND_PARAM_SET);

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
                   ValueFromPipelineByPropertyName = true, ParameterSetName = CERT_PARAM_SET)]
        [Alias("Cert", "InputObject")]
        [ValidateNotNull]
        public X509Certificate2 Certificate { get; set; }
        
        /// <summary>
        /// The <see cref="X509Certificate2.Thumbprint" /> of the <see cref="X509Certificate2" />.
        /// </summary>
        /// <seealso cref="X509Certificate2.Thumbprint" />
        [Parameter(Mandatory = true,  Position = 0, ValueFromPipeline = true,
                   ValueFromPipelineByPropertyName = true, ParameterSetName = FIND_PARAM_SET)]
        [Alias("CertHash", "Hash")]
        [ValidateNotNullOrEmpty]
        public string Thumbprint { get; set; }
        
        protected override void ProcessRecord()
        {
            try
            {
                var cert = Certificate ?? CertHelper.FindCertificate(Thumbprint);
                var privateKeyFile = PrivateKeyHelper.GetPrivateKey(cert);
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
