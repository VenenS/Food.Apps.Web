using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ITWebNet.Food.Site.Services
{
    public class CategoriesService : BaseClient<CategoriesService>, ICategoriesService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";
        private const string Prefix = "api/categories";


        public CategoriesService() : base(ServiceUrl)
        {
        }

        public CategoriesService(string baseAddress) : base(baseAddress)
        {
        }

        [ActivatorUtilitiesConstructor]
        public CategoriesService(IHttpContextAccessor contextAccessor) : base(ServiceUrl, contextAccessor)
        {
        }

        public CategoriesService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public CategoriesService(bool debug) : base(debug)
        {
        }

        public virtual async Task<HttpResult<List<FoodCategoryModel>>> GetCategories()
        {
            return await GetAsync<List<FoodCategoryModel>>(Prefix);
        }

        public virtual async Task<HttpResult<FoodCategoryModel>> GetCategoryById(long id)
        {
            return await GetAsync<FoodCategoryModel>($"{Prefix}/{id}");
        }

        public virtual async Task<HttpResult<FoodCategoryModel>> CreateCategory(FoodCategoryModel company)
        {
            return await PostAsync<FoodCategoryModel>(Prefix, company);
        }

        public virtual async Task<HttpResult<FoodCategoryModel>> UpdateCategory(FoodCategoryModel model)
        {
            return await PutAsync<FoodCategoryModel>($"{Prefix}", model);
        }

        public virtual async Task<bool> DeleteCategory(long id)
        {
            var response = await HttpClient.DeleteAsync($"{Prefix}/{id}");
            return response.IsSuccessStatusCode;
        }

        public virtual async Task<HttpResult<bool>> SetCategoryBusinessHours(CategoryBusinessHoursModel categoryBusinessHours)
        {
            return await PostAsync<bool>($"{Prefix}/SetCategoryBusinessHours", categoryBusinessHours);
        }

        public virtual async Task<CategoryBusinessHoursModel> GetCategoryBusinessHours(long cafeId, long categoryId)
        {
            var httpResult = await GetAsync<CategoryBusinessHoursModel>($"{Prefix}/GetCategoryBusinessHours?cafeId={cafeId}&categoryId={categoryId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }
    }
}