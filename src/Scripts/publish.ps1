Set-Location $PSScriptRoot

. .\shared\all.ps1

.\build.ps1

Write-Host ""

if ($global:BuildStatusCode -ne 0) { return }

$manifestPath = Join-Path $global:PUBLISH_PATH $global:MANIFEST_FILE_NAME

$manifestUpdated = Update-Manifest -Path $manifestPath
if (!$manifestUpdated) {
    return
}

Push-Location $global:PUBLISH_PATH

$apiKey = [System.Environment]::GetEnvironmentVariable('PSGALLERY_NUGET_API_KEY', 'User')

# publish to PSGallery
Publish-Module -Name ".\$global:MODULE_FILE_NAME" -NuGetApiKey $apiKey

Pop-Location
