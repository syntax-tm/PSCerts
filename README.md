<p align="center">
  <img src="images/PSCerts_header_color.png" />
</p>

<hr>

<p align="center">
  <a href="https://www.powershellgallery.com/packages/PSCerts">
    <img src="https://img.shields.io/powershellgallery/p/PSCerts?color=blueviolet&label=PSCerts&logo=powershell&style=for-the-badge"/>
  </a>
  <a href="https://www.powershellgallery.com/packages/PSCerts">
    <img src="https://img.shields.io/powershellgallery/v/PSCerts?color=blue&logo=nuget&style=for-the-badge"/>
  </a>
  <a href="https://www.powershellgallery.com/api/v2/package/PSCerts/0.0.3">
    <img src="https://img.shields.io/powershellgallery/dt/PSCerts?style=for-the-badge&color=blue"/>
  </a>
</p>

A Powershell module for managing certificates.

## Install

```powershell
Install-Module -Name PSCerts
```

## TOC

- [Install](#install)
- [Commands](#commands)
  - [Add-CertPermissions](#add-certpermissions)
  - [Add-SiteBinding](#add-sitebinding)
  - [Get-CertPermissions](#get-certpermissions)
  - [Get-CertPrivateKey](#get-certprivatekey)
  - [Get-CertSummary](#get-certsummary)
  - [Set-CertFriendlyName](#set-certfriendlyname)
- [Building](#building)
- [Testing](#testing)
  - [Unit Tests](#unit-tests)
- [In-Progress](#in-progress)
- [Backlog](#backlog)
- [Reference](#reference)
- [Additional Resources](#additional-resources)

## Commands

### Add-CertPermissions

Adds a [FileSystemAccessRule](https://learn.microsoft.com/en-us/dotnet/api/system.security.accesscontrol.filesystemaccessrule) to a certificate's private key.

**Usage:**

```powershell
Add-CertPermissions [-Certificate] <X509Certificate2> [-Identity] <string> [-FileSystemRights] <FileSystemRights> [[-AccessType] <AccessControlType>]
Add-CertPermissions [-Certificate] <X509Certificate2> [-Rule] <FileSystemAccessRule>
Add-CertPermissions [-Thumbprint] <string> [-Identity] <string> [-FileSystemRights] <FileSystemRights> [[-AccessType] <AccessControlType>]
Add-CertPermissions [-Thumbprint] <string> [-Rule] <FileSystemAccessRule>
```

**Examples:**

```powershell
$cert = Get-Item Cert:\LocalMachine\My\10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae
Add-CertPermissions -Certificate $cert -Identity "Network Service" -FileSystemRights FullControl -AccessType Allow

Add-CertPermissions -Thumbprint "10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae" -Identity "Network Service" -FileSystemRights FullControl -AccessType Allow
```

**Returns:** `None`

---

### Add-SiteBinding

Adds or updates the SSL [Binding](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.administration.binding) of an IIS site.

**Usage:**

```powershell
Add-SiteBinding [-Certificate] <X509Certificate2> [-Site] <string> [[-BindingInformation] <string>] [[-SslFlags] <SslFlags>]
Add-SiteBinding [-Thumbprint] <string> [-Site] <string> [[-BindingInformation] <string>] [[-SslFlags] <SslFlags>]
Add-SiteBinding [-FilePath] <string> [-Password] <string> [-Site] <string> [[-BindingInformation] <string>] [[-SslFlags] <SslFlags>]
Add-SiteBinding [-FilePath] <string> [-SecurePassword] <SecureString> [-Site] <string> [[-BindingInformation] <string>] [[-SslFlags] <SslFlags>]
```

**Examples:**

```powershell
# adds a new SSL binding for the default site
Add-SiteBinding -Thumbprint '10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae' -Site 'Default Web Site'
```

**Returns:** [CertBinding](./src/PSCerts/Models/CertBinding.cs)

---

### Get-CertPermissions

Returns the access control and audit security for a certificate's private key.

```powershell
Get-CertPermissions [-Certificate] <X509Certificate2>
Get-CertPermissions [-Thumbprint] <string>
```

**Examples:**

```powershell
$cert = Get-Item Cert:\LocalMachine\My\10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae
Get-CertPermissions -Certificate $cert

Get-CertPermissions -Thumbprint '10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae'
```

**Returns:** [List\<CertAccessRule>](./src/PSCerts/Models/CertAccessRule.cs)

---

### Get-CertPrivateKey

Determines the name and location of the certificate's private key.

**Usage:**

```powershell
Get-CertPrivateKey [-Certificate] <X509Certificate2>
Get-CertPrivateKey [-Thumbprint] <string>
```

**Examples:**

```powershell
$cert = Get-Item Cert:\LocalMachine\My\10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae
Get-CertPrivateKey -Certificate $cert

Get-CertPrivateKey -Thumbprint '10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae'
```

**Returns:** [FileInfo](https://learn.microsoft.com/en-us/dotnet/api/system.io.fileinfo)

---

### Get-CertSummary

Returns information about the currently installed certificates.

**Usage:**

```powershell
Get-CertSummary [-WithPrivateKey]
```

**Examples:**

```powershell
Get-CertSummary
Get-CertSummary -WithPrivateKey
```

**Returns:** [List\<CertSummaryItem>](/src/PSCerts/Models/Summary/CertSummaryItem.cs)

---

### Set-CertFriendlyName

Updates the [FriendlyName](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2.friendlyname) of an [X509Certificate2](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2).

**Usage:**

```powershell
Set-CertFriendlyName [-Certificate] <X509Certificate2> [-FriendlyName] <string>
Set-CertFriendlyName [-Thumbprint] <string> [-FriendlyName] <string>
```

**Examples:**

```powershell
Set-CertFriendlyName -Thumbprint '10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae' -FriendlyName "My Test Cert"
```

**Returns:** [X509Certificate2](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2)

---

## Building

The `build.ps1` script will build and publish both the CLR (`net462`) and Core CLR (`netstandard2.0`) frameworks.

```powershell
.\src\scripts\build.ps1
```

Once that is done, the module and all required assemblies, type data, manifest, etc will be in the `src\publish` directory. If you are wanting to import the module you can use this directory but it's recommended to use the [Test](#test) script.

## Testing

Because **PSCerts** is a binary module, importing the assembly from the build or publish directory will keep you from being able to buiild and/or deploy. Simply removing the module from the session with `Remove-Module` is **not** enough to remove the actual assembly reference. To get around this, `test.ps1` will run `build.ps1` and copy everything to `src\test`. You can load the assembly from the `test` path and still be able run build and publish.

If you are developing in VSCode, which is recommnded, you can configure the PowerShell add-on to create a temporary console for each debugging session. This prevents locking the binary and the script will automatically re-import the module with each session.

```json
"powershell.debugging.createTemporaryIntegratedConsole": true
```

### Unit Tests

`PSCerts.Tests` is the unit testing project. It's very much a work-in-progress.

---

## In-Progress

<details>
  <summary><b>Import-Certs</b></summary>

**certfile (Required):** The path to a certificate file
**stores (Required):** One or more stores the certificate will be imported to
**permissions:** File permissions for the private key (Optional)
**password:** The password for the certificate.

The `type` indicates how to handle the `value` property (see below).

- Type: `text`
  - The <strong>value</strong> is the password. <i>(Not recommended)</i>
  - [Example](/docs/examples/ImportCerts/basic.json)
- Type: `file`
  - The <strong>value</strong> is the path to a file that contains the password.
  - [Example](/docs/examples/ImportCerts/passwordFromFile.json)
- Type: `env`
  - The <strong>value</strong> is the name of an environment variable containing the password.
  - [Example](/docs/examples/ImportCerts/passwordFromEnv.json)

</details>

## Backlog

- [ ] Finish documentation for `Import-Certs`
- [ ] Add Cmdlet help information
- [ ] Add unit tests
- [ ] Add version history, release notes, etc. to the module manifest
- [ ] Move non-Cmdlet code to a separate project
- [ ] Create NuGet package for the core functionality
- [ ] Come up with better names for the model classes (and others)
- [ ] Create documentation (wiki)

## Reference

- [Version History](/CHANGELOG.txt)

## Additional Resources

- [Key Storage and Retrieval](https://learn.microsoft.com/en-us/windows/win32/seccng/key-storage-and-retrieval)
