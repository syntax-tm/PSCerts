using System;
using System.Management.Automation;

namespace PSCerts.Util
{
    public static class PowerShellHelper
    {
        private static PowerShell _default;
        public static PowerShell Default
        {
            get
            {
                if (_default != null) return _default;

                _default = PowerShell.Create();

                return _default;
            }
        }
        
        public static void Debug(string message)
        {
            using var ps = PowerShell.Create();
            ps.AddCommand("Write-Debug");
            ps.AddParameter("Message", message);
            ps.Invoke();
        }

        public static void Verbose(string message)
        {
            using var ps = PowerShell.Create();
            ps.AddCommand("Write-Verbose");
            ps.AddParameter("Message", message);
            ps.Invoke();
        }

        public static void Info(string message)
        {
            using var ps = PowerShell.Create();
            ps.AddCommand("Write-Information");
            ps.AddParameter("MessageData", message);
            ps.Invoke();
        }

        public static void Warn(string message)
        {
            using var ps = PowerShell.Create();
            ps.AddCommand("Write-Warning");
            ps.AddParameter("Message", message);
            ps.Invoke();
        }

        public static void Error(Exception ex)
        {
            using var ps = PowerShell.Create();
            ps.AddCommand("Write-Error");
            ps.AddParameter("Exception", ex);
            ps.AddParameter("Message", ex.Message);
            ps.Invoke();
        }
    }
}
