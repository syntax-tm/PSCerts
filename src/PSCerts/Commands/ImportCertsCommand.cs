using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using Newtonsoft.Json;
using PSCerts.Config;
using PSCerts.Util;

namespace PSCerts.Commands
{
    [Cmdlet(VerbsData.Import, "CertConfig")]
    //[OutputType(typeof(List<CertSummary>))]
    public class ImportCertsCommand : PSCmdlet
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
                if (ParameterSetName == FROM_FILE_PARAM_SET)
                {
                    if (string.IsNullOrWhiteSpace(FilePath)) throw new ArgumentNullException(nameof(FilePath));

                    var path = FileSystemHelper.ResolvePath(FilePath);
                    var configText = File.ReadAllText(path.FullName);

                    var config = JsonConvert.DeserializeObject<CertImportConfig>(configText);
                    if (config == null) throw new JsonException($"Config file format is not valid.");


                }
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
        }
    }
}
