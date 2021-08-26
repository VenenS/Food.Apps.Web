using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ITWebNet.Food.Site
{
    public static class StringUtilities
    {
        /// <summary>
        /// Функция для добавления пробелов после знаков пунктуации - точек, запятых и т.д.
        /// Используется, в частности, для обработки состава и описания блюда при редактировании блюда.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string AddSpacesAfterPunctuations(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                // Регулярное выражение. Ищет знак пунктуации из перечисленных, перед которым и после которого идёт не пробельный символ. После знаков пунктуации, которые идут в начале и в конце строки, пробел не ставим.
                Regex regex = new Regex(@"\S[.,:;)!?]\S");
                MatchCollection matches = regex.Matches(source);
                StringBuilder sbr = new StringBuilder(source.Length * 2);
                int pos = 0;
                // Если регулярное выражение нашло совпадения - копируем эти совпадения, добавляя пробелы:
                foreach (Match match in matches)
                {
                    if (match.Index > pos)
                    {
                        string before = source.Substring(pos, match.Index + 2 - pos);
                        sbr.Append(before);
                        sbr.Append(' ');
                    }
                    pos = match.Index + 2;
                }
                // Копируем оставщуюся часть строки:
                if (pos < source.Length)
                    sbr.Append(source.Substring(pos));
                return sbr.ToString();
            }
            // Если строка пустая - значит, знаков пунктуации в ней нет и пробелы добавлять не нужно:
            return source;
        }
    }
}