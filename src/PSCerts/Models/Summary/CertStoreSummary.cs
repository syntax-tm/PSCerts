using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PSCerts.Summary
{
    public class CertStoreSummary
    {
        public StoreLocation Location { get; set; }
        public string Name { get; set; }
        public List<CertSummaryItem> Certs { get; set; }
        public bool IsEmpty => Certs.Any();
    }
}
