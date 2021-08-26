using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ITWebNet.Food.Site.Models
{
    public enum EnumOrderType
    {
        Collective,
        Individual,
        Banket
    }

    public enum EnumOrderPayType
    {
        /// <summary>
        /// Наличкой курьеру
        /// </summary>
        [Display(Name="Наличными курьеру")]
        CourierCash = 1,

        /// <summary>
        /// Картой курьеру
        /// </summary>
        [Display(Name="Банковской картой курьеру")]
        CourierCard = 2,

        /// <summary>
        /// Картой на сайте
        /// </summary>
        [Display(Name="Банковской картой на сайте")]
        InternetAcquiring = 3,
    }

    public static class PayTypeConst
    {
        public const string CourierCash = "Наличными курьеру";
        public const string CourierCard = "Банковской картой курьеру";
        public const string InternetAcquiring = "Банковской картой на сайте";
        public const string CashlessPay = "Безналичный расчет";
    }

    /// <summary>
    /// Это класс словарь для русифицирования значений
    /// для отображения в представлениях типов оплаты
    /// </summary>
    public static class DictionaryOfpaymentTypes
    {
        static Dictionary<string, string> payTypeDictionary = new Dictionary<string, string>();
        static DictionaryOfpaymentTypes()
        {
            payTypeDictionary.Add("CourierCash", "Наличными курьеру");
            payTypeDictionary.Add("CourierCard", "Банковской картой курьеру");
            payTypeDictionary.Add("InternetAcquiring", "Банковской картой на сайте");
            payTypeDictionary.Add("СashlessPay", "Безналичный расчет");
        }

        /// <summary>
        /// Передаем значение которое надо руссифицировать и возвращаем его.
        /// </summary>
        /// <param name="key">Тип оплаты на аглийском</param>
        /// <returns>Возвращает русишь или неведому зверюшку.</returns>
        public static string ReturningAValueByKey(string key)
        {
            if (payTypeDictionary.ContainsKey(key))
                return payTypeDictionary[key];
            
            return "Неизвестный тип оплаты";
        }
    }
}
