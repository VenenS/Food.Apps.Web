using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITWebNet.Food.Site.Helpers
{
    public static class DeclensionHelper
    {
        /// <summary>
        /// Возвращает переданное существительное/глагол с нужным окончанием (в теории)
        /// !Warning! возможен говнокод
        /// </summary>
        /// <param name="count"> Количество элемента </param>
        /// <param name="one"> Один элемент </param>
        /// <param name="two"> От двух до четырех элментов </param>
        /// <param name="five"> От пяти до двадцати элементов </param>
        /// <returns></returns>
        public static string GetNoun(long count, string one, string two, string five)
        {
            count = Math.Abs(count);
            count %= 100;
            if (count >= 5 && count <= 20)
                return five;
            count %= 10;
            if (count == 1)
                return one;
            if (count >= 2 && count <= 4)
                return two;
            return five;
        }

        /// <summary>
        /// Возвращает переданное прилагательное с нужным окончанием (в теории)
        /// !Warning! возможен говнокод
        /// </summary>
        /// <param name="count"> Количество элемента </param>
        /// <param name="one"> Один элемент </param>
        /// <param name="two"> От двух до четырех элментов </param>
        /// <returns></returns>
        public static string GetAdjective(long count, string one, string two)
        {
            count %= 100;
            if (count == 11)
                return two;
            count %= 10;
            if (count == 1)
                return one;
            return two;
        }
    }
}