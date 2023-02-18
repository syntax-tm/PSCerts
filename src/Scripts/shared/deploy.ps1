
using namespace System.Collections
using namespace System.IO
using namespace System.Text
using namespace System.Security.AccessControl
using namespace System.Security.Cryptography.X509Certificates

. "$PSScriptRoot\styles.ps1"
. "$PSScriptRoot\vars.ps1"

function script:Update-Version
{
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Path,
        [switch]$SkipConfirmation,
        [switch]$Testing
    )

    $manifestContent = Get-Content $Path -Raw

    if ($Testing)
    {
        $manifestContent = $manifestContent -replace $global:VERSION_REGEX, "`$1'99.999.9'"
        [System.IO.File]::WriteAllText($Path, $manifestContent)

        return $true
    }

    $currentVersionText = (Find-Module PSCerts -Repository PSGallery).Version
    $currentVersion = [System.Version]::Parse($currentVersionText)

    $newMajor = $currentVersion.Major
    $newMinor = $currentVersion.Minor
    $newBuild = $currentVersion.Build
    $newRevision = $currentVersion.Revision

    [version]$newVersion

    # increment the smallest build component
    if ($currentVersion.Revision -ge 0)
    {
        $newVersion = [version]::new($newMajor, $newMinor, $newBuild, ++$newRevision)
    }
    elseif ($currentVersion.Build -ge 0)
    {
        $newVersion = [version]::new($newMajor, $newMinor, ++$newBuild)
    }
    elseif ($currentVersion.Minor -ge 0)
    {
        $newVersion = [version]::new($newMajor, ++$newMinor)
    }
    else
    {
        $newVersion = [version]::new(++$newMajor)
    }

    if (!$SkipConfirmation)
    {
        $title = 'Update Version'
        $question = "`nVersion '$cyanb$currentVersion$r' will be updated to '$cyanb$newVersion$r'. Are you sure you want to proceed?"
        $choices = '&Yes', '&No'

        $decision = $Host.UI.PromptForChoice($title, $question, $choices, 1)
        if ($decision -ne 0)
        {
            Write-Host "`n$yellowb`User requested cancellation. Exiting...`n$r"
            return $false
        }
    }

    $manifestContent = $manifestContent -replace $global:VERSION_REGEX, "`$1'$newVersion'"

    [System.IO.File]::WriteAllText($Path, $manifestContent)

    return $true
}

function script:Update-Changelog
{
    [OutputType([bool])]
    param(
        [string]$Path,
        [switch]$SkipReleaseNotes,
        [switch]$Testing
    )

    if ($Testing -or $SkipReleaseNotes) { return $true }

    if (!(Test-Path $global:CHANGELOG_PATH))
    {
        Write-Warning "$global:CHANGELOG_FILE_NAME not found. Skipping release notes...."
        return $false
    }

    $manifestContent = [System.IO.File]::ReadAllText($Path)
    $changelogContent = [System.IO.File]::ReadAllText($global:CHANGELOG_PATH)
    $manifestContent = $manifestContent -replace $global:RELEASE_NOTES_REGEX, "`$1'$changelogContent'"

    [System.IO.File]::WriteAllText($Path, $manifestContent)

    return $true
}

function Update-Manifest
{
    [OutputType([bool])]
    param(
        [string]$Path,
        [switch]$SkipReleaseNotes,
        [switch]$SkipConfirmation,
        [switch]$Testing
    )

    if (-not (Test-Path $Path))
    {
        Write-Host "`n$redb$inv[ERROR] $inv_off$r" -NoNewline
        Write-Host "$redb`The published module file '$cyanb$Path$r$redb' does not exist. Was '$cyanb`build.ps1$redb' run?`n$r"
        return $false
    }

    $versionSuccess = Update-Version -Path $Path -SkipConfirmation:$SkipConfirmation -Testing:$Testing
    if (!$versionSuccess)
    {
        return $false
    }

    $changelogSuccess = Update-Changelog -Path $Path -Testing:$Testing
    if (!$changelogSuccess)
    {
        return $false
    }

    return $true
}
