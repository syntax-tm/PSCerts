using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
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
    /// Add-CertPermissions -Certificate $cert -Identity "Network Service" -FileSystemRights FullControl -AccessType Allow
    /// </code>
    /// </example>
    /// <seealso cref="X509Certificate2" />
    /// <seealso cref="FileSystemAccessRule" />
    /// <seealso cref="RSA" />
    [Cmdlet(VerbsCommon.Add, "CertPermissions", DefaultParameterSetName = PROPS_PARAM_SET)]
    [OutputType(typeof(FileSecurity))]
    public class AddCertPermissionsCommand : PSCmdlet
    {
        private const string PROPS_PARAM_SET = nameof(PROPS_PARAM_SET);
        private const string PROPS_DENY_PARAM_SET = nameof(PROPS_DENY_PARAM_SET);
        private const string ACCESS_RULE_PARAM_SET = nameof(ACCESS_RULE_PARAM_SET);
        
        /// <summary>
        /// The <see cref="X509Certificate2"/> with a private key to add permissions.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = PROPS_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = PROPS_DENY_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = ACCESS_RULE_PARAM_SET)]
        [Alias("Cert")]
        public X509Certificate2 Certificate { get; set; }
        
        /// <summary>
        /// The <see cref="FileSystemAccessRule"/> to be added.
        /// </summary>
        /// <seealso cref="System.Security.AccessControl.FileSystemAccessRule" />
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ACCESS_RULE_PARAM_SET)]
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
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = PROPS_DENY_PARAM_SET)]
        [Alias("Account", "Name", "User", "UserName")]
        public string Identity { get; set; }
        
        /// <summary>
        /// The <see cref="FileSystemRights"/> of the new <see cref="FileSystemAccessRule"/>.
        /// </summary>
        /// <seealso cref="FileSystemRights"/>
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = PROPS_PARAM_SET)]
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = PROPS_DENY_PARAM_SET)]
        [Alias("Rights", "Permissions")]
        public FileSystemRights FileSystemRights { get; set; }
        
        /// <summary>
        /// The <see cref="AccessControlType" /> of the new <see cref="FileSystemAccessRule"/>.
        /// </summary>
        /// <remarks>The default type is <c>Allow</c>.</remarks>
        /// <seealso cref="AccessControlType"/>
        [Parameter(Mandatory = true, Position = 3, ParameterSetName = PROPS_PARAM_SET)]
        [Alias("Access", "Type")]
        public AccessControlType AccessType { get; set; } = AccessControlType.Allow;

        /// <summary>
        /// Adds a <see cref="AccessControlType.Deny" /> <see cref="FileSystemAccessRule"/>.
        /// </summary>
        [Parameter(Position = 3, ParameterSetName = PROPS_DENY_PARAM_SET)]
        public SwitchParameter Deny { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var privateKeyFile = PrivateKeyHelper.GetPrivateKey(Certificate);
                var access = Deny.IsPresent
                    ? AccessControlType.Deny
                    : AccessType;
                
                var rule = ParameterSetName switch
                {
                    PROPS_PARAM_SET or
                    PROPS_DENY_PARAM_SET  => new (Identity, FileSystemRights, access),
                    ACCESS_RULE_PARAM_SET => Rule,
                    _                     => throw new ArgumentException($"Unknown {nameof(ParameterSetName)} {ParameterSetName}.")
                };
                
                var acl = FileSystemHelper.AddAccessControl(privateKeyFile, rule);

                WriteObject(acl);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
        }
    }
}
