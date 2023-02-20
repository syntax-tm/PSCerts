using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PSCerts.Summary;

namespace PSCerts.Tests
{
    internal class SetCertFriendlyNameTests : PSCertsTestBase
    {
        [Test]
        [TestCase(@"10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae", "NEW_FRIENDLY_NAME")]
        public void Set_CertFriendlyName_UpdatesFriendlyName(string thumbprint, string friendlyName)
        {
            var cmdlet = new SetCertFriendlyNameCommand
            {
                Thumbprint = thumbprint,
                FriendlyName = friendlyName
            };
            
            var result = cmdlet.Invoke<X509Certificate2>().ToList();
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result[0].FriendlyName, Is.Not.Null);
            Assert.That(result[0].FriendlyName, Is.Not.Empty);
            Assert.That(result[0].FriendlyName, Is.EqualTo(friendlyName));

            Assert.Pass();
        }
        
        [Test]
        [TestCase(@"10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae", "")]
        public void Set_CertFriendlyName_EmptyStringShouldRemoveFriendlyName(string thumbprint, string friendlyName)
        {
            var cmdlet = new SetCertFriendlyNameCommand
            {
                Thumbprint = thumbprint,
                FriendlyName = friendlyName
            };
            
            var result = cmdlet.Invoke<X509Certificate2>()?.FirstOrDefault();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.FriendlyName, Is.Not.Null);
            Assert.That(result.FriendlyName, Is.EqualTo(friendlyName));

            Assert.Pass();
        }
    }
}
