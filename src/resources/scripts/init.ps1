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
    Enable-ANSIEscapes | Out-Null
}
