using System;
using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
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
    public class GetCertSummaryCommand : CmdletBase
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
                var pkOnly = HasPrivateKey.IsPresent;
                
                var locations = Location.HasValue
                    ? new [] { Location.Value }
                    : new [] { StoreLocation.CurrentUser, StoreLocation.LocalMachine };

                var stores = Detailed.IsPresent
                    ? Enum.GetValues(typeof(StoreName)).AsList<StoreName>()
                    : Stores?.ToList() ?? new () { StoreName.My };

                var summary = CertHelper.GetCertSummary(locations, stores, pkOnly);

                WriteObject(summary.Items, true);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
        }
    }
}
