using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface ICityService
    {
        Task<HttpResult<List<CityModel>>> GetCities(string searchString = null);

        Task<CityModel> GetDefaultCity();

        Task<CityModel> GetCityById(long id);

        Task<List<CityModel>> GetActiveCities(string searchString = null);

        /// <summary>
        /// Возвращает город по имени и региону
        /// </summary>
        /// <param name="name"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        Task<CityModel> GetCity(string name, string region = null);

        Task<List<RegionModel>> GetActiveCitiesForRegion(string searchString = null);
    }
}
