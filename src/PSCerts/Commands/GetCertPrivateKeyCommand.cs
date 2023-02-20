using System;
using System.IO;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using PSCerts.Util;

namespace PSCerts.Commands
{
    [Cmdlet(VerbsCommon.Get, "CertPrivateKey", DefaultParameterSetName = CERT_PARAM_SET)]
    [OutputType(typeof(FileInfo))]
    public class GetCertPrivateKeyCommand : CmdletBase
    {
        private const string CERT_PARAM_SET = nameof(CERT_PARAM_SET);
        private const string FIND_PARAM_SET = nameof(FIND_PARAM_SET);

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, 
                   ValueFromPipelineByPropertyName = true, ParameterSetName = CERT_PARAM_SET)]
        [Alias("Cert", "InputObject")]
        [ValidateNotNull]
        public X509Certificate2 Certificate { get; set; }
        
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
                var privateKeyInfo = new FileInfo(privateKeyFile);

                WriteObject(privateKeyInfo);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
        }
    }
}
