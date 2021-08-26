using System.Collections.Generic;
using System.Linq;
using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Site.Models
{
    public class MenuViewModel
    {
        public CafeModel Cafe { get; set; }
        public BanketModel Banket { get; set; }
        public bool IsCompanyEmployee { get; set; } = false;
        public Dictionary<FoodCategoryModel, List<FoodDishModel>> Menu { get; set; }
        public int ClientDiscount { get; set; }
        public List<FoodCategoryModel> StorageCategories { get; set; }

        public List<FoodCategoryModel> Categories
        {
            get { return Menu != null && Menu.Keys.Count > 0 ? 
                    Menu.Keys.ToList() : StorageCategories; }
        }

        public MenuViewModel()
        {
            Menu = new Dictionary<FoodCategoryModel, List<FoodDishModel>>();
        }
    }
}