using namespace System.Collections
using namespace System.IO
using namespace System.Text
using namespace System.Security.AccessControl
using namespace System.Security.Cryptography.X509Certificates

[string]$script:ScriptsPath = Split-Path $PSScriptRoot
[string]$script:SlnRoot = Split-Path $ScriptsPath
[string]$script:CertResourcesPath = Join-Path $SlnRoot "resources\certs"

[string]$moduleVersionToken = "@MODULE_VERSION@"
[string]$releaseNotesToken = "@RELEASE_NOTES@"

$cyan = $PSStyle.Foreground.Cyan
$cyanb = $PSStyle.Foreground.BrightCyan
$green = $PSStyle.Foreground.Green
$greenb = $PSStyle.Foreground.BrightGreen
$red = $PSStyle.Foreground.Red
$redb = $PSStyle.Foreground.BrightRed
$magenta = $PSStyle.Foreground.Magenta
$magentab = $PSStyle.Foreground.BrightMagenta
$blue = $PSStyle.Foreground.Blue
$blueb = $PSStyle.Foreground.BrightBlue
$yellow = $PSStyle.Foreground.Yellow
$yellowb = $PSStyle.Foreground.BrightYellow
$black = $PSStyle.Foreground.Black
$blackb = $PSStyle.Foreground.BrightBlack
$white = $PSStyle.Foreground.White
$whiteb = $PSStyle.Foreground.BrightWhite

$bg_cyan = $PSStyle.Background.Cyan
$bg_cyanb = $PSStyle.Background.BrightCyan
$bg_green = $PSStyle.Background.Green
$bg_bgreen = $PSStyle.Background.BrightGreen
$bg_red = $PSStyle.Background.Red
$bg_redb = $PSStyle.Background.BrightRed
$bg_magenta = $PSStyle.Background.Magenta
$bg_magentab = $PSStyle.Background.BrightMagenta
$bg_blue = $PSStyle.Background.Blue
$bg_blueb = $PSStyle.Background.BrightBlue
$bg_yellow = $PSStyle.Background.Yellow
$bg_yellowb = $PSStyle.Background.BrightYellow
$bg_black = $PSStyle.Background.Black
$bg_blackb = $PSStyle.Background.BrightBlack
$bg_white = $PSStyle.Background.White
$bg_whiteb = $PSStyle.Background.BrightWhite

$inv = $PSStyle.Reverse
$inv_off = $PSStyle.ReverseOff

$b = $PSStyle.Bold
$b_off = $PSStyle.BoldOff
$bo = $bo

$i = $PSStyle.Italic
$i_off = $PSStyle.ItalicOff
$io = $io

$u = $PSStyle.Underline
$u_off = $PSStyle.UnderlineOff
$uo = $uo_off

$s = $PSStyle.Strikethrough
$s_off = $PSStyle.StrikethroughOff
$so = $s_off

$th = $PSStyle.Formatting.TableHeader
$fmt_acc = $PSStyle.Formatting.FormatAccent
$cc = $fmt_acc
$err_acc = $PSStyle.Formatting.ErrorAccent
$err = $err_acc
$warn = $PSStyle.Formatting.Warning

$r = $PSStyle.Reset
$fr = "$inv_off$bo$io$uo$so$r"

class TestCert
{
    hidden [bool]$_certLoaded
    hidden [string]$_importedPath

    hidden [X509Certificate2]$Certificate
    hidden [string]$Thumbprint

    [string]$FullName
    [string]$Name
    [string]$Password
    [StoreLocation]$Location
    [StoreName]$Store
    [string]$Extension
    [bool]$Exportable

    TestCert ([string]$fileName, [string]$pass, [StoreLocation]$location, [StoreName]$store, [bool]$isExportable = $true)
    {
        $this.FullName = Join-Path $script:CertResourcesPath $fileName
        $this.Name = $fileName
        $this.Password = $pass
        $this.Location = $location
        $this.Store = $store
        $this.Exportable = $isExportable

        $this.LoadCertificate()
    }

    hidden [void] LoadCertificate()
    {
        if ($this._certLoaded) { return }

        $this.Validate()
        try
        {
            Write-Verbose "Loading certificate file '$($this.FullName)'..."

            [X509KeyStorageFlags]$storageFlags = 'PersistKeySet'
            if ($this.Exportable) { $storageFlags += 'Exportable' }
            if ($this.Location -eq 'LocalMachine') { $storageFlags += 'MachineKeySet' }

            $securePassword = $this.Password | ConvertTo-SecureString -AsPlainText -Force

            $cert = [X509Certificate2]::new($this.FullName, $securePassword, $storageFlags)
            $this.Certificate = $cert
            $this.Thumbprint = $cert.Thumbprint
            $this._importedPath = "Cert:\$($this.Location)\$($this.Store)$($cert.Thumbprint)"
            $this._certLoaded = $true

            Write-Verbose "Certificate '$($this.Name)' ($($this.Thumbprint)) loaded successfully."
        }
        catch
        {
            Write-Error "An error occurred loading certificate file '$($this.Name)'. $($_.ErrorDetails.Message)" -Category ReadError -ErrorAction Inquire
            Write-Host ""
        }
    }

