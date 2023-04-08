using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;

namespace PSCerts
{
    public static class IdentityExtensions
    {
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement", Justification = "Readability")]
        public static string GetAccountName(this SecurityIdentifier sid)
        {
            if (sid == null) throw new ArgumentNullException(nameof(sid));

            return sid.Translate(typeof(NTAccount)).ToString();
        }
    }
}
