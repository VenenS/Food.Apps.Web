using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
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
    public class CafeService : BaseClient<CafeService>, ICafeService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static string BaseUrl = ServiceUrl + "api/cafes";
        private static string MenuPatternsUrl = BaseUrl + "/menupatterns";

        public CafeService() : base(ServiceUrl) { }

        [ActivatorUtilitiesConstructor]
        public CafeService(IHttpContextAccessor contextAccessor) : base(ServiceUrl, contextAccessor)
        {
        }

        public CafeService(string baseAddress) : base(baseAddress) { }

        public CafeService(string baseAddress, string token) : base(baseAddress, token) { }

        public CafeService(bool debug) : base(debug) { }

        public virtual async Task<List<CafeModel>> GetCafesAsync()
        {
            var resposne = await GetAsync<List<CafeModel>>($"{BaseUrl}");
            return resposne.Succeeded ? resposne.Content : new List<CafeModel>();
        }

        //All undeleted caffes, including non-active
        public virtual async Task<List<CafeModel>> GetAllCafesAsync()
        {
            var resposne = await GetAsync<List<CafeModel>>($"{BaseUrl}/admin");
            return resposne.Succeeded ? resposne.Content : new List<CafeModel>();
        }

        public virtual async Task<CafeModel> GetCafeByCleanUrl(string cleanUrl)
        {
            var result = await GetAsync<CafeModel>($"{BaseUrl}/{cleanUrl}");
            return result.Succeeded ? result.Content : null;
        }

        public virtual async Task<CafeModel> GetCafeById(long id)
        {
            var result = await GetAsync<CafeModel>($"{BaseUrl}/{id}");
            return result.Succeeded ? result.Content : null;
        }

        public virtual async Task<List<CafeModel>> GetCafesToCurrentUserAsync()
        {
            var result = await GetAsync<List<CafeModel>>($"{BaseUrl}/GetCafesToCurrentUser");
            return result.Succeeded ? result.Content : new List<CafeModel>();
        }

        public virtual async Task<List<CafeModel>> GetManagedCaffee()
        {
            var result = await GetAsync<List<CafeModel>>($"{BaseUrl}/GetManagedCafes");
            return result.Succeeded ? result.Content : new List<CafeModel>();
        }

        public virtual async Task<List<CafeModel>> GetManagedCaffeeByMail(string email)
        {
            var result = await GetAsync<List<CafeModel>>($"{BaseUrl}/managedcaffeebymail?email={email}");
            return result.Succeeded ? result.Content : new List<CafeModel>();
        }

        public virtual async Task<List<CafeMenuPatternModel>> GetMenuPatternsByCafeIdAsync(long cafeId)
        {
            var result = await GetAsync<List<CafeMenuPatternModel>>($"{BaseUrl}/{cafeId}/menupatterns");
            if (result.Succeeded)
                return result.Content;

            return new List<CafeMenuPatternModel>();
        }

        public virtual async Task<HttpResponseMessage> RemoveCafeMenuPatternAsync(long cafeId, long patternId)
        {
            var response = await HttpClient.DeleteAsync($"{BaseUrl}/{cafeId}/menupatterns/{patternId}");
            return response;
        }

        public virtual async Task<HttpResult> AddCafeMenuPatternAsync(CafeMenuPatternModel pattern)
        {
            var result = await PostAsync<CafeMenuPatternModel>($"{MenuPatternsUrl}", pattern);
            return result;
        }

        public virtual async Task<HttpResult> UpdateCafeMenuPatternAsync(CafeMenuPatternModel pattern)
        {
            var result = await PutAsync<CafeMenuPatternModel>($"{MenuPatternsUrl}", pattern);
            return result;
        }

        public virtual async Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetMenuByPatternIdAsync(long cafeId, long id)
        {
            var result =
                await GetAsync<List<CafeMenuModel>>($"{BaseUrl}/{cafeId}/menu/{id}");
            return result.Succeeded
                ? result.Content.ToDictionary(c => c.Category, c => c.Dishes)
                : new Dictionary<FoodCategoryModel, List<FoodDishModel>>();
        }

        public virtual async Task<List<CompanyOrderScheduleModel>> GetCompanyOrderSchedulesByCafeId(long cafeId)
        {
            var result = await GetAsync<List<CompanyOrderScheduleModel>>($"{BaseUrl}/{cafeId}/schedules");
            return result.Content;
        }

        public virtual async Task<List<BanketModel>> GetBanketsByCafeId(long cafeId)
        {
            var result = await GetAsync<List<BanketModel>>($"{BaseUrl}/{cafeId}/bankets");
            return result.Succeeded ? result.Content : new List<BanketModel>();
        }

        public virtual async Task<OrderDeliveryPriceModel> GetCostOfDeliveryAsync(long cafeId, double price, bool corpOrder = false, string orderDate = "")
        {
            var nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = "."
            };
            var result = await GetAsync<OrderDeliveryPriceModel>($"{BaseUrl}/{cafeId}/cost/{price.ToString(nfi)}/{corpOrder}?orderDate={orderDate}");
            return result.Succeeded ? result.Content : new OrderDeliveryPriceModel() { ErrorDescription = $"Ошибка при выполнении запроса. Код:{result.StatusCode.ToString()}, cообщение: {result.Message}" };
        }

        public virtual async Task<OrderDeliveryPriceModel>
        GetShippingCostToSingleCompanyAddress(string address, long companyOrderId, double extraCash)
        {
            var url = $"{BaseUrl}/GetShippingCostToSingleCompanyAddress?"
                        + $"&address={Uri.EscapeDataString(address)}"
                        + $"&companyOrderId={companyOrderId}"
                        + $"&extraCash={extraCash}";
            var result = await GetAsync<OrderDeliveryPriceModel>(url);
            return result.Succeeded ? result.Content : new OrderDeliveryPriceModel { ErrorDescription = result.Message };
        }

        public virtual async Task<OrderDeliveryPriceModel>
        GetShippingCostToSingleCompanyAddress(string address, long companyId, long cafeId, double extraCash, DateTime date)
        {
            var url = $"{BaseUrl}/GetShippingCostToSingleCompanyAddress?companyId={companyId}"
                        + $"&address={Uri.EscapeDataString(address)}"
                        + $"&cafeId={cafeId}"
                        + $"&extraCash={extraCash.ToString(CultureInfo.InvariantCulture)}"
                        + $"&date={Uri.EscapeDataString(date.ToString("s", CultureInfo.InvariantCulture))}";
            var result = await GetAsync<OrderDeliveryPriceModel>(url);
            return result.Succeeded ? result.Content : new OrderDeliveryPriceModel { ErrorDescription = result.Message };
        }

        public virtual async Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetFilteredMenuByPatternId(long cafeId, long id,
            string search)
        {
            var result =
                await GetAsync<List<CafeMenuModel>>($"{BaseUrl}/{cafeId}/menu/{id}/{search}");
            return result.Succeeded
                ? result.Content.ToDictionary(c => c.Category, c => c.Dishes)
                : new Dictionary<FoodCategoryModel, List<FoodDishModel>>();
        }

        public virtual async Task<List<CafeManagerModel>> GetCafeManagers(long cafeId)
        {
            var response = await GetAsync<List<CafeManagerModel>>($"{BaseUrl}/{cafeId}/managers");
            return response.Succeeded ?
                response.Content
                : new List<CafeManagerModel>();
        }

        public virtual async Task<HttpResult<CafeManagerModel>> AddCafeManager(CafeManagerModel model)
        {
            return await PostAsync<CafeManagerModel>($"{BaseUrl}/managers/add", model);
        }

        public virtual async Task<bool> DeleteCafeManager(long cafeId, long userId)
        {
            var response = await HttpClient.DeleteAsync($"{BaseUrl}/{cafeId}/managers/delete/{userId}");
            return response.IsSuccessStatusCode;
        }

        public virtual async Task<bool> RemoveCafeAsync(long cafeId)
        {
            var response = await HttpClient.DeleteAsync($"{BaseUrl}/{cafeId}");
            return response.IsSuccessStatusCode;
        }

        public Task<HttpResult<CafeModel>> AddNewCafeAsync(CafeModel cafe)
        {
            return PostAsync<CafeModel>($"{BaseUrl}", cafe);
        }

        public virtual async Task<long> AddNewCafeNotificationContact(CafeNotificationContactModel cafeNotificationContact)
        {
            var httpResult = await PostAsync<long>($"{BaseUrl}/CafeNotificationContact", cafeNotificationContact);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return -1;
        }

        public virtual async Task<bool> AddUserCafeLink(long cafeId, long userId)
        {
            var httpResult = await PostAsync<bool>($"{BaseUrl}/UserCafeLink/{cafeId}/{userId}", null);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return false;
        }

        /// <summary>
        /// Проверяет уникальность названия кафе
        /// </summary>
        public virtual async Task<bool> СheckUniqueName(string name, long cafeId = -1)
        {
            var response = await GetAsync<bool>($"{BaseUrl}/checkUniqueName?name={name}&cafeId={cafeId}");
            return response.Succeeded == true ? response.Content : false;
        }

        public Task<HttpResult<CafeModel>> EditCafeAsync(CafeModel cafe)
        {
            return PutAsync<CafeModel>($"{BaseUrl}", cafe);
        }

        public virtual async Task<HttpResult<CafeManagerModel>> GetCafeManager(long cafeId, long id)
        {
            return await GetAsync<CafeManagerModel>($"{BaseUrl}/{cafeId}/managers/{id}");
        }

        public virtual async Task<bool> EditUserCafeLink(long cafeId, long userId)
        {
            var httpResult = await PutAsync<bool>($"{BaseUrl}/EditUserCafeLink/{cafeId}/{userId}", null);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return false;
        }

        public virtual async Task<long> AddNewCostOfDelivery(CostOfDeliveryModel model)
        {
            var httpResult = await PostAsync<long>($"{BaseUrl}/AddNewCostOfDelivery", model);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return -1;
        }

        public virtual async Task<bool> EditCostOfDelivery(CostOfDeliveryModel model)
        {
            var httpResult = await PutAsync<bool>($"{BaseUrl}/EditCostOfDelivery", model);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return false;
        }

        public virtual async Task<CafeBusinessHoursModel> GetCafeBusinessHours(long cafeId)
        {
            var httpResult = await GetAsync<CafeBusinessHoursModel>($"{BaseUrl}/GetCafeBusinessHours?cafeId={cafeId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<int> GetDiscountAmount(long cafeId, DateTime date, long? companyId = null)
        {
            var unixTime = DateTimeExtensions.ConvertToUnixTimestamp(date);
            var query = $"{BaseUrl}/{cafeId}/discounts/{unixTime}";
            if (companyId.HasValue)
                query = query + "?companyId=" + companyId;
            var response = await GetAsync<int>(query);
            return response.Succeeded ? response.Content : 0;
        }

        public virtual async Task<CafeModel> GetCafeByIdIgnoreActivity(long cafeId)
        {
            var httpResult = await GetAsync<CafeModel>($"{BaseUrl}/GetCafeByIdIgnoreActivity?cafeId={cafeId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<CafeModel> GetCafeByUrl(string cleanUrlName)
        {
            var httpResult = await GetAsync<CafeModel>($"{BaseUrl}", cleanUrlName);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<CafeInfoModel> GetCafeInfo(long cafeId)
        {
            var httpResult = await GetAsync<CafeInfoModel>($"{BaseUrl}/GetCafeInfo", value: cafeId);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<CafeNotificationContactModel> GetCafeNotificationContactById(long id)
        {
            var httpResult = await GetAsync<CafeNotificationContactModel>($"{BaseUrl}/CafeNotificationContact", value: id);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }


        public virtual async Task<List<CafeModel>> GetCafes()
        {
            var httpResult = await GetAsync<List<CafeModel>>($"{BaseUrl}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<CafeModel>> GetCafesForSchedules()
        {
            var httpResult = await GetAsync<List<CafeModel>>($"{BaseUrl}/GetCafesForSchedules");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<CafeModel>> GetCafesRange(int startRange, int count)
        {
            var httpResult = await GetAsync<List<CafeModel>>($"{BaseUrl}/GetCafesRange?startRange={startRange}&count={count}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<CafeModel>> GetCafesToCurrentUser()
        {
            var httpResult = await GetAsync<List<CafeModel>>($"{BaseUrl}/GetCafesToCurrentUser");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<CafeModel>> GetCafesToUser(long userId)
        {
            var httpResult = await GetAsync<List<CafeModel>>($"{BaseUrl}/cafe/user/{userId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<double> GetCostOfDelivery(long cafeId, double price, bool corpOrder = false)
        {
            var httpResult = await GetAsync<double>($"{BaseUrl}/{cafeId}/cost/{price}/{corpOrder}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return 0;
        }

        public virtual async Task<List<CafeModel>> GetListOfCafeByUserId(long userId)
        {
            var httpResult = await GetAsync<List<CafeModel>>($"{BaseUrl}/cafes/user/{userId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<CostOfDeliveryModel>> GetListOfCostOfDelivery(long cafeId, double? price)
        {
            var httpResult = await GetAsync<List<CostOfDeliveryModel>>($"{BaseUrl}/GetListOfCostOfDelivery?cafeId={cafeId}&price={price}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<CompanyServersModel>>
        GetCafesAvailableToEmployee(List<DateTime> dates, long? cityId = null)
        {
            var datesQ = String.Join("&", dates.Select((x, i) => $"dates[{i}]={x.Ticks}"));
            string city = null;
            if (cityId!= null)
            {
                if (dates.Count > 0)
                    city = $"&cityId={cityId}";
                else
                    city = $"cityId={cityId}";
            }
            var url = $"{BaseUrl}/GetCafesAvailableToEmployee?{datesQ}{city}";
            var result = await GetAsync<List<CompanyServersModel>>(url).ConfigureAwait(false);
            return result.Succeeded ? result.Content : new List<CompanyServersModel> { };
        }

        public virtual async Task<List<CafeModel>> GetManagedCafes()
        {
            var httpResult = await GetAsync<List<CafeModel>>($"{BaseUrl}/GetManagedCafes");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<CafeNotificationContactModel>> GetNotificationContactsToCafe(long cafeId, NotificationChannelModel? notificationChannel)
        {
            var httpResult = await GetAsync<List<CafeNotificationContactModel>>($"{BaseUrl}/GetNotificationContactsToCafe?cafeId={cafeId}&notificationChannel={(long?)notificationChannel}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<bool> IsUserOfManagerCafe(long cafeId)
        {
            var httpResult = await GetAsync<bool>($"{BaseUrl}/IsUserOfManagerCafe?cafeId={cafeId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return false;
        }

        public virtual async Task RemoveCafe(long cafeId)
        {
            await DeleteAsync($"{BaseUrl}/{cafeId}");
        }

        public virtual async Task RemoveCafeNotificationContact(long id)
        {
            await DeleteAsync($"{BaseUrl}/RemoveCafeNotificationContact/{id}");
        }

        public virtual async Task<bool> RemoveCostOfDelivery(long costOfDeliveryId)
        {
            var httpResult = await DeleteAsync<bool>($"{BaseUrl}/RemoveCostOfDelivery/{costOfDeliveryId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return false;
        }

        public virtual async Task<bool> RemoveUserCafeLink(long cafeId, long userId)
        {
            var httpResult = await DeleteAsync<bool>($"{BaseUrl}/RemoveUserCafeLink/{cafeId}/{userId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return false;
        }

        public virtual async Task SetCafeBusinessHours(CafeBusinessHoursModel cafeBusinessHours)
        {
            await PostAsync($"{BaseUrl}/SetCafeBusinessHours", cafeBusinessHours);
        }

        public virtual async Task SetCafeInfo(CafeInfoModel cafeInfo)
        {
            await PostAsync($"{BaseUrl}/SetCafeInfo", cafeInfo);
        }

        public virtual async Task<bool> UpdateCafeNotificationContact(CafeNotificationContactModel cafeNotificationContact)
        {
            var httpResult = await PutAsync<bool>($"{BaseUrl}/UpdateCafeNotificationContact", cafeNotificationContact);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return false;
        }
    }
}