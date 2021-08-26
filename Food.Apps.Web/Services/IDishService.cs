using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Site.Services
{
    public interface IDishService
    {
        Task<bool> ChangeDishIndex(long cafeId, long categoryId, int newIndex, int oldIndex, int? dishId = null);
        Task<bool> CheckUniqueNameWithinCafe(string dishName, long cafeId, long dishId = -1);
        Task<long> CreateDish(long cafeId, FoodDishModel dish);
        Task<long> DeleteDish(long dishId, long? categoryId = null);
        Task<HttpResult<List<FoodDishModel>>> GetDishesByFilter(DishFilterModel filter);
        Task<List<FoodDishModel>> GetDishesBySearchTermAndTagListAndCafeId(string searchTerm, List<long> tagIds, long cafeId);
        Task<List<OrderModel>> GetFirst100Dishes();
        Task<CafeModel> GetFoodCafeByDishId(long id);
        Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetFoodCategoryAndFoodDishesByCafeIdAndDate(long cafeId, DateTime? wantedDate);
        Task<FoodCategoryModel> GetFoodCategoryByDishId(long id);
        Task<List<FoodDishVersionModel>> GetFoodDishAllVersion(long dishId);
        Task<FoodDishModel> GetFoodDishById(long dishId);
        Task<FoodDishModel> GetFoodDishByIdAndDate(long dishId, DateTime date);
        Task<List<FoodDishModel>> GetFoodDishesByCafeId(long cafeId, DateTime? date);
        Task<List<FoodDishModel>> GetFoodDishesByCategoryIdAndCafeId(long cafeId, long categoryId, DateTime? date);
        Task<List<FoodDishModel>> GetFoodDishesByFilter(DishFilterModel filter);
        Task<long> UpdateDish(FoodDishModel dish);
        Task<bool> UpdateDishIndexInFirstCategory(long cafeId, long newCategoryId, long oldCategoryId, int newIndex, int oldIndex, long dishId);
        Task<bool> UpdateDishIndexInSecondCategory(long cafeId, long categoryId, int newIndex, long dishId);
    }
}