using System;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Web.Administration;
using PSCerts.Util;

namespace PSCerts.Commands
{
    [Cmdlet(VerbsCommon.Add, "SiteBinding", DefaultParameterSetName = CERT_PARAM_SET)]
    [OutputType(typeof(CertBinding))]
    public class AddSiteBindingCommand : CmdletBase
    {
        private const string CERT_PARAM_SET = nameof(CERT_PARAM_SET);
        private const string FROM_FILE_SET = nameof(FROM_FILE_SET);
        private const string FROM_FILE_SECURE_SET = nameof(FROM_FILE_SECURE_SET);

        private const string HTTPS_PROTOCOL = @"https";
        private const string DEFAULT_BINDING_INFO = @"*:443:";
        private const string DEFAULT_SITE_NAME = @"Default Web Site";

        private const string BINDING_INFO_HELP = "The IP address, port, and host name for the binding in a colon-delimited string (\"{IP}:{PORT}:{HOSTNAME}\").";

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = CERT_PARAM_SET)]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = FROM_FILE_SECURE_SET)]
        [Alias("File", "Path")]
        [ValidateNotNullOrEmpty]
        public string FilePath { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = FROM_FILE_SET)]
        [Alias("Pass")]
        [ValidateNotNullOrEmpty]
        public string Password { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = FROM_FILE_SECURE_SET)]
        [Alias("Password", "Pass")]
        [ValidateNotNull]
        public SecureString SecurePassword { get; set; }

        [Parameter(Position = 1, ParameterSetName = CERT_PARAM_SET)]
        [Parameter(Position = 2, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Position = 2, ParameterSetName = FROM_FILE_SECURE_SET)]
        [Alias("Name")]
        [ValidateNotNullOrEmpty]
        public string Site { get; set; } = DEFAULT_SITE_NAME;
        
        [Parameter(Position = 2, ParameterSetName = CERT_PARAM_SET, HelpMessage = BINDING_INFO_HELP)]
        [Parameter(Position = 3, ParameterSetName = FROM_FILE_SET, HelpMessage = BINDING_INFO_HELP)]
        [Parameter(Position = 3, ParameterSetName = FROM_FILE_SECURE_SET, HelpMessage = BINDING_INFO_HELP)]
        [Alias("Binding","Info")]
        [ValidateNotNullOrEmpty]
        public string BindingInformation { get; set; } = DEFAULT_BINDING_INFO;
        
        [Parameter(Position = 3, ParameterSetName = CERT_PARAM_SET)]
        [Parameter(Position = 4, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Position = 4, ParameterSetName = FROM_FILE_SECURE_SET)]
        public StoreName Store { get; set; } = StoreName.My;

        [Parameter(Position = 4, ParameterSetName = CERT_PARAM_SET)]
        [Parameter(Position = 5, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Position = 5, ParameterSetName = FROM_FILE_SECURE_SET)]
        public SslFlags SslFlags { get; set; } = SslFlags.None;

        protected override void ProcessRecord()
        {
            X509Store store = null;

            try
            {
                var cert = ParameterSetName switch
                {
                    CERT_PARAM_SET       => Certificate,
                    FROM_FILE_SET        => new (FilePath, Password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable),
                    FROM_FILE_SECURE_SET => new (FilePath, SecurePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable),
                    _                    => throw new ArgumentException($"Unknown parameter set {ParameterSetName}."),
                };

                var thumbprint = cert.Thumbprint ?? throw new InvalidOperationException($"{nameof(X509Certificate2)} {nameof(X509Certificate2.Thumbprint)} cannot be null.");

                store = new (Store, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);

                store.Add(cert);

                store.Close();

                var pk = PrivateKeyHelper.GetPrivateKey(cert);
                var mgr = new ServerManager();
                var site = mgr.Sites.Single(s => s.Name.EqualsIgnoreCase(Site));
                
                var appPoolName = site.Applications.FirstOrDefault()?.ApplicationPoolName;
                var appPool = mgr.ApplicationPools.SingleOrDefault(p => p.Name.EqualsIgnoreCase(appPoolName));

                if (appPool == null) throw new InvalidOperationException($"An {nameof(ApplicationPool)} with the name {appPoolName} was not found.");

                var appPoolIdentity = IISHelper.GetApplicationPoolIdentity(appPool);
                var acl = FileSystemHelper.AddAccessControl(pk, appPoolIdentity);

                WriteVerbose($"Added {FileSystemRights.Read} permissions for {appPoolIdentity} on {pk}.");

                var binding = site.Bindings.Add(BindingInformation, HTTPS_PROTOCOL);
                binding.CertificateHash = cert.GetCertHash();
                binding.SslFlags = SslFlags;
                binding.CertificateStoreName = store.Name;
                
                mgr.CommitChanges();

                var summary = new CertBinding
                {
                    Site = site,
                    AppPool = appPool,
                    AppPoolIdentity = appPoolIdentity,
                    Certificate = cert,
                    Location = StoreLocation.LocalMachine,
                    StoreName = Store,
                    PrivateKeySecurity = acl,
                    Binding = binding
                };

                WriteObject(summary);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
            finally
            {
                store?.Close();
            }
        }
    }
}
