using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PSCerts.Config
{
    [DebuggerDisplay("{Certs}")]
    public class CertImportConfig : IValidate
    {
        [JsonProperty("certs")]
        public List<CertConfig> Certs { get; set; }

        public ValidationResult Validate()
        {
            if (Certs == null || Certs.Count == 0) return new (@"Import configuration contains no certificates.");

            var results = Certs.Select(c => c.Validate());
            return new (results);
        }
    }
}
