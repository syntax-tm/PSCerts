using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using PSCerts.Util;

namespace PSCerts.Tests;

public static class CertificateFactory
{
    internal const string CA_FORMAT = @"PSCerts Test Authority - {0}";
    internal const int CA_RSA_LENGTH = 4096;
    internal const int RSA_LENGTH = 2048;

    internal static StoreLocation CurrentLocation
    {
        get
        {
            var location = IdentityHelper.IsAdministrator
                ? StoreLocation.LocalMachine
                : StoreLocation.CurrentUser;

            return location;
        }
    }
    internal static string CA => string.Format(CA_FORMAT, CurrentLocation);

    public static CertificateResult GetRootCertificate()
    {
        var location = IdentityHelper.IsAdministrator
            ? StoreLocation.LocalMachine
            : StoreLocation.CurrentUser;

        using var store = new X509Store(StoreName.Root, location);
        store.Open(OpenFlags.MaxAllowed);

        var matches = store.Certificates.Find(X509FindType.FindBySubjectName, $"CN={CA}", false);

        if (matches.Count == 0)
        {
            var generated = GenerateRootCert();
            var ca = generated.Certificate;

            store.Add(ca);

            return generated;
        }

        var existing = matches[0];
        var rsa = existing.GetRSAPrivateKey();

        var result = new CertificateResult
        {
            Certificate = existing,
            PrivateKey = rsa
        };
        
        return result;
    }

    public static CertificateResult GenerateSelfSignedCertificate(string subjectName)
    {
        using var rsa = RSA.Create(RSA_LENGTH);
    
        var parent = GetRootCertificate();
        var parentCert = parent.Certificate;
        
        var req = new CertificateRequest($"CN={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation, false));
        req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.8") }, true));
        req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

        using var cert = req.Create(parentCert, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(90), new byte[] { 1, 2, 3, 4 });

        using var store = new X509Store(StoreName.My, CurrentLocation);
        store.Open(OpenFlags.MaxAllowed);

        store.Add(cert);

        var result = new CertificateResult
        {
            Request = req,
            Certificate = cert.CopyWithPrivateKey(rsa),
            PrivateKey = rsa
        };

        return result;
    }

    private static CertificateResult GenerateRootCert()
    {
        using var rsa = RSA.Create(CA_RSA_LENGTH);
    
        var req = new CertificateRequest($"CN={CA}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        req.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));
        
        using var parentCert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-45), DateTimeOffset.UtcNow.AddDays(365));
        
        var result = new CertificateResult
        {
            Request = req,
            Certificate = parentCert.CopyWithPrivateKey(rsa),
            PrivateKey = rsa
        };

        return result;
    }
}
