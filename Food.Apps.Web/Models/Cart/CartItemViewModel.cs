using System;
using System.ComponentModel.DataAnnotations;
using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Site.Models
{
    public class CartItemViewModel
    {
        [Range(0, 100000)] public int Count { get; set; }

        [Display(Name = "Комментарий к блюду")]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        public long Id { get; set; }

        public long? OrderItemId { get; set; }

        [Display(Name = "Название")] public string Name { get; set; }

        [Display(Name = "Калорийность")] public double? Kcalories { get; set; }

        [Display(Name = "Вес")] public double? Weight { get; set; }

        [Display(Name = "Описание веса")] public string WeightDescription { get; set; }

        [Display(Name = "Цена")] public double BasePrice { get; set; }

        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Состав")] public string Composition { get; set; }

        /// <summary>
        ///     скидка в процентах! на нее не умножать ибо выйдет бред, само посчитается в свойстве Price
        /// </summary>
        public int Discount { get; set; }

        /// <summary>
        ///     Цена со скидкой
        /// </summary>
        public double Price => Math.Round(BasePrice * (1 - Discount * 0.01), 2);

        public double TotalPrice => Price * Count;

        public string Image { get; set; }
        public DateTime DeliveryDate { get; set; }

        public long CafeId { get; set; }

        internal static CartItemViewModel FromDish(FoodDishModel dish, int clientDiscount, DateTime? date)
        {
            return new CartItemViewModel
            {
                Id = dish.Id,
                Name = dish.Name,
                BasePrice = dish.BasePrice,
                Composition = dish.Composition,
                Count = 1,
                Description = dish.Description,
                Discount = clientDiscount,
                Kcalories = dish.Kcalories,
                Weight = dish.Weight,
                WeightDescription = dish.WeightDescription,
                Image = dish.Image,
                DeliveryDate = date ?? DateTime.Now.Date,
                CafeId = dish.CafeId
            };
        }
    }
}