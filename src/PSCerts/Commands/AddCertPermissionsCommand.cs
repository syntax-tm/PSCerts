using System;
using System.Management.Automation;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using PSCerts.Util;
using System.Security.Principal;

namespace PSCerts.Commands
{
    /// <summary>
    /// Adds a new <see cref="FileSystemAccessRule" /> on a <see cref="Certificate"/>'s private key.
    /// </summary>
    /// <example>
    /// <para>Selects an <see cref="X509Certificate2"/> from the <see cref="StoreName.My"/> store in <see cref="StoreLocation.LocalMachine"/>.</para>
    /// <para>Then calls <c>Add-CertPermissions</c> to grant <c>"NETWORK SERVICE"</c> account <c>FullControl</c> to the <c>$cert</c>'s private key.</para>
    /// <code>
    /// $cert = Get-Item Cert:\LocalMachine\My\10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae
    /// Add-CertPermissions -Certificate $cert -Identity "NETWORK SERVICE" -FileSystemRights FullControl -AccessType Allow
    /// </code>
    /// </example>
    /// <seealso cref="X509Certificate2" />
    /// <seealso cref="FileSystemAccessRule" />
    /// <seealso cref="RSA" />
    [Cmdlet(VerbsCommon.Add, "CertPermissions", DefaultParameterSetName = PROPS_PARAM_SET)]
    [OutputType(typeof(void))]
    public class AddCertPermissionsCommand : CmdletBase
    {
        private const string PROPS_PARAM_SET = nameof(PROPS_PARAM_SET);
        private const string ACCESS_RULE_PARAM_SET = nameof(ACCESS_RULE_PARAM_SET);
        private const string HASH_PROPS_PARAM_SET = nameof(HASH_PROPS_PARAM_SET);
        private const string HASH_ACCESS_RULE_PARAM_SET = nameof(HASH_ACCESS_RULE_PARAM_SET);
        
        /// <summary>
        /// The <see cref="X509Certificate2"/> with a private key to add permissions.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = PROPS_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = ACCESS_RULE_PARAM_SET)]
        [Alias("Cert")]
        public X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// The <see cref="X509Certificate2.Thumbprint" /> of the <see cref="X509Certificate2" />.
        /// </summary>
        /// <seealso cref="X509Certificate2.Thumbprint" />
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = HASH_PROPS_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = HASH_ACCESS_RULE_PARAM_SET)]
        [Alias("CertHash", "Hash")]
        [ValidateNotNullOrEmpty]
        public string Thumbprint { get; set; }
        
        /// <summary>
        /// The <see cref="FileSystemAccessRule"/> to be added.
        /// </summary>
        /// <seealso cref="FileSystemAccessRule" />
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ACCESS_RULE_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = HASH_ACCESS_RULE_PARAM_SET)]
        [Alias("FileSystemAccessRule", "AccessRule")]
        public FileSystemAccessRule Rule { get; set; }
        
        /// <summary>
        /// The <see cref="NTAccount"/> name of the user or group.
        /// </summary>
        /// <seealso cref="WindowsIdentity"/>
        /// <seealso cref="WindowsPrincipal"/>
        /// <seealso cref="NTAccount"/>
        /// <example>"NETWORK SERVICE"</example>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = PROPS_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = HASH_PROPS_PARAM_SET)]
        [Alias("Account", "Name", "User", "UserName")]
        public string Identity { get; set; }
        
        /// <summary>
        /// The <see cref="FileSystemRights"/> of the new <see cref="FileSystemAccessRule"/>.
        /// </summary>
        /// <seealso cref="FileSystemRights"/>
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = PROPS_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = HASH_PROPS_PARAM_SET)]
        [Alias("Rights", "Permissions")]
        public FileSystemRights FileSystemRights { get; set; }

        /// <summary>
        /// The <see cref="AccessControlType" /> of the new <see cref="FileSystemAccessRule"/>.
        /// </summary>
        /// <remarks>The default type is <c>Allow</c>.</remarks>
        /// <seealso cref="AccessControlType"/>
        [Parameter(Position = 3, ParameterSetName = PROPS_PARAM_SET)]
        [Parameter(Position = 3, ParameterSetName = HASH_PROPS_PARAM_SET)]
        [Alias("Access", "Type")]
        public AccessControlType AccessType { get; set; } = AccessControlType.Allow;

        protected override void ProcessRecord()
        {
            try
            {
                var cert = Certificate ?? CertHelper.FindCertificate(Thumbprint);
                var privateKeyFile = PrivateKeyHelper.GetPrivateKey(cert);
                var rule = Rule ?? new (Identity, FileSystemRights, AccessType);

                FileSystemHelper.AddAccessControl(privateKeyFile, rule);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
        }
    }
}
