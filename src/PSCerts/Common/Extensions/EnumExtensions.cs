using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSCerts
{
    public static class EnumExtensions
    {
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
    }
}
