using System;
using System.Collections.Generic;
using System.Text;

namespace PSCerts.Summary
{
    // TODO: Add an option to return a hierarchical structure
    // this is just a placeholder class for now
    public class CertSummary
    {
        public CertSummarySettings Settings { get; set; }
        public List<CertStoreSummary> Stores { get; set; }

        public CertSummary()
        {
            Settings = new ();
            Stores = new ();
        }
    }
}
