
$module = 'PSCerts'
$slnRoot = Split-Path $PSScriptRoot
$repoRoot = Split-Path $slnRoot
$publishPath = Join-Path $slnRoot "publish"

$moduleFileName = "$module.psm1"
$manifestFileName = "$module.psd1"
$manifestPath = Join-Path $publishPath $manifestFileName
$changelogFileName = "CHANGELOG.txt"
$changelogPath = Join-Path $repoRoot $changelogFileName

$moduleVersionToken = "@MODULE_VERSION@"
$releaseNotesToken = "@RELEASE_NOTES@"

$ErrorActionPreference = 'Stop'

Set-Location $PSScriptRoot

. .\shared\utils.ps1

if (-not (Test-Path $manifestPath)) {
    Write-Host "`n$yellowb$inv[WARN] $inv_off$r" -NoNewline
    Write-Host "$yellowb`The published module file '$cyanb$manifestPath$r$yellowb' does not exist. Was '$cyanb`build.ps1$yellowb' run?`n$r"
    return
}

$manifest = Get-Content -Path $manifestPath -Raw

if (!$manifest.Contains($releaseNotesToken)) {
    Write-Host "`n$redb$inv[ERROR]$inv_off$r " -NoNewline
    Write-Host "$redb`Unable to find the $cyanb`ReleaseNotes$redb token in the manifest. Exiting...`n$r"
    return
}

if (!(Test-Path $changelogPath)) {
    Write-Host "`n$redb$inv[ERROR]$inv_off$r " -NoNewline
    Write-Host "$redb`Release notes file '$cyanb$changelogFileName$redb' not found. Exiting...`n$r"
    return
}

$changelogContent = [System.IO.File]::ReadAllText($changelogPath)

if ([string]::IsNullOrWhiteSpace($changelogContent)) {
    Write-Host "`n$redb$inv[ERROR]$inv_off$r " -NoNewline
    Write-Host "$redb`Release notes are required. Exiting...`n$r"
    return
}

if (!$manifest.Contains($moduleVersionToken)) {
    Write-Host "`n$redb$inv[ERROR]$inv_off$r " -NoNewline
    Write-Host "$redb`Unable to find the $cyanb`ModuleVersion$redb token in the manifest. Exiting...`n$r"
    return
}

$currentVersionText = (Find-Module PSCerts -Repository PSGallery).Version
$currentVersion = [System.Version]::Parse($currentVersionText)

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
$question = "`nVersion '$cyanb$currentVersion$r' will be updated to '$cyanb$newVersion$r'. Are you sure you want to proceed?"
$choices = '&Yes', '&No'

$decision = $Host.UI.PromptForChoice($title, $question, $choices, 1)
if ($decision -ne 0) {
    Write-Host "`n$yellowb`User requested cancellation. Exiting...`n$r"
    return
}

$manifestContent = [System.IO.File]::ReadAllText($manifestPath)
$manifestContent = $manifestContent -replace $moduleVersionToken, $newVersion
$manifestContent = $manifestContent -replace $releaseNotesToken, $changelogContent

[System.IO.File]::WriteAllText($manifestPath, $manifestContent)

Push-Location $publishPath

# publish to PSGallery
Publish-Module -Name ".\$moduleFileName" -NuGetApiKey $env:PSGALLERY_NUGET_API_KEY

Pop-Location
