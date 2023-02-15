using namespace System.IO

$ErrorActionPreference = 'Stop'

Set-Location $PSScriptRoot

$module = 'PSCerts'
$moduleFileName = "$module.psm1"
$slnRoot = Split-Path $PSScriptRoot
$publishPath = Join-Path $slnRoot "publish"
$testPath = Join-Path $slnRoot "test"
$modulePath = Join-Path $testPath $moduleFileName

. .\shared\utils.ps1

.\build.ps1

Write-Host ""

if ($global:BuildStatusCode -ne 0) { return }

# copy everything from the publish directory to the testing directory
# that way we're still able to build
Remove-Item $testPath -Recurse -Force -ErrorAction Ignore
Copy-Item $publishPath -Destination $testPath -Force -Recurse -Container:$false

# import the module from the testing directory
Import-Module $modulePath -Verbose

# this will import the PFX certificates configured in utils.ps1
Import-TestCerts

Write-Host "`n$b$black[$whiteb`TEST$black]$r $fmt_acc$b`Get-CertSummary$r"

# test summary cmdlet + format
$summary = Get-CertSummary
$summary #| Out-Host

Write-Host "`n$b$black[$whiteb`TEST$black]$r $fmt_acc$b`Get-CertPrivateKey$r"

# test get-privatekey  cmdlet
$cert = Get-Item Cert:\LocalMachine\My\10DF834FC47DDFC4D069D2E4FE79E4BF1D6D4DAE # PSCerts_01.pfx
$certPk = Get-CertPrivateKey $cert
$certPk | Select-Object -Property Directory, Name #| Out-Host

Write-Host "`n$b$black[$whiteb`TEST$black]$r $fmt_acc$b`CertAccessRule Format$r"

# test permission format
$perms = $summary | Where-Object { $_.Permissions.Count -gt 0  } | Select-Object -ExpandProperty Permissions -First 1
$perms #| Out-Host

#Remove-TestCerts
