using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using PSCerts.Util;

namespace PSCerts.Tests
{
    public abstract class PSCertsTestBase
    {
        protected const string CERT_FILE_NAME = @"PSCerts_01";
        protected const string CERT_REL_PATH = @$"Resources\Certs\{CERT_FILE_NAME}.pfx";
        protected const string THUMBPRINT = @"10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae";
        
        protected static string BasePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        protected static string CertPath => Path.Combine(BasePath, CERT_REL_PATH);
        
        protected readonly List<X509Certificate2> Certs = new ();

        [SetUp]
        public void Setup()
        {
            VerifyAccess();

            ImportTestCert();
            
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            var matches = store.Certificates.Find(X509FindType.FindByThumbprint, THUMBPRINT, false);
            Certs.AddRange(matches.AsList<X509Certificate2>());
        }

        [TearDown]
        public void TearDown()
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            
            var matches = store.Certificates.Find(X509FindType.FindByThumbprint, THUMBPRINT, false);

            Console.WriteLine($"Removing {matches.Count} certs from {store.Name}...");

            store.Certificates.RemoveRange(matches);
            
            store.Close();
        }

        private static void VerifyAccess()
        {
            Assert.That(IdentityHelper.IsAdministrator, $"{Assembly.GetExecutingAssembly().GetName().Name} is not running as administrator.");
        }

        private static void ImportTestCert()
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);

            const X509KeyStorageFlags flags = X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable;

            var cert = new X509Certificate2(CertPath, CERT_FILE_NAME, flags);
            
            Console.WriteLine(THUMBPRINT);
            
            Console.WriteLine($"Importing certificate with thumbprint '{THUMBPRINT}' into {store.Name}...");

            store.Add(cert);

            store.Close();
        }
    }
}
