using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;

namespace PSCerts
{
    public static class Extensions
    {

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
        
        public static List<T> AsList<T>(this ICollection collection)
        {
            return collection.Cast<T>().ToList();
        }

    }
}
