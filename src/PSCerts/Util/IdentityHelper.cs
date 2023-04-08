using System;
using System.Security.Principal;

namespace PSCerts.Util;

public static class IdentityHelper
{
    private static bool? _isAdministrator;
    public static bool IsAdministrator
    {
        get
        {
            if (_isAdministrator != null) return _isAdministrator.Value;
            _isAdministrator = CurrentUserIsAdmin();
            return _isAdministrator.Value;
        }
    }

    public static string GetShortIdentityName(string identity)
    {
        if (string.IsNullOrWhiteSpace(identity)) return string.Empty;

        var name = identity;

        name = name.Replace($@"{Environment.MachineName}\", string.Empty);
        name = name.Replace(@"NT AUTHORITY\", string.Empty);
        name = name.Replace(@"BUILTIN\", string.Empty);

        return name;
    }

    private static bool CurrentUserIsAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
