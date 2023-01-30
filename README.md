# PSCerts

A Powershell module for managing certificates.

## Build

To build `PSCerts`, use the `build.ps1` script in the root directory.

```powershell
.\build.ps1
```

## Test

After building the assembly (`PSCerts.dll`), the module can be imported using the module manifest.

```powershell
Import-Module .\bin\Debug\PSCerts.psd1
```

## Commands

### Find-CertPrivateKey

Usage:

```powershell
Find-CertPrivateKey [-Certificate] <X509Certificate2>

Find-CertPrivateKey [-StoreLocation] <StoreLocation> [-StoreName] <StoreName> [-Key] <string> [-FindType] <X509FindType>
```

Returns: [FileInfo](https://learn.microsoft.com/en-us/dotnet/api/system.io.fileinfo?view=net-7.0)

### Add-CertPermissions

```powershell
Add-CertPermissions [-Certificate] <X509Certificate2> [-Identity] <string> [-FileSystemRights] <FileSystemRights> [-AccessType] <AccessControlType>

Add-CertPermissions [-Certificate] <X509Certificate2> [-Identity] <string> [-FileSystemRights] <FileSystemRights> [-Deny]

Add-CertPermissions [-Certificate] <X509Certificate2> [-FileSystemAccessRule] <FileSystemAccessRule>
```

Returns: [FileSecurity](https://learn.microsoft.com/en-us/dotnet/api/system.security.accesscontrol.filesecurity?view=net-7.0)

### Get-CertPermissions

```powershell
Get-CertPermissions [-Certificate] <X509Certificate2> [-Explicit] [-Inherited]
```

Returns: [AuthorizationRuleCollection](https://learn.microsoft.com/en-us/dotnet/api/system.security.accesscontrol.authorizationrulecollection?view=net-7.0)