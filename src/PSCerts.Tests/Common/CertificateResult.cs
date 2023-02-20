using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PSCerts.Tests;

public class CertificateResult
{
    public CertificateRequest Request { get; set; }
    public X509Certificate2 Certificate { get; set; }
    public RSA PrivateKey { get; set; }
}
