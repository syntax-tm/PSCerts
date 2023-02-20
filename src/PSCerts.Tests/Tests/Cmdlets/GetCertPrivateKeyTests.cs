namespace PSCerts.Tests
{
    internal class GetCertPrivateKeyTests : PSCertsTestBase
    {
        [Test]
        public void Get_CertPrivateKey_NullCertificateThrowsException()
        {
            var cmdlet = new GetCertPrivateKeyCommand
            {
                Certificate = null
            };
            
            var result = cmdlet.GetErrors();
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);

            Assert.Pass();
        }
        
        [Test]
        public void Get_CertPrivateKey_NullThumbprintThrowsException()
        {
            var cmdlet = new GetCertPrivateKeyCommand
            {
                Thumbprint = null
            };
            
            var result = cmdlet.GetErrors();
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);

            Assert.Pass();
        }

        [Test]
        public void Get_CertPrivateKey_ByCertificateIsSuccessful()
        {
            var cmdlet = new GetCertPrivateKeyCommand
            {
                Certificate = Certs[0]
            };
            
            var result = cmdlet.GetResult<FileInfo>();
            
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.HasOutput, Is.True);
                Assert.That(result.Output, Has.Count.EqualTo(1));
            });

            Assert.Pass();
        }

        [Test]
        [TestCase(@"10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae")]
        public void Get_CertPrivateKey_ByThumbprintIsSuccessful(string thumbprint)
        {
            var cmdlet = new GetCertPrivateKeyCommand
            {
                Thumbprint = thumbprint
            };
            
            var result = cmdlet.GetResult<FileInfo>();
            
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.HasOutput, Is.True);
                Assert.That(result.Output, Has.Count.EqualTo(1));
            });

            Assert.Pass();
        }
    }
}
