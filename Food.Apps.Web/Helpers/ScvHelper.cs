using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace ITWebNet.Food.Site.Helpers
{
    public static class ScvHelper
    {
        public static string ToScvString(IEnumerable collection)
        {
            if (collection == null)
                return null;

            Type[] itemsType = collection.GetType().GetGenericArguments();
            if (itemsType.Length == 0)
            {
                List<string> rows = new List<string>();
                foreach (var item in collection)
                    rows.Add(Escape(item.ToString()));

                return string.Join(Environment.NewLine, rows);
            }
            else if (itemsType.Length == 1 && itemsType[0].IsClass)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var item in collection)
                    builder.AppendLine(GetObjectValues(item));

                return builder.ToString();
            }
            else
                return null;
        }

        public static string GetObjectValues(object item)
        {
            var properties = item.GetType().GetProperties(
                BindingFlags.GetProperty |
                BindingFlags.Public |
                BindingFlags.Instance);

            var collections = properties.Where(i => i.PropertyType.GetInterface(typeof(IEnumerable).FullName) != null);
            var classes = properties.Except(collections).Where(i => i.PropertyType.IsClass);
            var primitives = properties.Except(collections).Where(i => i.PropertyType.IsValueType);

            var headers = primitives.Select(i => GetDisplayName(i));

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Join(",", headers));

            var values = properties.Select(i => Escape(i.GetValue(item).ToString()));
            builder.AppendLine(string.Join(",", values));

            foreach (var property in classes)
                builder.AppendLine(GetObjectValues(item));

            foreach (var property in collections)
                builder.AppendLine(ToScvString((IEnumerable)property.GetValue(item)));

            return builder.ToString();
        }

        private static string GetDisplayName(PropertyInfo item)
        {
            DescriptionAttribute descAttribute = item.GetCustomAttribute<DescriptionAttribute>();

            if (descAttribute != null)
                return descAttribute.Description;
            else
            {
                DisplayAttribute displayAttribute = item.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                    return displayAttribute.Name;
                else
                    return item.Name;
            }
        }

        private static string Escape(this string s)
        {
            if (s.Contains(QUOTE))
                s = s.Replace(QUOTE, ESCAPED_QUOTE);

            if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
                s = QUOTE + s + QUOTE;

            return s;
        }

        private static string Unescape(this string s)
        {
            if (s.StartsWith(QUOTE) && s.EndsWith(QUOTE))
            {
                s = s.Substring(1, s.Length - 2);

                if (s.Contains(ESCAPED_QUOTE))
                    s = s.Replace(ESCAPED_QUOTE, QUOTE);
            }

            return s;
        }

        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };
    }
}