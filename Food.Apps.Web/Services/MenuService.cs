using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodServiceManager;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public class MenuService : BaseClient<MenuService>, IMenuService
    {
        private static readonly string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static readonly string BaseUrl = ServiceUrl + "api/menu";

        public MenuService() : base(ServiceUrl)
        {
        }

        [ActivatorUtilitiesConstructor]
        public MenuService(IHttpContextAccessor contextAccessor) : base(ServiceUrl, contextAccessor)
        {
        }

        public MenuService(string baseAddress) : base(baseAddress)
        {
        }

        public MenuService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public MenuService(bool debug) : base(debug)
        {
        }

        public virtual async Task<long> AddSchedule(long dishId, ScheduleTypeEnum scheduleType, DateTime? beginDate, DateTime? endDate,
            DateTime? oneDay, string monthDays, string weekDays, double? price)
        {
            var beginDatePath = beginDate != null ? $"{DateTimeExtensions.ConvertToUnixTimestamp(beginDate.Value)}" : "";
            var endDatePath = endDate != null ? $"{DateTimeExtensions.ConvertToUnixTimestamp(endDate.Value)}" : "";
            var oneDayPath = oneDay != null ? $"{DateTimeExtensions.ConvertToUnixTimestamp(oneDay.Value)}" : "";
            var pricePath = price != null ? $"{price}" : "";
            pricePath = pricePath.Replace(",", ".");
            var response = await GetAsync<long>($"{BaseUrl}/schedule/add?dishId={dishId}&scheduleType={(int)scheduleType}&beginDate={beginDatePath}&endDate={endDatePath}&oneDay={oneDayPath}&monthDays={monthDays}&weekDays={weekDays}&price={pricePath}");
            return response.Content;
        }

        public virtual async Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetDishesByScheduleByDate(long cafeId, DateTime? date)
        {
            var beginDatePath = date != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(date.Value)}" : "";

            var response = await GetAsync<List<FoodCategoryWithDishes>>($"{BaseUrl}/dishesbyschedule/cafeid/{cafeId}{beginDatePath}");

            return await FoodCategoryExtensions.GetDicFoodCategoryWithDishes(response.Content);
        }

        public virtual async Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetDishesForManager(long cafeId, DateTime? date)
        {
            var beginDatePath = date != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(date ?? DateTime.MinValue)}" : "";
            var response = await GetAsync<List<FoodCategoryWithDishes>>($"{BaseUrl}/dishesformanager/cafeid/{cafeId}{beginDatePath}");
            return await FoodCategoryExtensions.GetDicFoodCategoryWithDishes(response.Content);
        }

        public virtual async Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetMenuForManager(long cafeId, DateTime? date)
        {
            var beginDatePath = date != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(date ?? DateTime.MinValue)}" : "";
            var response = await GetAsync<List<FoodCategoryWithDishes>>($"{BaseUrl}/menuformanager/cafeid/{cafeId}{beginDatePath}");
            return await FoodCategoryExtensions.GetDicFoodCategoryWithDishes(response.Content);
        }

        public virtual async Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetMenuSchedules(long cafeId, DateTime? date)
        {
            var beginDatePath = date != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(date ?? DateTime.MinValue)}" : null;
            var response = await GetAsync<List<FoodCategoryWithDishes>>($"{BaseUrl}/menuschedule/cafeid/{cafeId}{beginDatePath}");
            return await FoodCategoryExtensions.GetDicFoodCategoryWithDishes(response.Content);
        }

        public virtual async Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetDishesByScheduleByDateByRange(long cafeId,
            long categoryId, DateTime? date, int startRange, int count)
        {
            var datePath = date != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(date.Value)}" : "";
            var response = await GetAsync<List<FoodCategoryWithDishes>>($"{BaseUrl}/getDishesByScheduleByDateByRange/cafe/{cafeId}/category/{categoryId}{datePath}/start/{startRange}/count/{count}");
            return await FoodCategoryExtensions.GetDicFoodCategoryWithDishes(response.Content);
        }

        public virtual async Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetDishesByScheduleByDateByTagListAndSearchTerm(
            long cafeId, DateTime? date, string searchTerm, List<long> tagIds)
        {
            var datePath = date != null ? $"{DateTimeExtensions.ConvertToUnixTimestamp(date.Value)}" : "";
            var response = await PostAsync<List<FoodCategoryWithDishes>>($"{BaseUrl}/getDishesByScheduleByDateByTagListAndSearchTerm?cafeId={cafeId}&date={datePath}&searchTerm={searchTerm}", tagIds);
            return await FoodCategoryExtensions.GetDicFoodCategoryWithDishes(response.Content);
        }

        public virtual async Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetMenuByPatternId(long cafeId, long id)
        {
            var response = await GetAsync<List<FoodCategoryWithDishes>>($"{BaseUrl}/menubypattern/cafe/{id}/id/{id}");
            return await FoodCategoryExtensions.GetDicFoodCategoryWithDishes(response.Content);
        }

        /// <summary>
        /// Список блюд, сгруппированных по категориям, с 6 блюдами в каждой категории
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<FoodCategoryWithDishes>> GetDishesForDate(
            string name = null, int? countDishes = 6, string filter = null, long? dishCatId = null, long? companyId = null, long? cityId = null)
        {
            string dishCatParam = dishCatId.HasValue ? $"&dishCatId={dishCatId.Value.ToString()}" : string.Empty;
            var response = await GetAsync<List<FoodCategoryWithDishes>>($"{BaseUrl}/fordate?name={name}&countDishes={countDishes}&filter={filter}{dishCatParam}&companyId={companyId}&cityId={cityId}");
            if (response.Succeeded)
                return response.Content;
            else
                return null;
        }


        /// <summary>
        /// Метод для получения остальных, кроме 6 случайных блюд, из указанной категории.
        /// Нужен для отображения остатка блюд по нажатию кнопки
        /// "Показать ещё +N"
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<FoodDishModel>> GetRestOfCat(RestOfCategory rest)
        {
            var response = await PostAsync<List<FoodDishModel>>($"{BaseUrl}/other6", rest);
            if (response.Succeeded)
                return response.Content;
            else
                return null;
        }


        public virtual async Task<ScheduleModel> GetScheduleById(long scheduleId)
        {
            var response = await GetAsync<ScheduleModel>($"{BaseUrl}/schedule/{scheduleId}");
            return response.Content;
        }

        public virtual async Task<List<ScheduleModel>> GetScheduleForCafeByCafeId(long cafeId, DateTime? date)
        {
            var datePath = date != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(date.Value)}" : "";
            var response = await GetAsync<List<ScheduleModel>>($"{BaseUrl}/schedullebycafe/{cafeId}{datePath}");
            return response.Content;
        }

        public virtual async Task<List<ScheduleModel>> GetScheduleForDish(long dishId, DateTime? date)
        {
            var datePath = date != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(date.Value)}" : "";
            var response = await GetAsync<List<ScheduleModel>>($"{BaseUrl}/schedullefordish/{dishId}{datePath}");
            return response.Content;
        }

        //больше не используется
        public virtual async Task<List<ScheduleModel>> GetSchedulesForCafeByDishList(long cafeId, long[] dishList, bool onlyActive = true)
        {
            var response = await PostAsync<List<ScheduleModel>>($"{BaseUrl}/getSchedulesForCafeByDishList?cafeId={cafeId}&onlyActive={onlyActive}", dishList);
            return response.Content;
        }
        public virtual async Task<List<PageDishInMenuHistoryCategoryModel>> GetDishesInMenuHistoryForCafe(long cafeId, string dishName)
        {
            var response = await
                GetAsync<List<PageDishInMenuHistoryCategoryModel>>($"{BaseUrl}/getDishesInMenuHistoryForCafe?cafeId={cafeId}&dishName={dishName}");
            return response.Content;
        }

        public virtual async Task<bool> RemoveCafeMenuPattern(long cafeId, long id)
        {
            var response = await DeleteAsync<bool>($"{BaseUrl}/RemoveCafeMenuPattern/{id}/cafe/{cafeId}");
            return response.Content;
        }

        public virtual async Task<long> RemoveSchedule(long scheduleId)
        {
            var response = await DeleteAsync<long>($"{BaseUrl}/removeschedule/{scheduleId}");
            return response.Content;
        }

        public virtual async Task<long> UpdateSchedule(ScheduleModel schedule)
        {
            var response = await PostAsync<long>($"{BaseUrl}/updateschedule", schedule);
            return response.Content;
        }

        public virtual async Task<bool> UpdateSchedulesByPatternId(long cafeId, long patternId, DateTime? requestDate)
        {
            var datePath = requestDate != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(requestDate.Value)}" : "";
            var response = await GetAsync<bool>($"{BaseUrl}/updateSchedulesByPatternId/cafe/{cafeId}/pattern/{patternId}{datePath}");
            return response.Content;
        }
    }
}