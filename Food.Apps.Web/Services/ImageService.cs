using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public class ImageService : BaseClient<ImageService>, IImageService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";
        private static string BaseUrl = ServiceUrl + "api/images";

        public ImageService() : base(ServiceUrl)
        {
        }

        public ImageService(string baseAddress) : base(baseAddress)
        {
        }

        public ImageService(bool debug) : base(debug)
        {
        }

        public ImageService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public virtual async Task<bool> AddImage(string hash, long cafeId, long objectId, ObjectTypesEnum type)
        {
            var httpResult = await PostAsync<bool>($"{BaseUrl}/{cafeId}/{objectId}/{(int)type}?hash={hash}", null);
            return httpResult.Content;
        }

        public virtual async Task<bool> RemoveImage(string hash, long cafeId, long objectId, ObjectTypesEnum type)
        {
            var httpResult = await HttpClient.DeleteAsync($"{BaseUrl}/{cafeId}/{objectId}/{(int)type}?hash={hash}");
            return httpResult.IsSuccessStatusCode;
        }
    }
}