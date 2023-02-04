using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using PSCerts.Util;

namespace PSCerts.Tests
{
    public class CertPrivateKeyTests : PSCertsTestBase
    {
        [Test]
        public void Cert_With_PrivateKey_Not_Null()
        {
            Assert.That(Certs, Is.Not.Empty);

            var certsWithPk = Certs.Where(c => c.HasPrivateKey).ToList();

            Assert.That(certsWithPk, Is.Not.Empty);

            foreach (var cert in certsWithPk)
            {
                PrivateKeyHelper.TryGetPrivateKey(cert, out var pk);

                Console.WriteLine($"{pk}");
            }

            Assert.Pass();
        }

        [Test]
        public void Cert_With_PrivateKey_Exists()
        {
            Assert.That(Certs, Is.Not.Empty);

            var certsWithPk = Certs.Where(c => c.HasPrivateKey);

            Assert.That(certsWithPk, Is.Not.Empty);

            Assert.Pass();
        }

    }
}
