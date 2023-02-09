
$module = 'PSCerts'
$slnRoot = Split-Path $PSScriptRoot
$publishPath = Join-Path $slnRoot "publish"

$moduleFileName = "$module.psm1"
$modulePath = Join-Path $publishPath $moduleFileName
$manifestFileName = "$module.psd1"
$manifestPath = Join-Path $publishPath $manifestFileName

$versionRegex = "ModuleVersion\s*=\s*'(?<version>[\d|\.]+)'"

$ErrorActionPreference = 'Stop'

Set-Location $PSScriptRoot

. .\shared\utils.ps1

if (-not (Test-Path $manifestPath)) {
    Write-Host "`n$yellow$inv[WARN] $inv_off$r" -NoNewline
    Write-Host "$yellow`The published module file '$cyan$manifestPath$r$yellow' does not exist. Was '$cyan`build.ps1$yellow' run?$r"
    Write-Host ""
    return
}

$manifest = Get-Content -Path $manifestPath -Raw
$result = $manifest | Where-Object { $_ -match $versionRegex }

if ($null -eq $Matches) {
    Write-Host "`n$yellowb$inv[WARN]$inv_off$r " -NoNewline
    Write-Host "$yellowb`Unable to find the $blueb`ModuleVersion$yellow in the manifest '$cyanb$manifestPath$r$yellowb'. Exiting...$r"
    Write-Host ""
    return
}

$versionText = $Matches["version"]
$versionText

$currentVersion = [version]::Parse($versionText)

$newMajor = $currentVersion.Major
$newMinor = $currentVersion.Minor
$newBuild = $currentVersion.Build
$newRevision = $currentVersion.Revision

[version]$newVersion

# increment the smallest build component
if ($currentVersion.Revision -ge 0) {
    $newRevision++
    $newVersion = [version]::new($newMajor, $newMinor, $newBuild, $newRevision)
}
elseif ($currentVersion.Build -ge 0) {
    $newBuild++
    $newVersion = [version]::new($newMajor, $newMinor, $newBuild)
}
elseif ($currentVersion.Minor -ge 0) {
    $newMinor++
    $newVersion = [version]::new($newMajor, $newMinor)
}
else {
    $newMajor++
    $newVersion = [version]::new($newMajor)
}

$title = 'Update Version'
$question = "Version '$cyanb$currentVersion$r' will be updated to '$cyanb$newVersion$r'. Are you sure you want to proceed?"
$choices = '&Yes', '&No'

$decision = $Host.UI.PromptForChoice($title, $question, $choices, 1)
if ($decision -ne 0)
{
    Write-Host "`n$yellowb`User requested cancellation. Exiting...$r"
    Write-Host ""
    return
}

$manifestContent = [System.IO.File]::ReadAllText($manifestPath)
$manifestContent = $manifestContent -replace $currentVersion, $newVersion

[System.IO.File]::WriteAllText($manifestPath, $manifestContent)

Push-Location $publishPath

# publish to PSGallery
Publish-Module -Name ".\$moduleFileName" -NuGetApiKey $env:NUGET_API_KEY

Pop-Location

# update the source manifest with the new version
$projectManifestPath = Join-Path $slnRoot "$module\$manifestFileName"

if (-not (Test-Path $projectManifestPath)) {
    throw [System.IO.FileNotFoundException]::new("Project manifest file not found.", $projectManifestPath)
    return
}

Copy-Item -Path $manifestPath -Destination $projectManifestPath -Force
