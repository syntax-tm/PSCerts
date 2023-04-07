# adds ANSI escape support for Windows PowerShell

if ($null -ne $PSStyle) { return }
if ($host.Name -like '*ISE*') { return }

# older terminals require manually enabling support for ANSI
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

Enable-ANSIEscapes | Out-Null

class ForegroundStyle
{
    [string]$Black = "$([char]0x1b)[30m"
    [string]$Red = "$([char]0x1b)[31m"
    [string]$Green = "$([char]0x1b)[32m"
    [string]$Yellow = "$([char]0x1b)[33m"
    [string]$Blue = "$([char]0x1b)[34m"
    [string]$Magenta = "$([char]0x1b)[35m"
    [string]$Cyan = "$([char]0x1b)[36m"
    [string]$White = "$([char]0x1b)[37m"
    [string]$BrightBlack = "$([char]0x1b)[90m"
    [string]$BrightRed = "$([char]0x1b)[91m"
    [string]$BrightGreen = "$([char]0x1b)[92m"
    [string]$BrightYellow = "$([char]0x1b)[93m"
    [string]$BrightBlue = "$([char]0x1b)[94m"
    [string]$BrightMagenta = "$([char]0x1b)[95m"
    [string]$BrightCyan = "$([char]0x1b)[96m"
    [string]$BrightWhite = "$([char]0x1b)[97m"
    hidden [string]$Reset = "$([char]0x1b)[0m"

