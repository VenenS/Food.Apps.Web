using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.Extensions.Configuration;

namespace ITWebNet.Food.Site.Services
{
    public class RatingService : BaseClient<RatingService>, IRatingService
    {
        private static readonly string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static readonly string BaseUrl = ServiceUrl + "api/rating";

        public RatingService() : base(ServiceUrl)
        {
        }

        public RatingService(string baseAddress) : base(baseAddress)
        {
        }

        public RatingService(bool debug) : base(debug)
        {
        }

        public RatingService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public virtual async Task<List<RatingModel>> GetAllRatingFromUser(bool isFilter, ObjectTypesEnum typeOfObject)
        {
            var response =
                await GetAsync<List<RatingModel>>(
                    $"{BaseUrl}/allratingsfromuser/isfilter/{isFilter}/type/{(int)typeOfObject}");
            return response.Content;
        }

        public virtual async Task<List<RatingModel>> GetAllRatingToObject(long objectId, ObjectTypesEnum objectType)
        {
            var response =
                await GetAsync<List<RatingModel>>($"{BaseUrl}/allratingstoobject/{objectId}/{(int)objectType}");
            return response.Content;
        }

        public virtual async Task<double> GetFinalRateToCafe(long cafeId)
        {
            var response = await GetAsync<double>($"{BaseUrl}/finalrate/cafe/{cafeId}");
            return response.Content;
        }

        public virtual async Task<double> GetFinalRateToDish(long dishId)
        {
            var response = await GetAsync<double>($"{BaseUrl}/finalrate/dish/{dishId}");
            return response.Content;
        }

        public virtual async Task<long> InsertNewRating(long objectId, ObjectTypesEnum typeOfObject, int valueOfRating)
        {
            var response =
                await PostAsync<long>($"{BaseUrl}/insertrating/{objectId}/{(int)typeOfObject}/{valueOfRating}", null);
            return response.Content;
        }
    }
}