﻿using System;
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
    [Cmdlet(VerbsCommon.Get, "CertSummary")]
    [OutputType(typeof(CertSummary))]
    //[OutputType(typeof(List<CertSummary>))]
    public class GetCertSummaryCommand : CmdletBase
    {
        protected override void ProcessRecord()
        {
            try
            {
                var summary = CertHelper.GetCertSummary();

                WriteObject(summary.Items, true);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
        }
    }
}
