using System;
using System.Linq;
using System.Management.Automation;
using PSCerts.Config;
using PSCerts.Util;

namespace PSCerts.Commands
{
    [Cmdlet(VerbsData.Import, "Certs")]
    //[OutputType(typeof(List<CertSummary>))]
    public class ImportCertsCommand : CmdletBase
    {
        private const string FROM_FILE_PARAM_SET = nameof(FROM_FILE_PARAM_SET);
        private const string FROM_CONFIG_PARAM_SET = nameof(FROM_CONFIG_PARAM_SET);

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = FROM_FILE_PARAM_SET)]
        [Alias("ConfigFile")]
        public string FilePath { get; set; }
        
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = FROM_CONFIG_PARAM_SET)]
        [Alias("Config")]
        public CertImportConfig ImportConfig { get; set; }
        
        protected override void ProcessRecord()
        {
            try
            {
                var config = ImportConfig ?? CertConfigFactory.Load(FilePath);

                var validationResult = config.Validate();

                validationResult.AssertIsValid();

                foreach (var certConfig in config.Certs)
                {
                    certConfig.Import();

                    var firstStore = certConfig.Stores.First();

                    if (!certConfig.HasPermissions) continue;

                    var pk = PrivateKeyHelper.GetPrivateKey(certConfig.Thumbprint);
                    if (string.IsNullOrWhiteSpace(pk)) throw new InvalidOperationException($"Private key for '{certConfig.Thumbprint}' was not found. Unable to set permissions.");
                    foreach (var permission in certConfig.Permissions)
                    {
                        FileSystemHelper.AddAccessControl(pk, permission.ToAccessRule());
                    }
                }
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
        }
    }
}
