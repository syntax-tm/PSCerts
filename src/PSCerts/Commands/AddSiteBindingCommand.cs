using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Web.Administration;
using PSCerts.Util;

namespace PSCerts.Commands
{
    [Cmdlet(VerbsCommon.Add, "SiteBinding", DefaultParameterSetName = CERT_PARAM_SET)]
    public class AddSiteBindingCommand : PSCmdlet
    {
        private const string CERT_PARAM_SET = nameof(CERT_PARAM_SET);
        private const string FROM_FILE_SET = nameof(FROM_FILE_SET);
        private const string FROM_FILE_SECURE_SET = nameof(FROM_FILE_SECURE_SET);

        private const string BINDING_INFO_HELP = "The IP address, port, and host name for the binding in a colon-delimited string (\"{IP}:{PORT}:{HOSTNAME}\").";

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = CERT_PARAM_SET)]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = FROM_FILE_SECURE_SET)]
        [Alias("File", "Path")]
        [ValidateNotNullOrEmpty()]
        public string FilePath { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = FROM_FILE_SET)]
        [Alias("Pass")]
        [ValidateNotNullOrEmpty()]
        public string Password { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = FROM_FILE_SECURE_SET)]
        [Alias("Password", "Pass")]
        [ValidateNotNull()]
        public SecureString SecurePassword { get; set; }

        [Parameter(Position = 1, ParameterSetName = CERT_PARAM_SET)]
        [Parameter(Position = 2, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Position = 2, ParameterSetName = FROM_FILE_SECURE_SET)]
        [Alias("Name")]
        [ValidateNotNullOrEmpty()]
        public string Site { get; set; } = "Default Web Site";

        [Parameter(Position = 2, ParameterSetName = CERT_PARAM_SET)]
        [Parameter(Position = 3, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Position = 3, ParameterSetName = FROM_FILE_SECURE_SET)]
        public ushort Port { get; set; } = 443;

        [Parameter(Position = 3, ParameterSetName = CERT_PARAM_SET, HelpMessage = BINDING_INFO_HELP)]
        [Parameter(Position = 4, ParameterSetName = FROM_FILE_SET, HelpMessage = BINDING_INFO_HELP)]
        [Parameter(Position = 4, ParameterSetName = FROM_FILE_SECURE_SET, HelpMessage = BINDING_INFO_HELP)]
        [Alias("Binding","Info")]
        [ValidateNotNullOrEmpty()]
        public string BindingInformation { get; set; } = "*:443:";

        [Parameter(Position = 4, ParameterSetName = CERT_PARAM_SET)]
        [Parameter(Position = 5, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Position = 5, ParameterSetName = FROM_FILE_SECURE_SET)]
        [ValidateNotNullOrEmpty()]
        public string Protocol { get; set; } = "https";

        [Parameter(Position = 5, ParameterSetName = CERT_PARAM_SET)]
        [Parameter(Position = 6, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Position = 6, ParameterSetName = FROM_FILE_SECURE_SET)]
        public StoreName Store { get; set; } = StoreName.My;

        [Parameter(Position = 6, ParameterSetName = CERT_PARAM_SET)]
        [Parameter(Position = 7, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Position = 7, ParameterSetName = FROM_FILE_SECURE_SET)]
        public SslFlags SslFlags { get; set; } = SslFlags.None;

        protected override void ProcessRecord()
        {
            X509Store store = null;
            try
            {
                var cert = ParameterSetName switch
                {
                    CERT_PARAM_SET       => Certificate,
                    FROM_FILE_SET        => new X509Certificate2(FilePath, Password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable),
                    FROM_FILE_SECURE_SET => new X509Certificate2(FilePath, SecurePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable),
                    _                    => throw new ArgumentException($"Unknown parameter set {ParameterSetName}."),
                };

                store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);

                store.Add(cert);

                var mgr = new ServerManager();
                var site = mgr.Sites.Single(s => s.Name.EqualsIgnoreCase(Site));

                var binding = site.Bindings.Add(BindingInformation, Protocol);
                binding.CertificateHash = cert.GetCertHash();
                binding.SslFlags = SslFlags;
                binding.CertificateStoreName = Enum.GetName(typeof(StoreName), Store);

                mgr.CommitChanges();
            }
            catch (Exception e)
            {
                var error = ErrorHelper.CreateError(e);
                ThrowTerminatingError(error);
            }
            finally
            {
                store?.Close();
            }
        }
    }
}
