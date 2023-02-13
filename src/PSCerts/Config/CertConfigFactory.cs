using System;
using System.IO;
using Newtonsoft.Json;
using PSCerts.Util;

namespace PSCerts.Config
{
    public static class CertConfigFactory
    {
        public static CertImportConfig Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException($"The {nameof(CertImportConfig)} file does not exist.", path);

            var fullPath = FileSystemHelper.ResolvePath(path);
            var configText = File.ReadAllText(fullPath.FullName);

            var config = JsonConvert.DeserializeObject<CertImportConfig>(configText);
            
            if (config == null) throw new JsonException($"Config file '{fullPath}' format is not valid.");

            return config;
        }
    }
}
