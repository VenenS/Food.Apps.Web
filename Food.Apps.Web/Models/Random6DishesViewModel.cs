using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;

namespace ITWebNet.Food.Site.Models
{
    public class Random6DishesViewModel
    {
        public List<FoodCategoryWithDishes> FoodCategoryWithDishes { get; set; }
        public MenuViewModel Menu { get; set; }
        /// <summary>
        /// Количество блюд, отображаемых на главной странице в каждой категории до нажатия кнопки "Показать ещё +N"
        /// </summary>
        public int CountOfDishesMain { get; set; } = 6;
        /// <summary>
        /// Количество блюд, отображаемых при выборе фильтра по категории до нажатия кнопки "Показать ещё +N"
        /// </summary>
        public int CountOfDishesFilter { get; set; } = 30;
    }
}