# adds ANSI escape support for Windows PowerShell
if ($PSEdition -eq "Desktop")
{
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
        [int]$OutputRendering = 0
        $Formatting = [PSFormattingStyle]::new()
        $Background = [BackgroundStyle]::new()
        $Foreground = [ForegroundStyle]::new()
        $FileInfo = [PSStyleFileInfo]::new()
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
    }

    function Test-Ansi
    {
        [OutputType([bool])]
        param()

        return $host.Name -like "*ISE*"
    }

    function Get-PSStyle
    {
        [OutputType([PSStyle])]
        param()

        if (!(Test-Ansi))
        {
            Write-Host "$($host.Name) does not support ANSI escapes." -ForegroundColor Yellow
        }

        return [PSStyle]::new()
    }

    function Test-PSStyle
    {
        param(
            [Parameter(Position = 0)]
            [string]$TestMessage = "Test Message"
        )

        function Show-TestMessage
        {
            param(
                [string]$Header,
                [string]$Name,
                [string]$AnsiEscape,
                [string]$Message
            )

            $isFg = $Header -like "*Foreground*"

            if ($isFg)
            {
                $opt1 = $PSStyle.Background.Black
                $opt2 = $PSStyle.Background.White
            }
            else
            {
                $opt1 = $PSStyle.Foreground.White
                $opt2 = $PSStyle.Foreground.Black
            }

            $testString = "$AnsiEscape$($opt1)$Message$($PSStyle.Reset) $AnsiEscape$($opt2)$Message$($PSStyle.Reset)"

            $text = [string]::Format("{0}.{1,-13}: {2}", $Header, $Name, $testString)
            Write-Host $text
        }

        if ([string]::IsNullOrWhiteSpace($TestMessage))
        {
            $TestMessage = "Test Message"
        }

        $colors = $PSStyle.Foreground | Get-Member -MemberType Property | Where-Object Name -NotLike "Bright*" | Select-Object -ExpandProperty Name
        $colorType = @("Foreground", "Background")

        foreach ($styleType in $colorType)
        {
            # foreground colors
            foreach ($color in $colors)
            {
                $format = $PSStyle.$styleType
                $colorStyle = $format."$color"
                $brightColorStyle = $format."Bright$color"

                Show-TestMessage -Header $styleType -Name $color -AnsiEscape $colorStyle -Message $TestMessage
                Show-TestMessage -Header $styleType -Name "Bright$color" -AnsiEscape $brightColorStyle -Message $TestMessage
            }
        }
    }

    $PSStyle = [PSStyle]::new()

    Enable-ANSIEscapes
}
