//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Security.Cryptography;
//using System.Security.Cryptography.X509Certificates;

//namespace PSCerts.Util
//{
//    public static class CertificateHelper
//    {

//        private const string OID_SERVER_AUTH = @"1.3.6.1.5.5.7.3.1";
//        private const int KEY_LENGTH = 2048;
//        private const int EXPIRATION_YEARS = 10;
//        private const string LOCALHOST_DNS = @"localhost";
//        private const X509KeyUsageFlags KEY_USAGE = X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature;

//        private static List<string> _certNames = new ()
//        {
//            @"PSCerts_01",
//            @"PSCerts_02",
//            @"PSCerts_03"
//        };

//        //public static List<X509Certificate2> CreateTestCerts()
//        //{
//        //    var certs = new List<X509Certificate2>();
//        //
//        //    foreach (var name in _certNames)
//        //    {
//        //        var cert = CreateSelfSignedCertificate(name);
//        //    }
//        //
//        //    return certs;
//        //}

//        public static X509Certificate2 CreateSelfSignedCertificate(string name, string password = null, int yearsValid = EXPIRATION_YEARS,
//            X509KeyUsageFlags keyUsage = KEY_USAGE)
//        {
//            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
//            if (string.IsNullOrEmpty(password))
//            {
//                password = name;
//            }

//            var distinguishedName = new X500DistinguishedName($"CN={name}");

//            using var rsa = RSA.Create();
//            rsa.KeySize = KEY_LENGTH;
//            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

//            request.CertificateExtensions.Add(new X509KeyUsageExtension(keyUsage, false));
//            request.CertificateExtensions.Add(GetEnhancedKeyUsage());
//            request.CertificateExtensions.Add(GetSAN());

//            var certificate = request.CreateSelfSigned(new (DateTime.UtcNow.AddDays(-1)), new (DateTime.UtcNow.AddYears(yearsValid)));
//            certificate.FriendlyName = name;

//            return new (certificate.Export(X509ContentType.Pfx, password), password, X509KeyStorageFlags.MachineKeySet);
//        }

//        private static X509EnhancedKeyUsageExtension GetEnhancedKeyUsage()
//        {
//            var oid = new OidCollection { new (OID_SERVER_AUTH) };
//            return new (oid, false);
//        }

//        private static X509Extension GetSAN()
//        {
//            var sanBuilder = new SubjectAlternativeNameBuilder();

//            sanBuilder.AddIpAddress(IPAddress.Loopback);
//            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
//            sanBuilder.AddDnsName(LOCALHOST_DNS);
//            sanBuilder.AddDnsName(Environment.MachineName);

//            return sanBuilder.Build();
//        }

//    }
//}
