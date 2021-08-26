using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site
{
    public static class CafeExtensions
    {
        public static double GetAverageRating(this CafeModel cafe)
        {
            double average = 0;

            if (cafe.CafeRatingCount > 0)
                average = (double)cafe.CafeRatingSumm / (double)cafe.CafeRatingCount;

            if (average > 5.0)
                average = 5.0;

            return Math.Round(average, 1);
        }

        public static string GetRatingMessage(this CafeModel cafe)
        {
            var count = cafe.CafeRatingCount;
            long lastDigit = count % 10; // Получаем дробную часть
            long preLastDigit = (count / 10) % 10; //убираем один разряд и снова получаем дробную часть
            string casedPeopleVal =
                DeclensionHelper.GetNoun(
                    count,
                    "человек",
                    "человека",
                    "человек");
            string casedVotesVal =
                DeclensionHelper.GetNoun(
                    count,
                    "оценил",
                    "оценило",
                    "оценило");

            if (count > 0)
                return string.Format("{2} {0} {1}", cafe.CafeRatingCount, casedPeopleVal, casedVotesVal);
            else
                return "пока не оценено";
        }

        public static bool IsClosed(this CafeModel cafe)
        {
            bool result = true;

            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            result &= cafe.WorkingTimeFrom.HasValue
                && cafe.WorkingTimeFrom.Value.TimeOfDay > currentTime;

            result |= cafe.WorkingTimeTo.HasValue
                && cafe.WorkingTimeTo.Value.TimeOfDay < currentTime;

            return result;
        }

        public static bool IsDishCafeClosed(this FoodDishModel dish)
        {
            bool result = true;

            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            result &= dish.CafeTimeFrom.HasValue
                && dish.CafeTimeFrom.Value.TimeOfDay > currentTime;

            result |= dish.CafeTimeTo.HasValue
                && dish.CafeTimeTo.Value.TimeOfDay < currentTime;

            return result;
        }
    }

    public static class FoodCategoryExtensions
    {
        public static async Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetDicFoodCategoryWithDishes(List<FoodCategoryWithDishes> model)
        {
            var result = new Dictionary<FoodCategoryModel, List<FoodDishModel>>();
            foreach (var el in model)
            {
                result.Add(el.Category, el.Dishes);
            }
            return result;
        }
    }

    public static class OrdersExtensions
    {
        public static string GetHeaderRow(this CompanyOrderModel order)
        {
            return string.Join(",",
                "ID",
                "Время поступления",
                "Наименование",
                "Количество заказов",
                "Стоимость заказов");
        }

        public static string ToCsvString(this CompanyOrderModel order)
        {
            return string.Join(",",
                order.Id,
                order.OrderAutoCloseDate,
                order.Company.Name.Escape(),
                order.UserOrders.Count,
                order.UserOrders.Sum(i => i.TotalSum));
        }

        public static string GetHeaderRow(this OrderModel order)
        {
            return string.Join(",",
                "ID",
                "Время поступления",
                "Имя клиента",
                "Телефон",
                "Стоимость заказа");
        }
        public static string ToCsvString(this OrderModel order)
        {
            return string.Join(",",
                order.Id,
                order.Create,
                (order.Creator.UserFullName ?? "—————").Escape(),
                order.PhoneNumber.Escape(),
                order.TotalSum.ToString().Escape());
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

    public static class EnumExtensions
    {
        /// <summary>
        /// Получает описание элемента <see cref="System.Enum" /> , заданное в
        /// <see cref="System.ComponentModel.DescriptionAttribute" /> , либо имя элемента.
        /// </summary>
        /// <param name="value"> Элемент <see cref="System.Enum" /> . </param>
        /// <returns> Опимание либо имя элемента <see cref="System.Enum" /> . </returns>
        public static string GetDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute = fi.GetCustomAttribute<DescriptionAttribute>();

            if (attribute != null)
                return attribute.Description;
            else
                return value.ToString();
        }
    }

    public static class OrderExtensions
    {
        public static Task<List<OrderStatusEnum>> GetAvailableStatuses(this OrderModel order, OrderService client)
        {
            return client.GetAvailableOrderStatuses(order.Id, true);
        }
    }

    public static class DeliveryAddressExtensions
    {
        public static string GetAddressString(this DeliveryAddressModel address, bool forceFull = false)
        {
            if (address == null)
                return string.Empty;
            if (!forceFull && address.DisplayType == DisplayAddressType.Display)
                return address.RawAddress;

            var result = $"г. {address.CityName}, ул. {address.StreetName}, д. {address.HouseNumber}";
            if (!string.IsNullOrEmpty(address.BuildingNumber))
                result = result + " к. " + address.BuildingNumber;

            result = result + $" кв/оф {address.OfficeNumber}";

            return result;
        }
    }
}