using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;

namespace PSCerts.Config
{
    [DebuggerDisplay("{Certs}")]
    public class CertImportConfig
    {
        [JsonProperty("certs")]
        public List<CertConfig> Certs { get; set; }
    }
}
