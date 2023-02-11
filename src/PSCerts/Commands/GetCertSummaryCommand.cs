using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using PSCerts.Summary;
using PSCerts.Util;

namespace PSCerts.Commands
{
    /// <summary>
    /// <para type="synopsis">Returns information about installed certificates.</para>
    /// <para type="description">A <see cref="CertSummaryItem"/> containing information about currently installed certificates.</para>
    /// </summary>
    /// <seealso cref="CertSummaryItem" />
    /// <seealso cref="X509Certificate2" />
    [Cmdlet(VerbsCommon.Get, "CertSummary", DefaultParameterSetName = DEFAULT_PARAM_SET)]
    [OutputType(typeof(CertSummary))]
    //[OutputType(typeof(List<CertSummary>))]
    public class GetCertSummaryCommand : PSCmdlet
    {
        private const string DEFAULT_PARAM_SET = nameof(DEFAULT_PARAM_SET);
        private const string CUSTOM_PARAM_SET = nameof(CUSTOM_PARAM_SET);
        private const string DETAILED_RULE_PARAM_SET = nameof(DETAILED_RULE_PARAM_SET);

        /// <summary>
        /// <para type="description">The <see cref="StoreLocation"/>s to search.</para>
        /// </summary>
        [Parameter(Position = 0)]
        public StoreLocation? Location { get; set; }
        
        /// <summary>
        /// <para type="description">The <see cref="X509Store"/>s to search.</para>
        /// </summary>
        [Parameter(Position = 1, ParameterSetName = CUSTOM_PARAM_SET)]
        public StoreName[] Stores { get; set; }
        
        /// <summary>
        /// <para type="description">Returns certificates from every <see cref="X509Store"/> in each <see cref="StoreLocation"/>.</para>
        /// </summary>
        [Parameter(ParameterSetName = DETAILED_RULE_PARAM_SET)]
        public SwitchParameter Detailed { get; set; }

        /// <summary>
        /// <para type="description">Returns only certificates with a private key.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter HasPrivateKey { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var summary = new CertSummary();
                var pkOnly = HasPrivateKey.IsPresent;
                
                List<StoreLocation> locations = Location.HasValue
                    ? new () { Location.Value }
                    : new () { StoreLocation.CurrentUser, StoreLocation.LocalMachine };

                var stores = Detailed.IsPresent
                    ? Enum.GetValues(typeof(StoreName)).AsList<StoreName>()
                    : Stores?.ToList() ?? new () { StoreName.My };

                foreach (var location in locations)
                foreach (var storeName in stores)
                {
                    var store = new X509Store(storeName, location);

                    try
                    {
                        store.Open(OpenFlags.ReadOnly);

                        foreach (var cert in store.Certificates)
                        {
                            try
                            {
                                var summaryItem = new CertSummaryItem(store, cert);

                                if (PrivateKeyHelper.TryGetPrivateKey(cert, out var privateKeyFile))
                                {
                                    var privateKeyInfo = new FileInfo(privateKeyFile);
                                    summaryItem.PrivateKey = privateKeyInfo;

                                    var acl = FileSystemHelper.GetAccessControl(privateKeyFile);
                                    var rules = acl.GetAccessRules(true, true, typeof(SecurityIdentifier));
                                    var perms = CertAccessRule.Create(rules);

                                    summaryItem.Permissions = perms;
                                }
                                else if (pkOnly)
                                {
                                    continue;
                                }

                                summary.Items.Add(summaryItem);
                            }
                            catch (Exception ex)
                            {
                                WriteVerbose($"An exception occurred processing the {nameof(X509Certificate2)} with thumbprint '{cert?.Thumbprint}'. Details: {ex.Message}");
                            }
                        }
                    }
                    finally
                    {
                        store.Close();
                    }
                }

                WriteObject(summary);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
        }
    }
}
