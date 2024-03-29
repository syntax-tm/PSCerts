﻿using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Web.Administration;
using PSCerts.Util;

namespace PSCerts.Commands
{
    [Cmdlet(VerbsCommon.Add, "SiteBinding", DefaultParameterSetName = CERT_PARAM_SET)]
    [OutputType(typeof(CertBinding))]
    public class AddSiteBindingCommand : CmdletBase
    {
        private const string CERT_PARAM_SET = nameof(CERT_PARAM_SET);
        private const string THUMBPRINT_PARAM_SET = nameof(THUMBPRINT_PARAM_SET);
        private const string FROM_FILE_SET = nameof(FROM_FILE_SET);
        private const string FROM_FILE_SECURE_SET = nameof(FROM_FILE_SECURE_SET);

        private const string HTTPS_PROTOCOL = @"https";
        private const string DEFAULT_BINDING_INFO = @"*:443:";
        private const string DEFAULT_SITE_NAME = @"Default Web Site";
        private const StoreLocation DEFAULT_STORE_LOCATION = StoreLocation.LocalMachine;
        private const StoreName DEFAULT_STORE_NAME= StoreName.My;
        private const X509KeyStorageFlags DEFAULT_STORAGE_FLAGS = X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable;
        
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = CERT_PARAM_SET)]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = THUMBPRINT_PARAM_SET)]
        [ValidateNotNullOrEmpty]
        public string Thumbprint { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = FROM_FILE_SECURE_SET)]
        [Alias("File", "Path")]
        [ValidateNotNullOrEmpty]
        public string FilePath { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = FROM_FILE_SET)]
        [ValidateNotNullOrEmpty]
        public string Password { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = FROM_FILE_SECURE_SET)]
        [ValidateNotNull]
        public SecureString SecurePassword { get; set; }

        [Parameter(Position = 1, ParameterSetName = CERT_PARAM_SET)]
        [Parameter(Position = 1, ParameterSetName = THUMBPRINT_PARAM_SET)]
        [Parameter(Position = 2, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Position = 2, ParameterSetName = FROM_FILE_SECURE_SET)]
        [Alias("Name")]
        [ValidateNotNullOrEmpty]
        public string Site { get; set; } = DEFAULT_SITE_NAME;

        [Parameter(Position = 2, ParameterSetName = CERT_PARAM_SET)]
        [Parameter(Position = 2, ParameterSetName = THUMBPRINT_PARAM_SET)]
        [Parameter(Position = 3, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Position = 3, ParameterSetName = FROM_FILE_SECURE_SET)]
        [Alias("Binding", "Info")]
        [ValidateNotNullOrEmpty]
        public string BindingInformation { get; set; } = DEFAULT_BINDING_INFO;

        [Parameter(Position = 3, ParameterSetName = CERT_PARAM_SET)]
        [Parameter(Position = 3, ParameterSetName = THUMBPRINT_PARAM_SET)]
        [Parameter(Position = 4, ParameterSetName = FROM_FILE_SET)]
        [Parameter(Position = 4, ParameterSetName = FROM_FILE_SECURE_SET)]
        public SslFlags SslFlags { get; set; } = SslFlags.None;

        protected override void ProcessRecord()
        {
            X509Store store = null;

            try
            {
                X509Certificate2 cert = null;

                if (Certificate is not null)
                {
                    cert = Certificate;
                }
                else if (!string.IsNullOrWhiteSpace(Thumbprint))
                {
                    cert = CertHelper.FindCertificate(Thumbprint);
                }
                else if (!string.IsNullOrWhiteSpace(FilePath))
                {
                    if (!File.Exists(FilePath)) throw new FileNotFoundException($"File '{FilePath}' does not exist.", FilePath);
                    if (!string.IsNullOrWhiteSpace(Password))
                    {
                        cert = new (FilePath, Password, DEFAULT_STORAGE_FLAGS);
                    }
                    else if (SecurePassword is not null)
                    {
                        cert = new (FilePath, SecurePassword, DEFAULT_STORAGE_FLAGS);
                    }
                }

                if (cert == null)
                {
                    throw new ArgumentException("Unknown parameter set.");
                }

                var thumbprint = cert.Thumbprint ?? throw new InvalidOperationException($"{nameof(X509Certificate2)} {nameof(X509Certificate2.Thumbprint)} cannot be null.");

                store = new (DEFAULT_STORE_NAME, DEFAULT_STORE_LOCATION);
                store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);

                store.Add(cert);

                store.Close();

                var pk = PrivateKeyHelper.GetPrivateKey(cert);
                var mgr = new ServerManager();
                var site = mgr.Sites.Single(s => s.Name.EqualsIgnoreCase(Site));
                
                var appPoolName = site.Applications.FirstOrDefault()?.ApplicationPoolName;
                var appPool = mgr.ApplicationPools.SingleOrDefault(p => p.Name.EqualsIgnoreCase(appPoolName));

                if (appPool == null) throw new InvalidOperationException($"An {nameof(ApplicationPool)} with the name {appPoolName} was not found.");

                WriteVerbose($"IIS application pool {appPool.Name} found.");

                var appPoolIdentity = IISHelper.GetApplicationPoolIdentity(appPool);
                var acl = FileSystemHelper.AddAccessControl(pk, appPoolIdentity);

                WriteVerbose($"Added {FileSystemRights.Read} permissions for {appPoolIdentity} on {pk}.");

                // if a binding already exists, we'll update that, otherwise create a new one
                var existingBinding = site.Bindings.Where(b => b.BindingInformation == BindingInformation);
                var binding = existingBinding.FirstOrDefault() ?? site.Bindings.Add(BindingInformation, HTTPS_PROTOCOL);
                binding.CertificateHash = cert.GetCertHash();
                binding.SslFlags = SslFlags;
                binding.CertificateStoreName = store.Name;
                
                mgr.CommitChanges();

                var summary = new CertBinding
                {
                    Site = site,
                    AppPool = appPool,
                    AppPoolIdentity = appPoolIdentity,
                    Certificate = cert,
                    Location = DEFAULT_STORE_LOCATION,
                    StoreName = DEFAULT_STORE_NAME,
                    PrivateKeySecurity = acl,
                    Binding = binding
                };

                WriteObject(summary);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingException(e);
            }
            finally
            {
                store?.Close();
            }
        }
    }
}
