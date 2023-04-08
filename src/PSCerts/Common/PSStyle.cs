// ReSharper disable UnusedMember.Global
namespace PSCerts;

internal static class Fg
{
    public static string Black { get; set; } = $"{((char)0x1b)}[30m";
    public static string Red { get; set; } = $"{((char)0x1b)}[31m";
    public static string Green { get; set; } = $"{((char)0x1b)}[32m";
    public static string Yellow { get; set; } = $"{((char)0x1b)}[33m";
    public static string Blue { get; set; } = $"{((char)0x1b)}[34m";
    public static string Magenta { get; set; } = $"{((char)0x1b)}[35m";
    public static string Cyan { get; set; } = $"{((char)0x1b)}[36m";
    public static string White { get; set; } = $"{((char)0x1b)}[37m";
    public static string BrightBlack { get; set; } = $"{((char)0x1b)}[90m";
    public static string BrightRed { get; set; } = $"{((char)0x1b)}[91m";
    public static string BrightGreen { get; set; } = $"{((char)0x1b)}[92m";
    public static string BrightYellow { get; set; } = $"{((char)0x1b)}[93m";
    public static string BrightBlue { get; set; } = $"{((char)0x1b)}[94m";
    public static string BrightMagenta { get; set; } = $"{((char)0x1b)}[95m";
    public static string BrightCyan { get; set; } = $"{((char)0x1b)}[96m";
    public static string BrightWhite { get; set; } = $"{((char)0x1b)}[97m";
}

internal static class Bg
{
    public static string Black { get; set; } = $"{((char)0x1b)}[40m";
    public static string Red { get; set; } = $"{((char)0x1b)}[41m";
    public static string Green { get; set; } = $"{((char)0x1b)}[42m";
    public static string Yellow { get; set; } = $"{((char)0x1b)}[43m";
    public static string Blue { get; set; } = $"{((char)0x1b)}[44m";
    public static string Magenta { get; set; } = $"{((char)0x1b)}[45m";
    public static string Cyan { get; set; } = $"{((char)0x1b)}[46m";
    public static string White { get; set; } = $"{((char)0x1b)}[47m";
    public static string BrightBlack { get; set; } = $"{((char)0x1b)}[100m";
    public static string BrightRed { get; set; } = $"{((char)0x1b)}[101m";
    public static string BrightGreen { get; set; } = $"{((char)0x1b)}[102m";
    public static string BrightYellow { get; set; } = $"{((char)0x1b)}[103m";
    public static string BrightBlue { get; set; } = $"{((char)0x1b)}[104m";
    public static string BrightMagenta { get; set; } = $"{((char)0x1b)}[105m";
    public static string BrightCyan { get; set; } = $"{((char)0x1b)}[106m";
    public static string BrightWhite { get; set; } = $"{((char)0x1b)}[107m";
}

internal static class Style
{
    public static string Blink { get; set; } = $"{((char)0x1b)}[5m";
    public static string BlinkOff { get; set; } = $"{((char)0x1b)}[25m";
    public static string Bold { get; set; } = $"{((char)0x1b)}[1m";
    public static string BoldOff { get; set; } = $"{((char)0x1b)}[22m";
    public static string Hidden { get; set; } = $"{((char)0x1b)}[8m";
    public static string HiddenOff { get; set; } = $"{((char)0x1b)}[28m";
    public static string Reverse { get; set; } = $"{((char)0x1b)}[7m";
    public static string ReverseOff { get; set; } = $"{((char)0x1b)}[27m";
    public static string Italic { get; set; } = $"{((char)0x1b)}[3m";
    public static string ItalicOff { get; set; } = $"{((char)0x1b)}[23m";
    public static string Underline { get; set; } = $"{((char)0x1b)}[4m";
    public static string UnderlineOff { get; set; } = $"{((char)0x1b)}[24m";
    public static string Strikethrough { get; set; } = $"{((char)0x1b)}[9m";
    public static string StrikethroughOff { get; set; } = $"{((char)0x1b)}[29m";
    public static string Reset { get; set; } = $"{((char)0x1b)}[0m";
}
