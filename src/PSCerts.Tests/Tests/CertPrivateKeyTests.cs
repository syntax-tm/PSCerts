using System.Security.Principal;
using Newtonsoft.Json;
using PSCerts.Util;

namespace PSCerts.Tests
{
    public class CertPrivateKeyTests : PSCertsTestBase
    {
        [Test]
        [TestCase(@"10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae")]
        public void Cert_With_PrivateKey_Not_Null(string thumbprint)
        {
            var cert = Certs.Single(c => c.Thumbprint == thumbprint);

            PrivateKeyHelper.TryGetPrivateKey(cert, out var pk);

            Assert.That(pk, Is.Not.Null);
            Assert.That(pk, Is.Not.Empty);

            Assert.Pass();
        }

        [Test]
        [TestCase(@"10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae")]
        public void Cert_With_PrivateKey_Exists(string thumbprint)
        {
            var cert = Certs.Single(c => c.Thumbprint == thumbprint);
            
            Assert.That(cert.HasPrivateKey);

            PrivateKeyHelper.TryGetPrivateKey(cert, out var pk);

            Console.WriteLine("Private Key:");
            Console.WriteLine($"{pk}");
                
            Assert.That(pk, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(pk, Is.Not.Empty);
                Assert.That(File.Exists(pk));
            });

            Assert.Pass();
        }
        
        [Test]
        [TestCase(@"10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae")]
        public void Cert_With_PrivateKey_Can_Set_Permissions(string thumbprint)
        {
            var cert = Certs.Single(c => c.Thumbprint == thumbprint);
            
            Assert.That(cert.HasPrivateKey);

            PrivateKeyHelper.TryGetPrivateKey(cert, out var pk);
            
            FileSystemHelper.AddAccessControl(pk, @"NETWORK SERVICE");

            var acl = FileSystemHelper.GetAccessControl(pk);

            Assert.NotNull(acl);

            var rules = acl.GetAccessRules(true, true, typeof(SecurityIdentifier));

            Assert.That(rules, Is.Not.Empty);

            Console.WriteLine(JsonConvert.SerializeObject(rules, Formatting.Indented));

            Assert.Pass();
        }

    }
}
