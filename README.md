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

#### Add-CertPermissions

**Usage:**

```powershell
Add-CertPermissions [-Certificate] <X509Certificate2> [-Identity] <string> [-FileSystemRights] <FileSystemRights> [-AccessType] <AccessControlType>
Add-CertPermissions [-Certificate] <X509Certificate2> [-Identity] <string> [-FileSystemRights] <FileSystemRights> [-Deny]
Add-CertPermissions [-Certificate] <X509Certificate2> [-FileSystemAccessRule] <FileSystemAccessRule>
```

**Examples:**

```powershell
$cert = Get-Item Cert:\LocalMachine\My\10df834fc47ddfc4d069d2e4fe79e4bf1d6d4dae
Add-CertPermissions -Certificate $cert -Identity "Network Service" -FileSystemRights FullControl -AccessType Allow
```

**Returns:** [FileSecurity](https://learn.microsoft.com/en-us/dotnet/api/system.security.accesscontrol.filesecurity?view=net-7.0)

#### Get-CertPermissions

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

#### Get-CertPrivateKey

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

#### Get-CertSummary

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

**Returns:** [List\<CertSummaryItem>](./src/PSCerts/Models/Summary/CertSummaryItem.cs)

## Upcoming Features

### Not Documented

#### Add-SiteBinding

### In-Progress

#### Import-Certs

- **certfile (Required):** The full path to the certificate file.
- **stores (Required):** The stores the certificate will be imported to.
- **permissions (Required)**: The stores the certificate will be imported to.
- **password:** The password section can load the password directly from the config, an environment variable, or a file.

<table>
  <tr>
    <th>Type</th>
    <th>Description</th>
    <th>Example</th>
  </tr>
  <tr>
    <td align="center"><code>text</code></td>
    <td>
      The <strong>value</strong> is the password. <i>(Not recommended)</i>
    </td>
    <td>
      <pre>{
    "type": "text",
    "value": "abc123"
}</pre>
    </td>
  </tr>
  <tr>
    <td align="center"><code>file</code></td>
    <td>
      The <strong>value</strong> property is the name of a file containing the password.
    </td>
    <td>
      <pre>{
    "type": "file",
    "value": "C:\\secrets\\TestCert.pfx.pwd"
}</pre>
    </td>
  </tr>
  <tr>
    <td align="center"><code>env</code></td>
    <td>
      The <strong>value</strong> property is the name of the environment variable containing the password.</i>
    </td>
    <td>
      <pre>{
    "type": "text",
    "value": "SECRET_CERT_PW"
}</pre>
    </td>
  </tr>
</table>

#### Sample Config

```json
{
  "cert": "C:\\secrets\\TestCert.pfx",
  "password": {
    "type": "file",
    "filePath": "C:\\secrets\\TestCert.pfx.pwd"
  },
  "stores": [
    {
      "location": "LocalMachine",
      "store": "My"
    },
    {
      "location": "CurrentUser",
      "store": "Root"
    }
  ],
  "permissions": [
    {
      "identity": "NETWORK SERVICE",
      "rights": "FullControl",
      "access": "Allow"
    }
  ],
  "exportable": true
}
```