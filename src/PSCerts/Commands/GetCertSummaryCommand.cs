using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using PSCerts.Util;

namespace PSCerts.Commands
{
    /// <summary>
    /// <para type="synopsis">Returns information about installed certificates.</para>
    /// <para type="description">A <see cref="CertSummaryItem"/> containing information about currently installed certificates.</para>
    /// </summary>
    /// <seealso cref="CertSummaryItem" />
    /// <seealso cref="X509Certificate2" />
    [Cmdlet(VerbsCommon.Get, "CertSummary")]
    [OutputType(typeof(List<CertSummaryItem>))]
    public class GetCertSummaryCommand : CmdletBase
    {

        [Parameter(Position = 0, HelpMessage = "Show only certificates with a private key.")]
        public SwitchParameter WithPrivateKey { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var summary = CertHelper.GetCertSummary(WithPrivateKey);

                WriteObject(summary, true);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
        }
    }
}