    hidden [void] Validate()
    {
        if ([string]::IsNullOrWhiteSpace($this.FullName)) { throw [System.ArgumentNullException]::new("FullName")  }
        if ([string]::IsNullOrWhiteSpace($this.Name)) { throw [System.ArgumentNullException]::new("Name")  }
        if ([string]::IsNullOrWhiteSpace($this.Password)) { throw [System.ArgumentNullException]::new("Password")  }
        if ([string]::IsNullOrWhiteSpace($this.Store)) { throw [System.ArgumentNullException]::new("Store")  }

        if (![File]::Exists($this.FullName)) { throw [FileNotFoundException]::new("Certificate file does not exist.", $this.FullName)  }
    }

    [bool] IsImported()
    {
        return Test-Path $this._importedPath
    }

    [void] Import()
    {
        if ($this.IsImported()) { return }

        [X509Store]$certStore = $null

        try
        {
            $certStore = [X509Store]::new($this.Store, $this.Location)
            $certStore.Open('ReadWrite')
            $certStore.Add($this.Certificate)
        }
        finally
        {
            if ($null -ne $certStore -and $certStore.IsOpen) { $certStore.Close() }
        }
    }

    [void] Remove()
    {
        if (-not $this.IsImported()) { return }
        Remove-Item -Path $this._importedPath -Force | Out-Null
    }
}

[arraylist]$script:TestCerts = @(
    [TestCert]::new("PSCerts_01.pfx", "PSCerts_01", "LocalMachine", "My", $true)
)

function Import-TestCerts()
{
    param()

    foreach ($cert in $script:testCerts)
    {
        $cert.Import()
    }
}

function Remove-TestCerts()
{
    param()

    foreach ($cert in $script:TestCerts)
    {
        if (-not $cert.IsImported()) { continue }

        $cert.Remove()
    }
}

$module = 'PSCerts'
$repoRoot = Split-Path $script:SlnRoot
$publishPath = Join-Path $script:SlnRoot "publish"

$moduleFileName = "$module.psm1"
$manifestFileName = "$module.psd1"
$manifestPath = Join-Path $publishPath $manifestFileName
$changelogFileName = "CHANGELOG.txt"
$changelogPath = Join-Path $repoRoot $changelogFileName

$moduleVersionToken = "@MODULE_VERSION@"
$releaseNotesToken = "@RELEASE_NOTES@"

function Update-Version
{
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Path,
        [switch]$SkipConfirmation,
        [switch]$Testing
    )

    $manifestContent = Get-Content $Path -Raw

    if (!$manifestContent.Contains($moduleVersionToken))
    {
        Write-Host "`n$redb$inv[ERROR]$inv_off$r " -NoNewline
        Write-Host "$redb`Unable to find the $cyanb`ModuleVersion$redb token in the manifest. Exiting...`n$r"
        return
    }

    if ($Testing)
    {
        $manifestContent = $manifestContent -replace $moduleVersionToken, '99.99.999'
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
        $newRevision++
        $newVersion = [version]::new($newMajor, $newMinor, $newBuild, $newRevision)
    }
    elseif ($currentVersion.Build -ge 0)
    {
        $newBuild++
        $newVersion = [version]::new($newMajor, $newMinor, $newBuild)
    }
    elseif ($currentVersion.Minor -ge 0)
    {
        $newMinor++
        $newVersion = [version]::new($newMajor, $newMinor)
    }
    else
    {
        $newMajor++
        $newVersion = [version]::new($newMajor)
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

    $manifestContent = $manifestContent -replace $moduleVersionToken, $newVersion

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
        Write-Host "`n$yellowb$inv[WARN] $inv_off$r" -NoNewline
        Write-Host "$yellowb`The published module file '$cyanb$Path$r$yellowb' does not exist. Was '$cyanb`build.ps1$yellowb' run?`n$r"
        return $false
    }

    $versionSuccess = Update-Version -Path $Path -SkipConfirmation:$SkipConfirmation -Testing:$Testing
    if (!$versionSuccess) {
        return $false
    }

    $manifestContent = [System.IO.File]::ReadAllText($Path)

    if (!$Testing -and !$SkipReleaseNotes)
    {
        if (!$manifestContent.Contains($releaseNotesToken))
        {
            Write-Host "`n$redb$inv[ERROR]$inv_off$r " -NoNewline
            Write-Host "$redb`Unable to find the $cyanb`ReleaseNotes$redb token in the manifest. Exiting...`n$r"
            return $false
        }

        if (!(Test-Path $changelogPath))
        {
            Write-Host "`n$redb$inv[ERROR]$inv_off$r " -NoNewline
            Write-Host "$redb`Release notes file '$cyanb$changelogFileName$redb' not found. Exiting...`n$r"
            return $false
        }

        $changelogContent = [System.IO.File]::ReadAllText($changelogPath)

        if ([string]::IsNullOrWhiteSpace($changelogContent))
        {
            Write-Host "`n$redb$inv[ERROR]$inv_off$r " -NoNewline
            Write-Host "$redb`Release notes are required. Exiting...`n$r"
            return $false
        }

        $manifestContent = $manifestContent -replace $releaseNotesToken, $changelogContent

        [System.IO.File]::WriteAllText($Path, $manifestContent)
    }

    return $true
}
