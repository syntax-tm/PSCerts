using namespace System.Collections
using namespace System.IO
using namespace System.Text
using namespace System.Security.AccessControl
using namespace System.Security.Cryptography.X509Certificates

[string]$script:ScriptsPath = Split-Path $PSScriptRoot
[string]$script:SlnRoot = Split-Path $ScriptsPath
[string]$script:CertResourcesPath = Join-Path $SlnRoot "resources\certs"

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
            $this._importedPath = Join-Path "Cert:" $this.Location $this.Store $cert.Thumbprint
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
            $certStore = Get-Item "Cert:\$($this.Location)\$($this.Store)"
            $certStore.Add($this.Certificate)
        }
        finally
        {
            if ($certStore.IsOpen) { $certStore.Close() }
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

function Enable-ANSIEscapes
{
    # Enable ANSI / VT100 16-color escape sequences:
    # Original discovery blog post:
    # http://stknohg.hatenablog.jp/entry/2016/02/22/195644
    # Esc sequence support documentation
    # https://msdn.microsoft.com/en-us/library/windows/desktop/mt638032(v=vs.85).aspx

    # This doesn't do anything if the type is already added, so don't worry
    # about doing this every single time, I guess
    Add-Type -MemberDefinition @"
[DllImport("kernel32.dll", SetLastError=true)]
public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
[DllImport("kernel32.dll", SetLastError=true)]
public static extern IntPtr GetStdHandle(int handle);
[DllImport("kernel32.dll", SetLastError=true)]
public static extern bool GetConsoleMode(IntPtr handle, out int mode);
"@ -Namespace Win32 -Name NativeMethods

    # GetStdHandle: https://msdn.microsoft.com/en-us/library/windows/desktop/ms683231(v=vs.85).aspx
    # -11 is the code for STDOUT (-10 is STDIN, -12 is STDERR)
    $Handle = [Win32.NativeMethods]::GetStdHandle(-11)

    # GetConsoleMode: https://msdn.microsoft.com/en-us/library/windows/desktop/ms683167(v=vs.85).aspx
    # get the console "mode" --- contains info about how to handle
    # wrapping, etc. $Mode is set by reference by GetConsoleMode
    $Mode = 0
    [Win32.NativeMethods]::GetConsoleMode($Handle, [ref]$Mode)
    # the mode is a bitmask so we binary or with 0x0004
    # (ENABLE_VIRTUAL_TERMINAL_PROCESSING)

    # SetConsoleMode: https://msdn.microsoft.com/en-us/library/windows/desktop/ms686033(v=vs.85).aspx
    return [Win32.NativeMethods]::SetConsoleMode($Handle, $Mode -bor 4)
}

# older terminals require manually enabling support for ANSI
if ($PSEdition -eq "Desktop")
{
    Enable-ANSIEscapes
}

$cyan        = $PSStyle.Foreground.Cyan
$cyanb       = $PSStyle.Foreground.BrightCyan
$green       = $PSStyle.Foreground.Green
$greenb      = $PSStyle.Foreground.BrightGreen
$red         = $PSStyle.Foreground.Red
$redb        = $PSStyle.Foreground.BrightRed
$magenta     = $PSStyle.Foreground.Magenta
$magentab    = $PSStyle.Foreground.BrightMagenta
$blue        = $PSStyle.Foreground.Blue
$blueb       = $PSStyle.Foreground.BrightBlue
$yellow      = $PSStyle.Foreground.Yellow
$yellowb     = $PSStyle.Foreground.BrightYellow
$black       = $PSStyle.Foreground.Black
$blackb      = $PSStyle.Foreground.BrightBlack
$white       = $PSStyle.Foreground.White
$whiteb      = $PSStyle.Foreground.BrightWhite

$fg_cyan     = $PSStyle.Foreground.Cyan
$fg_cyanb    = $PSStyle.Foreground.BrightCyan
$fg_green    = $PSStyle.Foreground.Green
$fg_greenb   = $PSStyle.Foreground.BrightGreen
$fg_red      = $PSStyle.Foreground.Red
$fg_redb     = $PSStyle.Foreground.BrightRed
$fg_magenta  = $PSStyle.Foreground.Magenta
$fg_magentab = $PSStyle.Foreground.BrightMagenta
$fg_blue     = $PSStyle.Foreground.Blue
$fg_blueb    = $PSStyle.Foreground.BrightBlue
$fg_yellow   = $PSStyle.Foreground.Yellow
$fg_yellowb  = $PSStyle.Foreground.BrightYellow
$fg_black    = $PSStyle.Foreground.Black
$fg_blackb   = $PSStyle.Foreground.BrightBlack
$fg_white    = $PSStyle.Foreground.White
$fg_whiteb   = $PSStyle.Foreground.BrightWhite

$bg_cyan     = $PSStyle.Background.Cyan
$bg_cyanb    = $PSStyle.Background.BrightCyan
$bg_green    = $PSStyle.Background.Green
$bg_bgreen   = $PSStyle.Background.BrightGreen
$bg_red      = $PSStyle.Background.Red
$bg_redb     = $PSStyle.Background.BrightRed
$bg_magenta  = $PSStyle.Background.Magenta
$bg_magentab = $PSStyle.Background.BrightMagenta
$bg_blue     = $PSStyle.Background.Blue
$bg_blueb    = $PSStyle.Background.BrightBlue
$bg_yellow   = $PSStyle.Background.Yellow
$bg_yellowb  = $PSStyle.Background.BrightYellow
$bg_black    = $PSStyle.Background.Black
$bg_blackb   = $PSStyle.Background.BrightBlack
$bg_white    = $PSStyle.Background.White
$bg_whiteb   = $PSStyle.Background.BrightWhite

$inv         = $PSStyle.Reverse
$inv_off     = $PSStyle.ReverseOff

$b           = $PSStyle.Bold
$b_off       = $PSStyle.BoldOff
$bo          = $bo

$i           = $PSStyle.Italic
$i_off       = $PSStyle.ItalicOff
$io          = $io

$u           = $PSStyle.Underline
$u_off       = $PSStyle.UnderlineOff
$uo          = $uo_off

$s           = $PSStyle.Strikethrough
$s_off       = $PSStyle.StrikethroughOff
$so          = $s_off

$th          = $PSStyle.Formatting.TableHeader
$fmt_acc     = $PSStyle.Formatting.FormatAccent
$cc          = $fmt_acc
$err_acc     = $PSStyle.Formatting.ErrorAccent
$err         = $err_acc
$warn        = $PSStyle.Formatting.Warning

$r           = $PSStyle.Reset
$reset       = $r

$fr          = "$inv_off$bo$io$uo$so$r"
$reset_force = $fr