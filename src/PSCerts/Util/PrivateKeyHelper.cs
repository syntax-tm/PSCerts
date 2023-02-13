using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
                typeof(ECDsa)
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
                catch
                {
                    // TODO: Write to output from outside of the Cmdlet class

                    // var message = $"Retrieving certificate '{cert.Thumbprint}' private key as {alg.Name} failed. ";
                    // message += $"Details: {e.Message}";
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
            // TODO: Validate this list is all inclusive when running as user and admin
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            var containers = new List<string>
            {
                $@"{appData}\Microsoft\Crypto\RSA",
                $@"{commonAppData}\Microsoft\Crypto\RSA\MachineKeys"
            };

            if (IdentityHelper.IsAdministrator)
            {
                containers.Add(RSA_SCHANNEL_KEYS);
                containers.Add(APPDATA_MS_CRYPTO_KEYS);
                containers.Add(PROGRAMDATA_MS_CRYPTO_KEYS);
                containers.Add($@"{winDir}\ServiceProfiles");
            }

            return containers;
        }
    }
}
