using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodServiceManager;

namespace ITWebNet.Food.Site.Services
{
    public interface IMenuService
    {
        Task<long> AddSchedule(long dishId, ScheduleTypeEnum scheduleType, DateTime? beginDate, DateTime? endDate, DateTime? oneDay, string monthDays, string weekDays, double? price);
        Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetDishesByScheduleByDate(long cafeId, DateTime? date);
        Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetDishesByScheduleByDateByRange(long cafeId, long categoryId, DateTime? date, int startRange, int count);
        Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetDishesByScheduleByDateByTagListAndSearchTerm(long cafeId, DateTime? date, string searchTerm, List<long> tagIds);
        Task<List<FoodCategoryWithDishes>> GetDishesForDate(string name = null, int? countDishes = 6, string filter = null, long? dishCatId = null, long? companyId = null, long? cityId = null);
        Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetDishesForManager(long cafeId, DateTime? date);
        Task<List<PageDishInMenuHistoryCategoryModel>> GetDishesInMenuHistoryForCafe(long cafeId, string dishName);
        Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetMenuByPatternId(long cafeId, long id);
        Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetMenuForManager(long cafeId, DateTime? date);
        Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetMenuSchedules(long cafeId, DateTime? date);
        Task<List<FoodDishModel>> GetRestOfCat(RestOfCategory rest);
        Task<ScheduleModel> GetScheduleById(long scheduleId);
        Task<List<ScheduleModel>> GetScheduleForCafeByCafeId(long cafeId, DateTime? date);
        Task<List<ScheduleModel>> GetScheduleForDish(long dishId, DateTime? date);
        Task<List<ScheduleModel>> GetSchedulesForCafeByDishList(long cafeId, long[] dishList, bool onlyActive = true);
        Task<bool> RemoveCafeMenuPattern(long cafeId, long id);
        Task<long> RemoveSchedule(long scheduleId);
        Task<long> UpdateSchedule(ScheduleModel schedule);
        Task<bool> UpdateSchedulesByPatternId(long cafeId, long patternId, DateTime? requestDate);
    }
}