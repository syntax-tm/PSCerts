using PSCerts.Summary;

namespace PSCerts.Tests
{
    internal class GetCertSummaryTests : CmdletBase
    {
        [Test]
        public void Get_CertSummary_ShouldReturnMultipleItems()
        {
            var cmdlet = new GetCertSummaryCommand();

            var results = cmdlet.GetResults<CertSummaryItem>();
            
            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Not.Empty);
            
            Console.WriteLine($"{nameof(GetCertSummaryCommand)} results:");

            foreach (var item in results)
            {
                Console.WriteLine(item.ToString());
            }

            Assert.Pass();
        }

        [Test]
        [TestCase(@"10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae")]
        public void Get_CertSummary_IncludesTestingCert(string thumbprint)
        {
            var cmdlet = new GetCertSummaryCommand();
            
            var results = cmdlet.GetResults<CertSummaryItem>();

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Not.Empty);

            var exists = results.FirstOrDefault(r => r.Thumbprint.EqualsIgnoreCase(thumbprint));

            Assert.That(exists, Is.Not.Null);

            Console.WriteLine($"{nameof(GetCertSummaryCommand)} contains {thumbprint}.");

            Assert.Pass();
        }
    }
}
