using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public class CityService : BaseClient<CityService>, ICityService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";
        private static string BaseUrl = ServiceUrl + "api/cities";
        public CityService() : base(ServiceUrl)
        {
        }

        [ActivatorUtilitiesConstructor]
        public CityService(IHttpContextAccessor contextAccessor) : base(ServiceUrl, contextAccessor)
        {
        }

        public CityService(string baseAddress) : base(baseAddress)
        {
        }

        public CityService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public CityService(bool debug) : base(debug)
        {
        }

        public virtual async Task<HttpResult<List<CityModel>>> GetCities(string searchString = null)
        {
            return await GetAsync<List<CityModel>>($"{BaseUrl}", searchString is null ? null : $"searchString={searchString}");
        }

        public virtual async Task<CityModel> GetDefaultCity()
        {
            var response = await GetAsync<CityModel>($"{BaseUrl}/GetDefaultCity");
            return response.Content;
        }

        public async Task<CityModel> GetCityById(long id)
        {
            var response = await GetAsync<CityModel>($"{BaseUrl}/GetCityById/{id}");
            return response.Content;
        }

        public async Task<List<CityModel>> GetActiveCities(string searchString = null)
        {
            var response = await GetAsync<List<CityModel>>($"{BaseUrl}/active", searchString is null ? null : $"searchString={searchString}");
            return response.Content ?? new List<CityModel>();
        }

        public async Task<CityModel> GetCity(string name, string region = null)
        {
            region = region is null ? "" : $"&region={region}";
            var response = await GetAsync<CityModel>($"{BaseUrl}/byname?name={name}{region}");
            return response.Content;
        }

        public async Task<List<RegionModel>> GetActiveCitiesForRegion(string searchString = null)
        {
            var response = await GetAsync<List<RegionModel>>($"{BaseUrl}/activeforregion", searchString is null ? null : $"searchString={searchString}");
            return response.Content ?? new List<RegionModel>();
        }
    }
}
