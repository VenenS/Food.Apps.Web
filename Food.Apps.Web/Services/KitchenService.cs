using ITWebNet.Food.Core.DataContracts.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public class KitchenService : BaseClient<KitchenService>, IKitchenService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";
        private const string Prefix = "api/kitchens";

        [ActivatorUtilitiesConstructor]
        public KitchenService(IHttpContextAccessor contextAccessor) : base(ServiceUrl, contextAccessor)
        {
        }

        public KitchenService() : base(ServiceUrl)
        {
        }

        public KitchenService(string baseAddress) : base(baseAddress)
        {
        }

        public KitchenService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public KitchenService(bool debug) : base(debug)
        {
        }

        public virtual async Task<bool> AddKitchenToCafe(Int64 cafeId, Int64 kitchenId)
        {
            var response = await PostAsync<long>($"{Prefix}/cafe/{cafeId}/kitchen/{kitchenId}", null);
            return response.Succeeded;
        }

        public virtual async Task<KitchenModel[]> GetCurrentListOfKitchenToCafe(Int64 cafeId)
        {
            var response = await GetAsync<KitchenModel[]>($"{Prefix}/{cafeId}");
            return response.Content;
        }

        public virtual async Task<KitchenModel[]> GetFullListOfKitchen()
        {
            var response = await GetAsync<KitchenModel[]>($"{Prefix}");
            return response.Content;
        }

        public virtual async Task<bool> RemoveKitchenFromCafe(Int64 cafeId, Int64 kitchenId)
        {
            var response = await DeleteAsync<bool>($"{Prefix}/cafe/{cafeId}/kitchen/{kitchenId}");
            return response.Succeeded;
        }
    }
}