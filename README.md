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

## Build

The `build.ps1` script will build and publish both the CLR (`net472`) and Core CLR (`netstandard2.0`) frameworks.

```powershell
.\src\scripts\build.ps1
```

Once that is done, the module and all required assemblies, type data, manifest, etc will be in the `src\publish` directory. If you are wanting to import the module you can use this directory but it's recommended to use the [Test](#test) script.

## Test

Because **PSCerts** is a binary module, importing the assembly from the build or publish directory will keep you from being able to buiild and/or deploy. Simply removing the module from the session with `Remove-Module` is **not** enough to remove the actual assembly reference. To get around this, `test.ps1` will run `build.ps1` and copy everything to `src\test`. You can load the assembly from the `test` path and still be able run build and publish.

If you are developing in VSCode, which is recommnded, you can configure the PowerShell add-on to create a temporary console for each debugging session. This prevents locking the binary and the script will automatically re-import the module with each session.

```json
"powershell.debugging.createTemporaryIntegratedConsole": true
```

## Commands

### Add-CertPermissions

Adds a [FileSystemAccessRule](https://learn.microsoft.com/en-us/dotnet/api/system.security.accesscontrol.filesystemaccessrule) to the access control and audit security for a certificate's private key file.

**Usage:**

```powershell
Add-CertPermissions [-Certificate] <X509Certificate2> [-Identity] <string> [-FileSystemRights] <FileSystemRights> [-AccessType] <AccessControlType>
Add-CertPermissions [-Certificate] <X509Certificate2> [-Identity] <string> [-FileSystemRights] <FileSystemRights> [-Deny]
Add-CertPermissions [-Certificate] <X509Certificate2> [-Rule] <FileSystemAccessRule>
```

**Examples:**

```powershell
$cert = Get-Item Cert:\LocalMachine\My\10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae
Add-CertPermissions -Certificate $cert -Identity "Network Service" -FileSystemRights FullControl -AccessType Allow
```

**Returns:** [FileSecurity](https://learn.microsoft.com/en-us/dotnet/api/system.security.accesscontrol.filesecurity?view=net-7.0)

---

### Get-CertPermissions

Returns the access control and audit security for a certificate's private key.

```powershell
Get-CertPermissions [-Certificate] <X509Certificate2>
```

**Examples:**

```powershell
$cert = Get-Item Cert:\LocalMachine\My\10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae
Get-CertPermissions -Certificate $cert
```

**Returns:** [List\<CertAccessRule>](./src/PSCerts/Models/CertAccessRule.cs)

---

### Get-CertPrivateKey

Determines the name and location of the certificate's private key.

**Usage:**

```powershell
Get-CertPrivateKey [-Certificate] <X509Certificate2>
Get-CertPrivateKey [-StoreLocation] <StoreLocation> [-StoreName] <StoreName> [-Key] <string> [-FindType] <X509FindType>
```

**Examples:**

```powershell
$cert = Get-Item Cert:\LocalMachine\My\10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae
Get-CertPrivateKey -Certificate $cert
```

**Returns:** [FileInfo](https://learn.microsoft.com/en-us/dotnet/api/system.io.fileinfo?view=net-7.0)

### Get-CertSummary

Returns information about installed certificates including the `Subject`, `Thumbprint`, `StoreLocation`, `Store`, private key name and path, permissions, and more. The return value can be used for further processing or displayed in the console.

**Usage:**

```powershell
Get-CertSummary [[-Location] <StoreLocation>] [-HasPrivateKey]
Get-CertSummary [[-Location] <StoreLocation>] [[-Stores] <StoreName[]>] [-HasPrivateKey]
Get-CertSummary [[-Location] <StoreLocation>] [-Detailed] [-HasPrivateKey]
```

**Examples:**

```powershell
Get-CertSummary
Get-CertSummary -Location LocalMachine
Get-CertSummary -Detailed
```

**Returns:** [List\<CertSummaryItem>](/src/PSCerts/Models/Summary/CertSummaryItem.cs)

---

## In-Progress

<details>
  <summary><b>Add-SiteBinding</b></summary>

Adds a new or updates an existing IIS site's binding.

TODO: Consider renaming the noun and using the `Set` verb instead of `Add`.

</details>

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

## TODO

- [x] Finish `Add-SiteBinding`
- [x] Finish `Import-Certs`
- [ ] Add documentation for `Add-SiteBinding`
- [ ] Finish documentation for `Import-Certs`
- [-] Add Cmdlet help information
- [ ] Add unit tests
- [ ] Add support for .NET 4.6.1 (or older)
- [ ] Add version history, release notes, etc. to the module manifest
- [ ] Move non-Cmdlet code to a separate project
- [ ] Create NuGet package for the core functionality
- [ ] Come up with better names for the model classes (and others)
- [ ] Move [Version History](#version-history) to its own file
- [ ] Create documentation (wiki)

## Version History

- **0.0.10**
  - Added `Import-Certs`
  - Private key search locations are now dependent upon the user's access (admin vs. user)
  - Misc. cleanup

- **0.0.9**
  - Updated `README.md`
  - Fixed ACLs on Windows PowerShell
  - Misc. cleanup
