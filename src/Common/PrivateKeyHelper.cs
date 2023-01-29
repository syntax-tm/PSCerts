using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace PSCerts
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
                var privateKeyFile = GetKeyFileName(cert);
                return GetKeyFile(privateKeyFile);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred attempting to find the private key for '{key}'. {e.Message}", e);
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
                throw new Exception($"An error occurred attempting to find the private key. {e.Message}", e);
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

        private static string GetKeyFileName(X509Certificate2 cert)
        {
            var hProvider = IntPtr.Zero; // CSP handle
            var freeProvider  = false;   // Do we need to free the CSP ?
            uint acquireFlags  = 0;
            var _keyNumber = 0;
            string keyFileName = null;

            // determine whether there is private key information available for this certificate in the key store
            if (CryptAcquireCertificatePrivateKey(cert.Handle, acquireFlags, IntPtr.Zero, ref hProvider,
                                                  ref _keyNumber, ref freeProvider))
            {
                var pBytes  = IntPtr.Zero; // native Memory for the CRYPT_KEY_PROV_INFO structure
                var cbBytes = 0;           // native Memory size

                try
                {
                    if (CryptGetProvParam(hProvider, CryptGetProvParamType.PP_UNIQUE_CONTAINER, IntPtr.Zero, ref cbBytes, 0))
                    {
                        pBytes = Marshal.AllocHGlobal(cbBytes);

                        if (CryptGetProvParam(hProvider, CryptGetProvParamType.PP_UNIQUE_CONTAINER, pBytes, ref cbBytes, 0))
                        {
                            var keyFileBytes = new byte[cbBytes];

                            Marshal.Copy(pBytes,keyFileBytes,0,cbBytes);

                            // copy everything except tailing null byte
                            keyFileName = System.Text.Encoding.ASCII.GetString(keyFileBytes, 0, keyFileBytes.Length - 1);
                        }
                    }
                }
                finally
                {
                    if (freeProvider)
                        CryptReleaseContext(hProvider, 0);

                    // free native memory
                    if (pBytes != IntPtr.Zero)
                        Marshal.FreeHGlobal(pBytes);
                }
            }

            if (keyFileName == null)
                throw new InvalidOperationException("Unable to obtain private key file name");

            return keyFileName;
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

            return new ()
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