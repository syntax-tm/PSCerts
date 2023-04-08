using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace PSCerts
{
    public static class StringExtensions
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

        public static string Truncate(this string source, int length, bool padRight = false, string overflowString = @"...")
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(overflowString)) throw new ArgumentNullException(nameof(overflowString));

            if (source.Length <= length)
            {
                return padRight
                    ? source.PadRight(length, ' ')
                    : source;
            }

            var overflowLength = overflowString.Length;

            var sb = new StringBuilder();
            sb.Append(source.Substring(0, length - overflowLength));
            sb.Append(overflowString);
            return sb.ToString();
        }
    }
}
