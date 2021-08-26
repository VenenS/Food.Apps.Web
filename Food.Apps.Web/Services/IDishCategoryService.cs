using System.Collections.Generic;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Site.Services
{
    public interface IDishCategoryService
    {
        Task<long> AddCafeFoodCategory(long cafeId, long categoryId, int categoryIndex);
        Task ChangeFoodCategoryOrder(long cafeId, long categoryId, long categoryOrder);
        Task<List<FoodCategoryModel>> GetFoodCategories();
        Task<List<FoodCategoryModel>> GetFoodCategoriesByCafeId(long cafeId);
        Task<FoodCategoryModel> GetFoodCategoryById(long categoryId);
        Task RemoveCafeFoodCategory(long cafeId, long categoryId);
        Task<List<SelectableFoodCategoryModel>> GetSelectableCategoriesByCafeId(long cafeId);
        Task<List<SelectableFoodCategoryModel>> GetSelectableCategoriesByDishId(long dishId);
    }
}