using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PSCerts.Config
{
    public static class CertConfigFactory
    {
        public static CertConfig Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException($"The {nameof(CertConfig)} file does not exist.", path);

            var configText = File.ReadAllText(path);

            var config = JsonConvert.DeserializeObject<CertConfig>(configText);

            return config;
        }
    }
}
