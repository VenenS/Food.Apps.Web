using ITWebNet.Food.Core.DataContracts.Manager;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public class DiscountService : BaseClient<DiscountService>, IDiscountService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static string BaseUrl = ServiceUrl + "api/discounts";

        public DiscountService() : base(ServiceUrl)
        {
        }

        public DiscountService(string baseAddress) : base(baseAddress)
        {
        }

        public DiscountService(bool debug) : base(debug)
        {
        }

        public DiscountService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public virtual async Task<long> AddDiscount(DiscountModel model)
        {
            var httpResult = await PostAsync<long>(BaseUrl, model);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return -1;
        }

        public virtual async Task<bool> EditDiscount(DiscountModel model)
        {
            var httpResult = await PutAsync<bool>(BaseUrl, model);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return false;
        }

        public virtual async Task<int> GetDiscountAmount(long cafeId, DateTime date, long? companyID)
        {
            var httpResult = await GetAsync<int>($"{BaseUrl}/amount?cafeId={cafeId}&date={date}&companyID={companyID}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return 0;
        }

        public virtual async Task<List<DiscountModel>> GetDiscounts(long[] discountIdList)
        {
            var httpResult = await PostAsync<List<DiscountModel>>($"{BaseUrl}/GetDiscounts", discountIdList);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<DiscountModel>> GetUserDiscounts(long userId)
        {
            var httpResult = await GetAsync<List<DiscountModel>>($"{BaseUrl}/user/{userId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<bool> RemoveDiscount(long discountId)
        {
            var httpResult = await DeleteAsync<bool>($"{BaseUrl}/{discountId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return false;
        }

    }
}