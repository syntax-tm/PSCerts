using System;
using System.Management.Automation;

namespace PSCerts.Util
{
    public static class PowerShellHelper
    {
        public static PSCmdlet CurrentCmdlet { get; set; }
        
        public static void WriteVerbose(string message)
        {
            CurrentCmdlet?.WriteVerbose(message);
        }

        public static void Debug(string message)
        {
            CurrentCmdlet?.WriteDebug(message);
        }

        public static void Info(string message)
        {
            CurrentCmdlet?.WriteInformation(new (message, null));
        }

        public static void Warn(string message)
        {
            CurrentCmdlet?.WriteWarning(message);
        }

        public static void Error(Exception ex, bool fatal = false)
        {
            if (fatal)
            {
                CurrentCmdlet?.ThrowTerminatingException(ex);
                return;
            }
            CurrentCmdlet?.ThrowException(ex);
        }
    }
}