    [string] ToString()
    {
        $sb = [System.Text.StringBuilder]::new()

        $sb.AppendFormat("Black         : {0}{1}{2}`n", $this.Black, $this.Black.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Red           : {0}{1}{2}`n", $this.Red, $this.Red.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Green         : {0}{1}{2}`n", $this.Green, $this.Green.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Yellow        : {0}{1}{2}`n", $this.Yellow, $this.Yellow.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Blue          : {0}{1}{2}`n", $this.Blue, $this.Blue.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Magenta       : {0}{1}{2}`n", $this.Magenta, $this.Magenta.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Cyan          : {0}{1}{2}`n", $this.Cyan, $this.Cyan.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("White         : {0}{1}{2}`n", $this.White, $this.White.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightBlack   : {0}{1}{2}`n", $this.BrightBlack, $this.BrightBlack.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightRed     : {0}{1}{2}`n", $this.BrightRed, $this.BrightRed.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightGreen   : {0}{1}{2}`n", $this.BrightGreen, $this.BrightGreen.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightYellow  : {0}{1}{2}`n", $this.BrightYellow, $this.BrightYellow.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightBlue    : {0}{1}{2}`n", $this.BrightBlue, $this.BrightBlue.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightMagenta : {0}{1}{2}`n", $this.BrightMagenta, $this.BrightMagenta.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightCyan    : {0}{1}{2}`n", $this.BrightCyan, $this.BrightCyan.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightWhite   : {0}{1}{2}", $this.BrightWhite, $this.BrightWhite.Replace("$([char]0x1b)", '`e'), $this.Reset)

        return $sb.ToString()
    }
}

class BackgroundStyle
{
    [string]$Black = "$([char]0x1b)[40m"
    [string]$Red = "$([char]0x1b)[41m"
    [string]$Green = "$([char]0x1b)[42m"
    [string]$Yellow = "$([char]0x1b)[43m"
    [string]$Blue = "$([char]0x1b)[44m"
    [string]$Magenta = "$([char]0x1b)[45m"
    [string]$Cyan = "$([char]0x1b)[46m"
    [string]$White = "$([char]0x1b)[47m"
    [string]$BrightBlack = "$([char]0x1b)[100m"
    [string]$BrightRed = "$([char]0x1b)[101m"
    [string]$BrightGreen = "$([char]0x1b)[102m"
    [string]$BrightYellow = "$([char]0x1b)[103m"
    [string]$BrightBlue = "$([char]0x1b)[104m"
    [string]$BrightMagenta = "$([char]0x1b)[105m"
    [string]$BrightCyan = "$([char]0x1b)[106m"
    [string]$BrightWhite = "$([char]0x1b)[107m"
    hidden [string]$Reset = "$([char]0x1b)[0m"

    [string] ToString()
    {
        $sb = [System.Text.StringBuilder]::new()

        $sb.AppendFormat("Black         : {0}{1}{2}`n", $this.Black, $this.Black.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Red           : {0}{1}{2}`n", $this.Red, $this.Red.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Green         : {0}{1}{2}`n", $this.Green, $this.Green.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Yellow        : {0}{1}{2}`n", $this.Yellow, $this.Yellow.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Blue          : {0}{1}{2}`n", $this.Blue, $this.Blue.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Magenta       : {0}{1}{2}`n", $this.Magenta, $this.Magenta.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Cyan          : {0}{1}{2}`n", $this.Cyan, $this.Cyan.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("White         : {0}{1}{2}`n", $this.White, $this.White.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightBlack   : {0}{1}{2}`n", $this.BrightBlack, $this.BrightBlack.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightRed     : {0}{1}{2}`n", $this.BrightRed, $this.BrightRed.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightGreen   : {0}{1}{2}`n", $this.BrightGreen, $this.BrightGreen.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightYellow  : {0}{1}{2}`n", $this.BrightYellow, $this.BrightYellow.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightBlue    : {0}{1}{2}`n", $this.BrightBlue, $this.BrightBlue.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightMagenta : {0}{1}{2}`n", $this.BrightMagenta, $this.BrightMagenta.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightCyan    : {0}{1}{2}`n", $this.BrightCyan, $this.BrightCyan.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("BrightWhite   : {0}{1}{2}", $this.BrightWhite, $this.BrightWhite.Replace("$([char]0x1b)", '`e'), $this.Reset)

        return $sb.ToString()
    }
}

class PSFormattingStyle
{
    [string]$FormatAccent = "$([char]0x1b)[32;1m"
    [string]$TableHeader = "$([char]0x1b)[32;1m"
    [string]$ErrorAccent = "$([char]0x1b)[36;1m"
    [string]$Error = "$([char]0x1b)[31;1m"
    [string]$Warning = "$([char]0x1b)[33;1m"
    [string]$Verbose = "$([char]0x1b)[33;1m"
    [string]$Debug = "$([char]0x1b)[33;1m"
    hidden [string]$Reset = "$([char]0x1b)[0m"

    [string] ToString()
    {
        $sb = [System.Text.StringBuilder]::new()

        $sb.AppendFormat("FormatAccent  : {0}{1}{2} `n", $this.FormatAccent, $this.FormatAccent.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("TableHeader   : {0}{1}{2} `n", $this.TableHeader, $this.TableHeader.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("ErrorAccent   : {0}{1}{2} `n", $this.ErrorAccent, $this.ErrorAccent.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Error         : {0}{1}{2} `n", $this.Error, $this.Error.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Warning       : {0}{1}{2} `n", $this.Warning, $this.Warning.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Verbose       : {0}{1}{2} `n", $this.Verbose, $this.Verbose.Replace("$([char]0x1b)", '`e'), $this.Reset)
        $sb.AppendFormat("Debug         : {0}{1}{2}", $this.Debug, $this.Debug.Replace("$([char]0x1b)", '`e'), $this.Reset)

        return $sb.ToString()
    }
}

class PSProgressStyle
{
    [string]$ProgressStyle = "$([char]0x1b)[33;1m"
    [int]$MaxWidth = 120
    [int]$View = 0
    [bool]$UseOSCIndicator = $false
}

class PSStyleFileInfo
{
    [string]$Directory = "$([char]0x1b)[44;1m"
    [string]$SymbolicLink = "$([char]0x1b)[36;1m"
    [string]$Executable = "$([char]0x1b)[32;1m"
    [string[]]$Extensions = @(
        ".zip",
        ".tgz",
        ".gz",
        ".tar",
        ".nupkg",
        ".cab",
        ".7z",
        ".ps1",
        ".psd1",
        ".psm1",
        ".ps1xml"
    )
}

class PSStyle
{
    hidden [int]$OutputRendering = 0
    $Formatting = [PSFormattingStyle]::new()
    $Background = [BackgroundStyle]::new()
    $Foreground = [ForegroundStyle]::new()
    hidden [PSStyleFileInfo]$FileInfo = [PSStyleFileInfo]::new()
    [string]$Blink = "$([char]0x1b)[5m"
    [string]$BlinkOff = "$([char]0x1b)[25m"
    [string]$Bold = "$([char]0x1b)[1m"
    [string]$BoldOff = "$([char]0x1b)[22m"
    [string]$Hidden = "$([char]0x1b)[8m"
    [string]$HiddenOff = "$([char]0x1b)[28m"
    [string]$Reverse = "$([char]0x1b)[7m"
    [string]$ReverseOff = "$([char]0x1b)[27m"
    [string]$Italic = "$([char]0x1b)[3m"
    [string]$ItalicOff = "$([char]0x1b)[23m"
    [string]$Underline = "$([char]0x1b)[4m"
    [string]$UnderlineOff = "$([char]0x1b)[24m"
    [string]$Strikethrough = "$([char]0x1b)[9m"
    [string]$StrikethroughOff = "$([char]0x1b)[29m"
    [string]$Reset = "$([char]0x1b)[0m"

    [string] ToString()
    {
        $sb = [System.Text.StringBuilder]::new()

        $sb.AppendLine("Background`n----------")
        $sb.AppendLine($this.Background)

        $sb.AppendLine("Foreground`n----------")
        $sb.AppendLine($this.Foreground)

        $sb.AppendLine("Formatting`n----------")
        $sb.AppendLine($this.Formatting)

        $sb.AppendLine("Styles`n------")
        $sb.AppendFormat("Blink         : {0}{1}{2} `n", $this.Blink, $this.Blink.Replace("$([char]0x1b)", '`e'), $this.BlinkOff)
        $sb.AppendFormat("Bold          : {0}{1}{2} `n", $this.Bold, $this.Bold.Replace("$([char]0x1b)", '`e'), $this.BoldOff)
        $sb.AppendFormat("Hidden        : {0}{1}{2} `n", $this.Hidden, $this.Hidden.Replace("$([char]0x1b)", '`e'), $this.HiddenOff)
        $sb.AppendFormat("Reverse       : {0}{1}{2} `n", $this.Reverse, $this.Reverse.Replace("$([char]0x1b)", '`e'), $this.ReverseOff)
        $sb.AppendFormat("Italic        : {0}{1}{2} `n", $this.Italic, $this.Italic.Replace("$([char]0x1b)", '`e'), $this.ItalicOff)
        $sb.AppendFormat("Underline     : {0}{1}{2} `n", $this.Underline, $this.Underline.Replace("$([char]0x1b)", '`e'), $this.UnderlineOff)
        $sb.AppendFormat("Strikethrough : {0}{1}{2}", $this.Strikethrough, $this.Strikethrough.Replace("$([char]0x1b)", '`e'), $this.StrikethroughOff)

        return $sb.ToString()
    }
}

$PSStyle = [PSStyle]::new()

Set-Variable -Name PSStyle -Option AllScope -Description "Configuration controlling how text is rendered."
