using System.Management.Automation;

namespace PSCerts.Tests
{
    public class CertSummaryTests : PSCertsTestBase
    {
        [Test]
        public void Get_CertSummary_Shows_Private_Keys()
        {
            var cmd = new GetCertSummaryCommand();
            var ps = PowerShell.Create();

            var result = ps.AddCommand("Get-CertSummary").Invoke();

            Assert.That(result, Is.Not.Empty);

            Console.WriteLine(result.ToString());

            Assert.Pass();
        }
    }
}
