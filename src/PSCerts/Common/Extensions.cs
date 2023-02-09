using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Security.Principal;

namespace PSCerts
{
    public static class Extensions
    {

        public static bool EqualsIgnoreCase(this string input, string other)
        {
            if (input == null) return other == null;
            if (other == null) return false;
            return input.Equals(other, StringComparison.InvariantCultureIgnoreCase);
        }

        public static SecureString AsSecureString(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new ArgumentNullException(nameof(s));

            var secureString = new SecureString();

            s.ToList().ForEach(secureString.AppendChar);

            return secureString;
        }

        public static string GetDescription<T>(this T enumValue)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                return null;

            var description = enumValue.ToString();
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo == null) return description;

            var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attrs is { Length: > 0 })
            {
                description = ((DescriptionAttribute)attrs[0]).Description;
            }

            return description;
        }

        public static bool TryGetValueFromDescription<T>(this string description, out T value)
            where T : struct, IConvertible
        {
            value = default;

            try
            {
                value = GetValueFromDescription<T>(description);
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static T GetValueFromDescription<T>(this string description)
            where T : struct, IConvertible
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>();

            foreach (var value in values)
            {
                var valueDescription = value.GetDescription();
                if (valueDescription.EqualsIgnoreCase(description))
                {
                    return value;
                }
            }

            throw new ArgumentException($"{typeof(T).Name} description '{description}' not found.", nameof(description));
        }
        
        public static List<T> AsList<T>(this ICollection collection)
        {
            return collection.Cast<T>().ToList();
        }

        public static string GetAccountName(this SecurityIdentifier sid)
        {
            if (sid == null) throw new ArgumentNullException(nameof(sid));

            return sid.Translate(typeof(NTAccount)).ToString();
        }
    }
}
