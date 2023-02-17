
$module = 'PSCerts'
$slnRoot = Split-Path $PSScriptRoot
$publishPath = Join-Path $slnRoot "publish"

$moduleFileName = "$module.psm1"
$manifestFileName = "$module.psd1"
$manifestPath = Join-Path $publishPath $manifestFileName

$ErrorActionPreference = 'Stop'

Set-Location $PSScriptRoot

. .\shared\utils.ps1

$manifestUpdated = Update-Manifest -Path $manifestPath
if (!$manifestUpdated) {
    return
}

Push-Location $publishPath

# publish to PSGallery
Publish-Module -Name ".\$moduleFileName" -NuGetApiKey $env:PSGALLERY_NUGET_API_KEY

Pop-Location
