using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using PSCerts.Util;

namespace PSCerts.Commands
{
    [Cmdlet(VerbsCommon.Get, "CertPrivateKey", DefaultParameterSetName = CERT_PARAM_SET)]
    [OutputType(typeof(FileInfo))]
    public class GeCertPrivateKeyCommand : PSCmdlet
    {
        private const string CERT_PARAM_SET = "Certificate";
        private const string FIND_PARAM_SET = "Find";

        [Parameter(Mandatory = true, Position = 0,
                   ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = CERT_PARAM_SET)]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(Mandatory = true, Position = 0,
                   ValueFromPipelineByPropertyName = true, ParameterSetName = FIND_PARAM_SET)]
        public StoreLocation StoreLocation { get; set; }
        [Parameter(Mandatory = true, Position = 1,
                   ValueFromPipelineByPropertyName = true, ParameterSetName = FIND_PARAM_SET)]
        public StoreName StoreName { get; set; }

        [Parameter(Mandatory = true,  Position = 2,
                   ValueFromPipelineByPropertyName = true, ParameterSetName = FIND_PARAM_SET)]
        [Alias("Thumbprint", "CN", "CommonName", "Subject", "SubjectName")]
        [ValidateNotNullOrEmpty]
        public string Key { get; set; }

        [Parameter(Mandatory = true,
                   Position = 3,
                   ValueFromPipelineByPropertyName = true,
                   ParameterSetName = FIND_PARAM_SET)]
        public X509FindType FindType { get; set; } = X509FindType.FindByThumbprint;

        protected override void ProcessRecord()
        {
            try
            {
                var privateKeyFile = ParameterSetName switch
                {
                    CERT_PARAM_SET => PrivateKeyHelper.GetPrivateKey(Certificate),
                    FIND_PARAM_SET => PrivateKeyHelper.GetPrivateKey(StoreLocation, StoreName, Key, FindType),
                    _              => throw new ArgumentException($"Unknown parameter set {ParameterSetName}."),
                };
                var privateKeyInfo = new FileInfo(privateKeyFile);

                WriteObject(privateKeyInfo);
            }
            catch (Exception e)
            {
                var error = ErrorHelper.CreateError(e);
                ThrowTerminatingError(error);
            }
        }
    }
}
