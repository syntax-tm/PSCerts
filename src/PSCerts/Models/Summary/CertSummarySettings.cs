using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PSCerts.Summary
{
    public class CertSummarySettings
    {
        public List<StoreLocation> Locations { get; set; }
        public List<StoreName> Stores { get; set; }
        public bool Detailed { get; set; }
        public bool WithPrivateKey { get; set; }
    }
}
