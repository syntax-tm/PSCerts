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

    private static bool CurrentUserIsAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
