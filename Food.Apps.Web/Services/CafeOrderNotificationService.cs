using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Food.Services.Models;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.Extensions.Configuration;

namespace ITWebNet.Food.Site.Services
{
    public class CafeOrderNotificationService : BaseClient<CafeService>, ICafeOrderNotificationService
    {
        private static readonly string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static readonly string BaseUrl = ServiceUrl + "api/notifications";

        public CafeOrderNotificationService() : base(ServiceUrl)
        {
        }

        public CafeOrderNotificationService(string baseAddress) : base(baseAddress)
        {
        }

        public CafeOrderNotificationService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public CafeOrderNotificationService(bool debug) : base(debug)
        {
        }

        /// <summary>
        ///     Get caffee id with new unwatched orders for user
        /// </summary>
        /// <param name="userId">Manager of caffee id</param>
        /// <returns></returns>
        public virtual async Task<long> GetNotificationForUser(long userId)
        {
            var result = await GetAsync<long>($"{BaseUrl}/{userId}");
            return result.Succeeded ? result.Content : -1;
        }

        /// <summary>
        ///     User doesn't want to be notified about orders that already done
        /// </summary>
        /// <param name="userId">Identifier of a cafe manager</param>
        public virtual async Task<bool> StopNotifyUser(long userId)
        {
            var result = await PostAsync<bool>($"{BaseUrl}/stop/{userId}", null);
            return result.Succeeded && result.Content;
        }

        /// <summary>
        ///     One of the managers had already watched the orders of the caffee, remove unneccessary notifications
        /// </summary>
        /// <param name="cafeId">Caffee identifier</param>
        public virtual async Task<bool> SetOrdersViewed(long cafeId)
        {
            var result = await PostAsync<bool>($"{BaseUrl}/viewed/{cafeId}", null);
            return result.Succeeded && result.Content;
        }

        /// <summary>
        ///     New caffe order was created, all managers should know about that!
        /// </summary>
        /// <param name="cafeId">Caffee identifier</param>
        public virtual async Task<List<string>> NewOrder(long cafeId, DateTime deliveryDate)
        {
            var unixTime = DateTimeExtensions.ConvertToUnixTimestamp(deliveryDate.Date);
            var result = await PostAsync<List<string>>($"{BaseUrl}/ordered/{cafeId}/deliveryDate/{unixTime}", null);
            if (result.Succeeded)
                return result.Content;
            return new List<string>();
        }

        /// <summary>
        /// Отправить смс код на номер телефона
        /// </summary>
        /// <param name="phone">Телефон</param>
        public virtual async Task<ResponseModel> SendSmsCode(string phone, bool isConfirming = false)
        {
            var httpResponse = await GetAsync<ResponseModel>($"{BaseUrl}/SendSmsCode?phone={phone}&isConfirming={isConfirming}");
            if (httpResponse.Succeeded)
            {
                return httpResponse.Content;
            }
            else
                return new ResponseModel() { Message = "Не удалось отправить смс код", Status = 1 };
        }
    }
}