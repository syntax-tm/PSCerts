using System;
using System.Management.Automation;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using PSCerts.Util;

namespace PSCerts.Commands
{
    /// <summary>
    /// Updates an <see cref="X509Certificate2"/> <see cref="X509Certificate2.FriendlyName"/>.
    /// </summary>
    /// <example>
    /// <para>Selects an <see cref="X509Certificate2"/> from the <see cref="StoreName.My"/> store in <see cref="StoreLocation.LocalMachine"/>.</para>
    /// <para>Then calls <c>Set-CertFriendlyName</c> to change the <see cref="X509Certificate2.FriendlyName"/> to "My Test Cert".</para>
    /// <code>
    /// $cert = Get-Item Cert:\LocalMachine\My\10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae
    /// Set-CertFriendlyName -Certificate $cert -FriendlyName "My Test Cert"
    /// </code>
    /// </example>
    /// <seealso cref="X509Certificate2" />
    /// <seealso cref="X509Certificate2.FriendlyName" />
    [Cmdlet(VerbsCommon.Set, "CertFriendlyName", DefaultParameterSetName = CERT_PARAM_SET)]
    [OutputType(typeof(X509Certificate2))]
    public class SetCertFriendlyNameCommand : CmdletBase
    {
        private const string CERT_PARAM_SET = nameof(CERT_PARAM_SET);
        private const string HASH_PARAM_SET = nameof(HASH_PARAM_SET);
        
        /// <summary>
        /// The <see cref="X509Certificate2"/> with a private key to add permissions.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = CERT_PARAM_SET)]
        [Alias("Cert")]
        public X509Certificate2 Certificate { get; set; }
        
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = HASH_PARAM_SET)]
        [Alias("CertHash", "Hash")]
        [ValidateNotNullOrEmpty]
        public string Thumbprint { get; set; }
        
        /// <summary>
        /// The new <see cref="X509Certificate2.FriendlyName"/> value.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = CERT_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = HASH_PARAM_SET)]
        [Alias("Name", "FN")]
        public string FriendlyName { get; set; }
        
        protected override void ProcessRecord()
        {
            try
            {
                var cert = Certificate ?? CertHelper.FindCertificate(Thumbprint);                
                cert.FriendlyName = FriendlyName;

                WriteObject(cert);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
        }
    }
}
