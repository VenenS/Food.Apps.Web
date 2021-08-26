using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ITWebNet.Food.Site.Services
{
    public class DishCategoryService : BaseClient<DishCategoryService>, IDishCategoryService
    {
        private static string ServiceUrl =
           FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";
        private static string BaseUrl = ServiceUrl + "api/categories";

        public DishCategoryService() : base(ServiceUrl)
        {
        }

        [ActivatorUtilitiesConstructor]
        public DishCategoryService(IHttpContextAccessor contextAccessor) : base(ServiceUrl, contextAccessor)
        {
        }

        public DishCategoryService(string baseAddress) : base(baseAddress)
        {
        }

        public DishCategoryService(bool debug) : base(debug)
        {
        }

        public DishCategoryService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public virtual async Task<long> AddCafeFoodCategory(long cafeId, long categoryId, int categoryIndex)
        {
            var httpResult = await GetAsync<long>($"{BaseUrl}/AddCafeFoodCategory?cafeId={cafeId}&categoryId={categoryId}&categoryIndex={categoryIndex}");

            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return 0;
        }

        public virtual async Task ChangeFoodCategoryOrder(long cafeId, long categoryId, long categoryOrder)
        {
            await GetAsync<long?>($"{BaseUrl}/ChangeFoodCategoryOrder?cafeId={cafeId}&categoryId={categoryId}&categoryOrder={categoryOrder}");
        }

        public virtual async Task<List<FoodCategoryModel>> GetFoodCategories()
        {
            var httpResult = await GetAsync<List<FoodCategoryModel>>($"{BaseUrl}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<FoodCategoryModel>> GetFoodCategoriesByCafeId(long cafeId)
        {
            var httpResult = await GetAsync<List<FoodCategoryModel>>($"{BaseUrl}?cafeId={cafeId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<FoodCategoryModel> GetFoodCategoryById(long categoryId)
        {
            var httpResult = await GetAsync<FoodCategoryModel>($"{BaseUrl}", value:categoryId);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<SelectableFoodCategoryModel>> GetSelectableCategoriesByCafeId(long cafeId)
        {
            var httpResult = await GetAsync<List<SelectableFoodCategoryModel>>($"{BaseUrl}/GetSelectableCategoriesByCafeId/{cafeId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<SelectableFoodCategoryModel>> GetSelectableCategoriesByDishId(long dishId)
        {
            var httpResult = await GetAsync<List<SelectableFoodCategoryModel>>($"{BaseUrl}/GetSelectableCategoriesByDishId/{dishId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task RemoveCafeFoodCategory(long cafeId, long categoryId)
        {
            var i = await GetAsync<bool>($"{BaseUrl}/RemoveCafeFoodCategory?cafeId={cafeId}&categoryId={categoryId}");
        }
    }
}