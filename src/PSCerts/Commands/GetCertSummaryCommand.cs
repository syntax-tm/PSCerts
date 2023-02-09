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
using PSCerts.Config;
using PSCerts.Summary;
using PSCerts.Util;

namespace PSCerts.Commands
{
    [Cmdlet(VerbsCommon.Get, "CertSummary", DefaultParameterSetName = DEFAULT_PARAM_SET)]
    [OutputType(typeof(List<CertSummaryItem>))]
    //[OutputType(typeof(List<CertSummary>))]
    public class GetCertSummaryCommand : PSCmdlet
    {
        private const string DEFAULT_PARAM_SET = nameof(DEFAULT_PARAM_SET);
        private const string CUSTOM_PARAM_SET = nameof(CUSTOM_PARAM_SET);
        private const string DETAILED_RULE_PARAM_SET = nameof(DETAILED_RULE_PARAM_SET);

        [Parameter(Position = 0)]
        public StoreLocation? Location { get; set; }

        [Parameter(Position = 1, ParameterSetName = CUSTOM_PARAM_SET)]
        public StoreName[] Stores { get; set; }

        [Parameter(ParameterSetName = DETAILED_RULE_PARAM_SET)]
        public SwitchParameter Detailed { get; set; }
        
        [Parameter]
        public SwitchParameter HasPrivateKey { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var certs = new List<CertSummaryItem>();
                var pkOnly = HasPrivateKey.IsPresent;

                List<StoreName> stores;
                List<StoreLocation> locations = Location.HasValue
                    ? new () { Location.Value }
                    : new () { StoreLocation.CurrentUser, StoreLocation.LocalMachine };

                if (Detailed.IsPresent)
                {
                    stores = Enum.GetValues(typeof(StoreName)).AsList<StoreName>();
                }
                else if (Stores?.Any() ?? false)
                {
                    stores = new (Stores);
                }
                else
                {
                    stores = new ()
                    {
                        StoreName.My
                    };
                }
                
                foreach (var location in locations)
                foreach (var storeName in stores)
                {
                    var store = new X509Store(storeName, location);

                    try
                    {
                        store.Open(OpenFlags.ReadOnly);

                        foreach (var cert in store.Certificates)
                        {
                            var summary = new CertSummaryItem(store, cert);

                            if (PrivateKeyHelper.TryGetPrivateKey(cert, out var privateKeyFile))
                            {
                                var privateKeyInfo = new FileInfo(privateKeyFile);
                                summary.PrivateKey = privateKeyInfo;
                                
                                var acl = FileSystemHelper.GetAccessControl(privateKeyFile);
                                var rules = acl.GetAccessRules(true, true, typeof(SecurityIdentifier));
                                var perms = CertAccessRule.Create(rules);

                                summary.Permissions = perms;
                            }
                            else if (pkOnly)
                            {
                                continue;
                            }

                            certs.Add(summary);
                        }
                    }
                    finally
                    {
                        store.Close();
                    }
                }

                WriteObject(certs, true);
            }
            catch (Exception e)
            {
                var error = ErrorHelper.CreateError(e);
                ThrowTerminatingError(error);
            }
        }
    }
}
