using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace PSCerts.Util
{
    public class PrivateKeyHelper
    {
        private static readonly string[] _privateKeyStoreFormats =
        {
            // Cryptography API: Next Generation (CNG)
            @"%APPDATA%\Microsoft\Crypto\Keys",                                     // User private
            @"%ALLUSERSPROFILE%\Application Data\Microsoft\Crypto\SystemKeys",      // Local system private
            @"%WINDIR%\ServiceProfiles\LocalService",                               // Local service private
            @"%WINDIR%\ServiceProfiles\NetworkService",                             // Network service private
            @"%ALLUSERSPROFILE%\Application Data\Microsoft\Crypto\Keys",            // Shared private
            // Legacy CryptoAPI
            @"%APPDATA%\Microsoft\Crypto\RSA\{0}\",                                 // User private
            @"%APPDATA%\Microsoft\Crypto\DSS\{0}\",                                 // User private
            @"%ALLUSERSPROFILE%\Application Data\Microsoft\Crypto\RSA\S-1-5-18\",   // Local system private
            @"%ALLUSERSPROFILE%\Application Data\Microsoft\Crypto\DSS\S-1-5-18\",   // Local system private
            @"%ALLUSERSPROFILE%\Application Data\Microsoft\Crypto\RSA\S-1-5-19\",   // Local service private
            @"%ALLUSERSPROFILE%\Application Data\Microsoft\Crypto\DSS\S-1-5-19\",   // Local service private
            @"%ALLUSERSPROFILE%\Application Data\Microsoft\Crypto\RSA\S-1-5-20\",   // Network service private
            @"%ALLUSERSPROFILE%\Application Data\Microsoft\Crypto\DSS\S-1-5-20\",   // Network service private
            @"%ALLUSERSPROFILE%\Application Data\Microsoft\Crypto\RSA\MachineKeys", // Shared private
            @"%ALLUSERSPROFILE%\Application Data\Microsoft\Crypto\DSS\MachineKeys"  // Shared private
        };

        private static List<string> _keyContainers;
        protected static List<string> KeyContainers
        {
            get
            {
                if (_keyContainers != null) return _keyContainers;

                _keyContainers = GetPrivateKeyStores();

                return _keyContainers;
            }
        }

        public static string GetPrivateKey(string thumbprint)
        {
            try
            {
                var cert = CertHelper.FindCertificate(thumbprint);
                return GetPrivateKey(cert);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"An error occurred attempting to find the private key certificate '{thumbprint}'. {e.Message}", e);
            }
        }

        public static string GetPrivateKey(X509Certificate2 cert)
        {
            try
            {
                var privateKeyFile = GetPrivateKeyName(cert);
                return LocatePrivateKey(privateKeyFile);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"An error occurred attempting to find the private key. {e.Message}", e);
            }
        }
        
        public static bool TryGetPrivateKey(string thumbprint, out string filePath)
        {
            filePath = null;

            try
            {
                if (string.IsNullOrWhiteSpace(thumbprint)) return false;

                var cert = CertHelper.FindCertificate(thumbprint);
                return TryGetPrivateKey(cert, out filePath);
            }
            catch
            {
                return false;
            }
        }

        public static bool TryGetPrivateKey(X509Certificate2 cert, out string filePath)
        {
            filePath = null;

            try
            {
                if (!cert.HasPrivateKey) return false;

                var privateKeyFile = GetPrivateKeyName(cert);
                filePath = LocatePrivateKey(privateKeyFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetPrivateKeyName(X509Certificate2 cert)
        {
            if (cert == null) throw new ArgumentNullException(nameof(cert));
            if (!cert.HasPrivateKey) return null;
    
            var rsaPk = cert.GetRSAPrivateKey();

            if (rsaPk != null)
            {
                return rsaPk switch
                {
                    RSACng rsaCngPk                 => rsaCngPk.Key.UniqueName,
                    RSACryptoServiceProvider rsaCsp => rsaCsp.CspKeyContainerInfo.UniqueKeyContainerName,
                    _                               => throw new NotSupportedException($"{nameof(RSA)} private key type {rsaPk.KeyExchangeAlgorithm} is not currently supported.")
                };
            }

            var ecdPk = cert.GetECDsaPrivateKey();
            if (ecdPk is ECDsaCng ecdCng)
            {
                return ecdCng.Key.UniqueName;
            }
            
            throw new InvalidOperationException($"A private key for the {nameof(X509Certificate)} was not found.");
        }

        private static string LocatePrivateKey(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName)) throw new ArgumentNullException(nameof(keyName));

            foreach (var location in KeyContainers)
            {
                try
                {
                    if (!Directory.Exists(location)) continue;

                    var results = Directory.GetFiles(location, $"*{keyName}*", SearchOption.AllDirectories);
                    if (!results.Any()) continue;
                    return results.First();
                }
                catch (Exception e)
                {
                    PowerShellHelper.Debug($"An error occurred searching '{location}' for private key '{keyName}'. {e}");
                }
            }

            throw new ArgumentException($"Unable to find private key '{keyName}'.", nameof(keyName));
        }

        private static List<string> GetPrivateKeyStores()
        {
            var currentUser = WindowsIdentity.GetCurrent();
            var sid = currentUser.User?.Value;

            Debug.Assert(sid != null, $"Current {nameof(WindowsIdentity)} is null.");

            var stores = _privateKeyStoreFormats.Select(s => Environment.ExpandEnvironmentVariables(string.Format(s, sid)));
            return stores.ToList();
        }
    }
}
