using namespace System.IO

$ErrorActionPreference = 'Stop'

Set-Location $PSScriptRoot

$module = 'PSCerts'
$moduleFileName = "$module.psm1"
$slnRoot = Split-Path $PSScriptRoot
#$buildPath = Join-Path $slnRoot "$module\bin\Release\$framework"
$publishPath = Join-Path $slnRoot "publish"

$configPath = Join-Path $PSScriptRoot "config.json"
$config = Get-Content $configPath | ConvertFrom-Json

$testPath = [Path]::GetFullPath($config.test.testPath, $slnRoot)
$modulePath = Join-Path $testPath $moduleFileName

. .\shared\utils.ps1

.\build.ps1 $config.build.configuration

if ($global:BuildStatusCode -ne 0) { return }

# copy everything from the publish directory to the testing directory
# that way we're still able to build
Remove-Item $testPath -Recurse -ErrorAction Ignore
#New-Item -Path $testPath -ItemType Directory | Out-Null

Copy-Item $publishPath -Destination $testPath -Recurse -Container:$false

# switch the configured terminal for testing
#Invoke-Expression -Command "& $($config.test.terminal)"

# import the module from the testing directory
Import-Module $modulePath -Verbose

# this will import the PFX certificates configured in utils.ps1
Import-TestCerts

Write-Host "`n[TEST] Get-CertSummary" -ForegroundColor Magenta

# test summary cmdlet + format
$summary = Get-CertSummary
$summary | Out-Host

Write-Host "`n[TEST] Get-CertPrivateKey" -ForegroundColor Magenta

# test get-privatekey  cmdlet
$cert = Get-Item Cert:\LocalMachine\My\10DF834FC47DDFC4D069D2E4FE79E4BF1D6D4DAE # PSCerts_01.pfx
$certPk = Get-CertPrivateKey $cert
$certPk | Select-Object -Property Directory, Name | Out-Host

Write-Host "[TEST] CertAccessRule Format" -ForegroundColor Magenta

# test permission format
$perms = $summary | Where-Object { $_.Permissions.Count -gt 0  } | Select-Object -ExpandProperty Permissions -First 1
$perms | Out-Host

#Remove-TestCerts
