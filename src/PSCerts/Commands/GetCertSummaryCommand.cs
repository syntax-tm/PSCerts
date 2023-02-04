using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using PSCerts.Util;

namespace PSCerts.Commands
{
    [Cmdlet(VerbsCommon.Get, "CertSummary")]
    [OutputType(typeof(List<CertSummary>))]
    public class GetCertSummaryCommand : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public StoreLocation? StoreLocation { get; set; }
        
        protected override void ProcessRecord()
        {
            try
            {
                var certs = new List<CertSummary>();

                var locations = StoreLocation.HasValue
                    ? new List<StoreLocation> { StoreLocation.Value }
                    : Enum.GetValues(typeof(StoreLocation)).Cast<StoreLocation>();

                foreach (var location in locations)
                foreach (var storeName in Enum.GetValues(typeof(StoreName)).Cast<StoreName>())
                {
                    var store = new X509Store(storeName, location);

                    try
                    {
                        store.Open(OpenFlags.ReadOnly);

                        foreach (var cert in store.Certificates)
                        {
                            var summary = new CertSummary(store, cert);

                            if (PrivateKeyHelper.TryGetPrivateKey(cert, out var pkFilePath))
                            {
                                var pkFi = new FileInfo(pkFilePath);
                                var acl = pkFi.GetAccessControl(AccessControlSections.All);
                                var rules = acl.GetAccessRules(true, true, typeof(NTAccount));

                                summary.PrivateKey = new (pkFilePath);
                                summary.Permissions = rules.AsList<AuthorizationRule>();
                            }

                            certs.Add(summary);
                        }
                    }
                    finally
                    {
                        store.Close();
                    }
                }

                WriteObject(certs, false);
            }
            catch (Exception e)
            {
                var error = ErrorHelper.CreateError(e);
                ThrowTerminatingError(error);
            }
        }
    }
}
