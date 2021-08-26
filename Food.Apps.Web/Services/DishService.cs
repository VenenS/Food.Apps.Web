using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using Food.Services.Contracts.Companies;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ITWebNet.Food.Site.Services
{
    public class DishService : BaseClient<DishService>, IDishService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";
        private const string Prefix = "api/dishes";
        public static string filterUrl = ServiceUrl + Prefix + "/filter";

        public DishService() : base(ServiceUrl)
        {
        }

        [ActivatorUtilitiesConstructor]
        public DishService(IHttpContextAccessor contextAccessor) : base(ServiceUrl, contextAccessor)
        {
        }

        public DishService(string baseAddress) : base(baseAddress)
        {
        }

        public DishService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public DishService(bool debug) : base(debug)
        {
        }

        public virtual async Task<HttpResult<List<FoodDishModel>>> GetDishesByFilter(DishFilterModel filter)
        {
            return await PostAsync<List<FoodDishModel>>(filterUrl, filter);
        }

        public virtual async Task<long> CreateDish(long cafeId, FoodDishModel dish)
        {
            var response = await PostAsync<long>($"{Prefix}?cafeId={cafeId}", dish);
            return response.Succeeded ? response.Content : -1;
        }

        public virtual async Task<bool> ChangeDishIndex(long cafeId, long categoryId, int newIndex, int oldIndex, int? dishId = null)
        {
            var response = await GetAsync<bool>(
                $"{Prefix}/ChangeDishIndex?cafeId={cafeId}&categoryId={categoryId}&newIndex={newIndex}&oldIndex={oldIndex}"
                + (dishId.HasValue ? $"&dishId={dishId}" : ""));
            return response.Succeeded;
        }

        /// <summary>
        /// Проверяет уникальное ли имя блюда в рамках кафе
        /// </summary>
        public virtual async Task<bool> CheckUniqueNameWithinCafe(string dishName, long cafeId, long dishId = -1)
        {
            var response = await GetAsync<bool>($"{Prefix}/CheckUniqueNameWithinCafe?dishName={dishName}&cafeId={cafeId}&dishId={dishId}");
            return response.Succeeded == true ? response.Content : false;
        }

        public virtual async Task<long> UpdateDish(FoodDishModel dish)
        {
            var response = await PutAsync<long>($"{Prefix}", dish);
            return response.Succeeded ? response.Content : -1;
        }

        public virtual async Task<List<FoodDishModel>> GetFoodDishesByFilter(DishFilterModel filter)
        {
            var response = await PostAsync<List<FoodDishModel>>($"{Prefix}/ChangeDishIndex/filter", filter);
            return response.Content;
        }

        public virtual async Task<List<FoodDishModel>> GetDishesBySearchTermAndTagListAndCafeId(string searchTerm, List<long> tagIds, long cafeId)
        {
            var response = await PostAsync<List<FoodDishModel>>($"{Prefix}/GetDishesBySearchTermAndTagListAndCafeId?searchTerm={searchTerm}&cafeId={cafeId}", tagIds);
            return response.Content;
        }

        public virtual async Task<List<OrderModel>> GetFirst100Dishes()
        {
            var response = await GetAsync<List<OrderModel>>($"{Prefix}/GetFirst100Dishes");
            return response.Content;
        }

        public virtual async Task<CafeModel> GetFoodCafeByDishId(long id)
        {
            var response = await GetAsync<CafeModel>($"{Prefix}/GetFoodCafeByDishId?id={id}");
            return response.Content;
        }

        public virtual async Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetFoodCategoryAndFoodDishesByCafeIdAndDate(long cafeId, DateTime? wantedDate)
        {
            var date = wantedDate != null ? $"&wantedDate={DateTimeExtensions.ConvertToUnixTimestamp(wantedDate.Value)}" : "";
            var response = await GetAsync<List<FoodCategoryWithDishes>>($"{Prefix}/GetFoodCategoryAndFoodDishesByCafeIdAndDate?cafeId={cafeId}{date}");
            return await FoodCategoryExtensions.GetDicFoodCategoryWithDishes(response.Content);
        }

        public virtual async Task<FoodCategoryModel> GetFoodCategoryByDishId(long id)
        {
            var response = await GetAsync<FoodCategoryModel>($"{Prefix}/GetFoodCategoryByDishId?id={id}");
            return response.Content;
        }

        public virtual async Task<List<FoodDishVersionModel>> GetFoodDishAllVersion(long dishId)
        {
            var response = await GetAsync<List<FoodDishVersionModel>>($"{Prefix}/GetFoodDishAllVersion?dishId={dishId}");
            return response.Content;
        }

        public virtual async Task<FoodDishModel> GetFoodDishById(long dishId)
        {
            var response = await GetAsync<FoodDishModel>($"{Prefix}?dishId={dishId}");
            return response.Content;
        }

        public virtual async Task<FoodDishModel> GetFoodDishByIdAndDate(long dishId, DateTime date)
        {
            var response = await GetAsync<FoodDishModel>($"{Prefix}/GetFoodDishByIdAndDate?dishId={dishId}&date={DateTimeExtensions.ConvertToUnixTimestamp(date)}");
            return response.Content;
        }

        public virtual async Task<List<FoodDishModel>> GetFoodDishesByCafeId(long cafeId, DateTime? date)
        {
            var datePath = date != null ? $"&date={DateTimeExtensions.ConvertToUnixTimestamp(date.Value)}" : "";
            var response = await GetAsync<List<FoodDishModel>>($"{Prefix}/GetFoodDishesByCafeId?cafeId={cafeId}{datePath}");
            return response.Content;
        }

        public virtual async Task<List<FoodDishModel>> GetFoodDishesByCategoryIdAndCafeId(long cafeId, long categoryId, DateTime? date)
        {
            var datePath = date != null ? "&date=" + DateTimeExtensions.ConvertToUnixTimestamp(date.Value) : "";
            var response = await GetAsync<List<FoodDishModel>>($"{Prefix}/GetFoodDishesByCategoryIdAndCafeId?cafeId={cafeId}&categoryId={categoryId}{datePath}");
            return response.Content;
        }

        public virtual async Task<long> DeleteDish(long dishId, long? categoryId = null)
        {
            var catStr = categoryId == null ? "" : $"&categoryId={categoryId.Value}";
            var response = await DeleteAsync<long>($"{Prefix}?dishId={dishId}{catStr}");
            return response.Content;
        }

        public virtual async Task<bool> UpdateDishIndexInFirstCategory(long cafeId, long newCategoryId, long oldCategoryId, int newIndex, int oldIndex, long dishId)
        {
            var response = await PutAsync<bool>($"{Prefix}/UpdateDishIndexInFirstCategory/{cafeId}/{newCategoryId}/{oldCategoryId}/{newIndex}/{oldIndex}/{dishId}", null);
            return response.Succeeded;
        }

        public virtual async Task<bool> UpdateDishIndexInSecondCategory(long cafeId, long categoryId, int newIndex, long dishId)
        {
            var response = await PutAsync<bool>($"{Prefix}/UpdateDishIndexInSecondCategory/{cafeId}/{categoryId}/{newIndex}/{dishId}", null);
            return response.Succeeded;
        }
    }
}