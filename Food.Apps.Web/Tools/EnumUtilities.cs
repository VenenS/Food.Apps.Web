using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Food.Apps.Web.Tools
{
    public class EnumUtilities
    {
        /// <summary>
        /// Возвращает список значений энума и значение атрибута DisplayName или поле Name
        /// атрибута Display.
        /// 
        /// Если атрибуты не определены - значение пропускается.
        /// </summary>
        public static IEnumerable<KeyValuePair<int, string>> DescribeValues<T>() where T : Enum
        {
            var members = typeof(T).GetFields();
            foreach (var m in members) {
                var displayName = FindDisplayName(m);
                if (displayName == null)
                    continue;

                var value = 0;

                try {
                    value = (int)m.GetValue(m);
                } catch (Exception e) {
                    throw new InvalidOperationException("Failed to cast an enum value to an int.", e);
                }

                yield return KeyValuePair.Create(value, displayName);
            }
        }

        private static string FindDisplayName(FieldInfo fi)
        {
            var dna = (DisplayNameAttribute)fi
                .GetCustomAttributes(typeof(DisplayNameAttribute), false)
                .FirstOrDefault();
            if (dna != null)
                return dna.DisplayName;

            var dn = (DisplayAttribute)fi
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault();
            if (dn != null)
                return dn.Name;

            return null;
        }
    }
}
