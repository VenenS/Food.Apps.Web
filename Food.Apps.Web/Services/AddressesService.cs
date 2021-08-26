using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public class AddressesService : BaseClient<AddressesService>, IAddressesService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";
        private static string BaseUrl = ServiceUrl + "api/addresses";

        public AddressesService() : base(ServiceUrl)
        {
        }

        [ActivatorUtilitiesConstructor]
        public AddressesService(IHttpContextAccessor contextAccessor) : base(ServiceUrl, contextAccessor)
        {
        }

        public AddressesService(string baseAddress) : base(baseAddress)
        {
        }

        public AddressesService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public AddressesService(bool debug) : base(debug)
        {
        }

        public virtual async Task<long?> AddAddress(DeliveryAddressModel model)
        {
            var httpResult = await PostAsync<long>(BaseUrl, model);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<DeliveryAddressModel> GetAddressById(long? id)
        {
            var httpResult = await GetAsync<DeliveryAddressModel>($"{BaseUrl}/{id}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<HttpResult<List<DeliveryAddressModel>>> GetAddresses()
        {
            return await GetAsync<List<DeliveryAddressModel>>($"{BaseUrl}");
        }

        public virtual async Task<List<DeliveryAddressModel>> GetCompanyAddresses(long companyId)
        {
            var httpResult = await GetAsync<List<DeliveryAddressModel>>($"{BaseUrl}/company/{companyId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<HttpResult<DeliveryAddressModel>> CreateAddress(DeliveryAddressModel model)
        {
            return await PostAsync<DeliveryAddressModel>($"{BaseUrl}", model);
        }

        public virtual async Task<bool> DeleteAddress(long id)
        {
            var response = await HttpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }

        public virtual async Task<DeliveryAddressModel> UpdateAddress(DeliveryAddressModel model)
        {
            var httpResult = await PutAsync<DeliveryAddressModel>($"{BaseUrl}", model);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }
    }
}