using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PSCerts.Util
{
    public class PrivateKeyHelper
    {
        /// <summary>
        /// Crypto Service Provider (Microsoft RSA SChannel Cryptographic Provider)
        /// </summary>
        private const string RSA_SCHANNEL_KEYS = @"C:\ProgramData\Application Data\Microsoft\Crypto\Keys";
        /// <summary>
        /// Crypto Next Generation (Microsoft Key Storage Provider)
        /// </summary>
        private const string APPDATA_MS_CRYPTO_KEYS = @"C:\ProgramData\Application Data\Microsoft\Crypto\Keys";
        private const string PROGRAMDATA_MS_CRYPTO_KEYS = @"C:\ProgramData\Microsoft\Crypto";

        private static List<string> _keyContainers;
        public static List<string> KeyContainers
        {
            get
            {
                if (_keyContainers != null) return _keyContainers;

                _keyContainers = GetKeyContainers();

                return _keyContainers;
            }
        }

        public static string GetPrivateKey(StoreLocation location, StoreName storeName, string key, X509FindType findType)
        {
            try
            {
                var cert = GetCertificate(storeName, location, key, findType);
                return GetPrivateKey(cert);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"An error occurred attempting to find the private key for '{key}'. {e.Message}", e);
            }
        }

        public static string GetPrivateKey(X509Certificate2 cert)
        {
            try
            {
                var privateKeyFile = GetKeyFileName(cert);
                return GetKeyFile(privateKeyFile);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"An error occurred attempting to find the private key. {e.Message}", e);
            }
        }
        
        public static bool TryGetPrivateKey(StoreLocation location, StoreName storeName, string key, X509FindType findType, out string filePath)
        {
            filePath = null;

            try
            {
                var cert = GetCertificate(storeName, location, key, findType);
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
                var privateKeyFile = GetKeyFileName(cert);
                filePath = GetKeyFile(privateKeyFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static X509Certificate2 GetCertificate(StoreName storeName, StoreLocation storeLocation, string key, X509FindType findType)
        {
            X509Certificate2 result;

            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            try
            {
                var matches = store.Certificates.Find(findType, key, false);

                if (matches.Count > 1)
                    throw new InvalidOperationException($"More than one certificate with key '{key}' found in the store.");
                if (matches.Count == 0)
                    throw new InvalidOperationException($"No certificates with key '{key}' found in the store.");

                result = matches[0];
            }
            finally
            {
                store.Close();
            }

            return result;
        }
        
        public static string GetKeyFileName(X509Certificate2 cert)
        {
            if (cert == null) throw new ArgumentNullException(nameof(cert));
    
            var algorithms = new List<Type>
            {
                typeof(RSA),
                typeof(ECDsa),
                //typeof(DSA)
            };

            foreach (var alg in algorithms)
            {
                try
                {
                    if (alg == typeof(RSA))
                    {
                        var rsaPk = cert.GetRSAPrivateKey();

                        if (rsaPk == null) continue;

                        return rsaPk switch
                        {
                            RSACng rsaCngPk                 => rsaCngPk.Key.UniqueName,
                            RSACryptoServiceProvider rsaCsp => rsaCsp.CspKeyContainerInfo.UniqueKeyContainerName,
                            _                               => throw new NotSupportedException($"RSA private key type {rsaPk.KeyExchangeAlgorithm} is not currently supported.")
                        };
                    }

                    if (alg == typeof(ECDsa))
                    {
                        var ecdPk = cert.GetECDsaPrivateKey();
                        if (ecdPk is ECDsaCng ecdCng)
                        {
                            return ecdCng.Key.UniqueName;
                        }
                    }
                    
                    // INFO: DSA not currently supported
                    // if (alg == typeof(DSA))
                    // {
                    //     var ecdPk = cert.GetDSAPrivateKey();
                    //     if (ecdPk is ECDsaCng ecdCng)
                    //     {
                    //         return ecdCng.Key.UniqueName;
                    //     }
                    // }
                }
                catch (Exception e)
                {
                    var message = $"Private {alg.Name} private key was not successful.";
                    message += $"Details: {e.Message}";
                    
                    // TODO: Write to output from outside of the Cmdlet class
                }
            }
            
            throw new InvalidOperationException($"A private key for the {nameof(X509Certificate)} was not found.");
        }
        
        private static string GetKeyFile(string keyFileName)
        {
            if (string.IsNullOrWhiteSpace(keyFileName)) throw new ArgumentNullException(nameof(keyFileName));

            foreach (var location in KeyContainers)
            {
                var results = Directory.GetFiles(location, $"*{keyFileName}*", SearchOption.AllDirectories);

                if (!results.Any()) continue;

                return results.Single();
            }

            throw new ArgumentException($"Unable to find private key '{keyFileName}'.", nameof(keyFileName));
        }

        private static List<string> GetKeyContainers()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            return new()
            {
                RSA_SCHANNEL_KEYS,
                APPDATA_MS_CRYPTO_KEYS,
                PROGRAMDATA_MS_CRYPTO_KEYS,
                $"{commonAppData}\\Microsoft\\Crypto\\RSA\\MachineKeys",
                $"{appData}\\Microsoft\\Crypto\\RSA",
                $"{winDir}\\ServiceProfiles"
            };
        }

        [DllImport("crypt32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CryptAcquireCertificatePrivateKey(IntPtr pCert, uint dwFlags, IntPtr pvReserved, ref IntPtr phCryptProv, ref int pdwKeySpec, ref bool pfCallerFreeProv);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CryptGetProvParam(IntPtr hCryptProv, CryptGetProvParamType dwParam, IntPtr pvData, ref int pcbData, uint dwFlags);

        [DllImport("advapi32", SetLastError = true)]
        private static extern bool CryptReleaseContext(IntPtr hProv, uint dwFlags);
    }

    internal enum CryptGetProvParamType
    {
        PP_ENUMALGS = 1,
        PP_ENUMCONTAINERS = 2,
        PP_IMPTYPE = 3,
        PP_NAME = 4,
        PP_VERSION = 5,
        PP_CONTAINER = 6,
        PP_CHANGE_PASSWORD = 7,
        PP_KEYSET_SEC_DESCR = 8, // get/set security descriptor of keyset
        PP_CERTCHAIN = 9,        // for retrieving certificates from tokens
        PP_KEY_TYPE_SUBTYPE = 10,
        PP_PROVTYPE = 16,
        PP_KEYSTORAGE = 17,
        PP_APPLI_CERT = 18,
        PP_SYM_KEYSIZE = 19,
        PP_SESSION_KEYSIZE = 20,
        PP_UI_PROMPT = 21,
        PP_ENUMALGS_EX = 22,
        PP_ENUMMANDROOTS = 25,
        PP_ENUMELECTROOTS = 26,
        PP_KEYSET_TYPE = 27,
        PP_ADMIN_PIN = 31,
        PP_KEYEXCHANGE_PIN = 32,
        PP_SIGNATURE_PIN = 33,
        PP_SIG_KEYSIZE_INC = 34,
        PP_KEYX_KEYSIZE_INC = 35,
        PP_UNIQUE_CONTAINER = 36,
        PP_SGC_INFO = 37,
        PP_USE_HARDWARE_RNG = 38,
        PP_KEYSPEC = 39,
        PP_ENUMEX_SIGNING_PROT = 40,
        PP_CRYPT_COUNT_KEY_USE = 41
    }
}
