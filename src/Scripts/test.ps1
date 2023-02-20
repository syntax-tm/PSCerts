using namespace System.IO

$ErrorActionPreference = 'Stop'

Set-Location $PSScriptRoot

. .\shared\all.ps1

.\build.ps1

Write-Host ""

if ($global:BuildStatusCode -ne 0) { return }

# copy everything from the publish directory to the testing directory
# that way we're still able to build
Remove-Item $global:TEST_PATH -Recurse -Force -ErrorAction Ignore
Copy-Item $global:PUBLISH_PATH -Destination $global:TEST_PATH -Force -Recurse -Container:$false

# uninstall current module (if exists)
Uninstall-Module $global:MODULE_NAME -AllVersions -Force -ErrorAction SilentlyContinue | Out-Null

# remove current module (if exists)
Get-Module $global:MODULE_NAME -ListAvailable | Remove-Item -Force | Out-Null

$manifestUpdated = Update-Manifest -Path $global:TEST_MANIFEST_PATH -Testing
if (!$manifestUpdated)
{
    return
}

# import the module from the testing directory
Import-Module $global:TEST_MANIFEST_PATH -Verbose

# this will import the PFX certificates configured in data.ps1
Import-TestCerts

$testCertThumbprint = '10DF834FC47DDFC4D069D2E4FE79E4BF1D6D4DAE'
$testCert = Get-Item "Cert:\LocalMachine\My\$testCertThumbprint"

function Write-TestHeader
{
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$ID,
        [Parameter(Mandatory = $true, Position = 1)]
        [string]$CommandName,
        [Parameter(Position = 2)]
        [string]$Type = 'TEST',
        [Parameter(Position = 3)]
        [string]$Description
    )

    $header = "$b$black[$cyanb$Type$black | $cyanb$ID$black]$r $fmt_acc$b$CommandName$r"
    $len = $Type.Length + $CommandName.Length + $ID.Length + 6

    Write-Host ""
    Write-Host $header
    Write-Host ([string]::Empty.PadRight($len, '-'))
    Write-Host ""
}

# | TEST | 0.0.1 | Get-CertPermissions
# ------------------------------------

Write-TestHeader '0.0.1' 'Get-CertPermissions'

Get-CertPermissions -Thumbprint $testCertThumbprint | Out-Host

# | TEST | 0.0.2 | Add-CertPermissions
# ------------------------------------

Write-TestHeader '0.0.2' 'Add-CertPermissions'

Add-CertPermissions -Thumbprint $testCertThumbprint -Identity 'Guest' -FileSystemRights Read -AccessType Deny
Add-CertPermissions -Thumbprint $testCertThumbprint -Identity 'DefaultAccount' -FileSystemRights Write -AccessType Deny
Add-CertPermissions -Thumbprint $testCertThumbprint -Identity 'DefaultAccount' -FileSystemRights Read -AccessType Allow
Add-CertPermissions -Thumbprint $testCertThumbprint -Identity 'LOCAL SERVICE' -FileSystemRights Read -AccessType Allow
Add-CertPermissions -Thumbprint $testCertThumbprint -Identity 'NETWORK SERVICE' -FileSystemRights FullControl -AccessType Allow

Write-Host "OK" -ForegroundColor Green

# | TEST | 0.0.3 | Get-CertPermissions
# ------------------------------------

Write-TestHeader '0.0.3' 'Get-CertPermissions'

Get-CertPermissions -Thumbprint $testCertThumbprint | Out-Host

# | TEST | 0.1.1 | Get-CertPrivateKey
# -----------------------------------

Write-TestHeader '0.1.1' 'Get-CertPrivateKey'

Get-CertPrivateKey -Thumbprint $testCertThumbprint | Format-List -Property Directory, Name | Out-Host

# | TEST | 0.2.1 | Get-CertSummary
# -----------------------------------

Write-TestHeader '0.2.1' 'Get-CertSummary'

Get-CertSummary | Where-Object HasPrivateKey -eq $true | Out-Host

# | TEST | 1.0.1 | Get-CertSummary
# -----------------------------------

Write-TestHeader '1.0.1' '[CertAccessRule]'

Get-CertSummary | Where-Object Thumbprint -eq $testCertThumbprint | Select-Object -ExpandProperty Permissions -Unique | Format-Table | Out-Host
