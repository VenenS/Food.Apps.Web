using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.Extensions.Configuration;

namespace ITWebNet.Food.Site.Services
{
    public class BanketsService : BaseClient<BanketsService>, IBanketsService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static string BaseUrl = ServiceUrl + "api/bankets";

        public BanketsService() : base(ServiceUrl)
        {
        }

        public BanketsService(string baseAddress) : base(baseAddress)
        {
        }

        public BanketsService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public BanketsService(bool debug) : base(debug)
        {
        }

        public virtual async Task<HttpResult<BanketModel>> CreateBanket(BanketModel model)
        {
            return await PostAsync<BanketModel>($"{BaseUrl}", model);
        }

        public virtual async Task<HttpResult<BanketModel>> GetBanketById(long banketId)
        {
            return await GetAsync<BanketModel>($"{BaseUrl}/{banketId}");
        }

        public virtual async Task<HttpResponseMessage> DeleteBanket(long banketId)
        {
            var result = await HttpClient.DeleteAsync($"{BaseUrl}/{banketId}");
            return result;
        }

        public virtual async Task<HttpResult<bool>> UpdateBanket(BanketModel model)
        {
            return await PutAsync<bool>($"{BaseUrl}", model);
        }

        public virtual async Task<HttpResult<OrderModel>> GetOrderByBanketId(long banketId, long userId)
        {
            return await GetAsync<OrderModel>($"{BaseUrl}/{banketId}/user/{userId}");
        }

        public virtual async Task<HttpResult> PostOrders(OrderModel order)
        {
            var response = await PostAsync<string>($"{BaseUrl}/order", order);
            return response;
        }

        public virtual async Task<HttpResult> UpdateOrder(OrderModel order)
        {
            var response = await PutAsync<string>($"{BaseUrl}/order", order);
            return response;
        }

        public virtual async Task<List<BanketModel>> GetBanketsByFilter(BanketsFilterModel model)
        {
            var response = await PostAsync<List<BanketModel>>($"{BaseUrl}/filter", model);
            if (response.Succeeded)
                return response.Content;

            return new List<BanketModel>();
        }

        public virtual async Task<List<OrderModel>> GetOrdersInBanket(long banketId)
        {
            var response = await GetAsync<List<OrderModel>>($"{BaseUrl}/{banketId}/orders/");
            if (response.Succeeded)
                return response.Content;

            return new List<OrderModel>();
        }

        public virtual async Task<bool> DeleteOrderItem(long orderItemId)
        {
            var response = await HttpClient.DeleteAsync($"{BaseUrl}/orderitems/{orderItemId}");
            return response.IsSuccessStatusCode;
        }
    }
}