using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Security.Principal;
using PSCerts.Commands;
using PSCerts.Util;

namespace PSCerts
{
    public static class Extensions
    {

#if CODE_ANALYSIS
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement", Justification = "Readability")]
#endif
        public static bool EqualsIgnoreCase(this string input, string other)
        {
            if (input == null) return other == null;
            if (other == null) return false;
            return input.Equals(other, StringComparison.InvariantCultureIgnoreCase);
        }
        
        public static bool Equals(this string input, string other, bool ignoreCase)
        {
            if (input == null) return other == null;
            if (other == null) return false;

            var comparison = ignoreCase
                ? StringComparison.InvariantCultureIgnoreCase
                : StringComparison.InvariantCulture;

            return input.Equals(other, comparison);
        }
        
        public static List<T> AsList<T>(this ICollection collection)
        {
            return collection.Cast<T>().ToList();
        }
        
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement", Justification = "Readability")]
        public static string GetAccountName(this SecurityIdentifier sid)
        {
            if (sid == null) throw new ArgumentNullException(nameof(sid));

            return sid.Translate(typeof(NTAccount)).ToString();
        }
    }
}
