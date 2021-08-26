using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface ICafeService
    {
        Task<HttpResult<CafeManagerModel>> AddCafeManager(CafeManagerModel model);
        Task<HttpResult> AddCafeMenuPatternAsync(CafeMenuPatternModel pattern);
        Task<HttpResult<CafeModel>> AddNewCafeAsync(CafeModel cafe);
        Task<long> AddNewCafeNotificationContact(CafeNotificationContactModel cafeNotificationContact);
        Task<long> AddNewCostOfDelivery(CostOfDeliveryModel model);
        Task<bool> AddUserCafeLink(long cafeId, long userId);
        Task<bool> DeleteCafeManager(long cafeId, long userId);
        Task<HttpResult<CafeModel>> EditCafeAsync(CafeModel cafe);
        Task<bool> EditCostOfDelivery(CostOfDeliveryModel model);
        Task<bool> EditUserCafeLink(long cafeId, long userId);
        Task<List<CafeModel>> GetAllCafesAsync();
        Task<List<BanketModel>> GetBanketsByCafeId(long cafeId);
        Task<CafeBusinessHoursModel> GetCafeBusinessHours(long cafeId);
        Task<CafeModel> GetCafeByCleanUrl(string cleanUrl);
        Task<CafeModel> GetCafeById(long id);
        Task<CafeModel> GetCafeByIdIgnoreActivity(long cafeId);
        Task<CafeModel> GetCafeByUrl(string cleanUrlName);
        Task<CafeInfoModel> GetCafeInfo(long cafeId);
        Task<HttpResult<CafeManagerModel>> GetCafeManager(long cafeId, long id);
        Task<List<CafeManagerModel>> GetCafeManagers(long cafeId);
        Task<CafeNotificationContactModel> GetCafeNotificationContactById(long id);
        Task<List<CafeModel>> GetCafes();
        Task<List<CafeModel>> GetCafesAsync();
        Task<List<CompanyServersModel>> GetCafesAvailableToEmployee(List<DateTime> dates, long? cityId = null);
        Task<List<CafeModel>> GetCafesRange(int startRange, int count);
        Task<List<CafeModel>> GetCafesToCurrentUser();
        Task<List<CafeModel>> GetCafesToCurrentUserAsync();
        Task<List<CafeModel>> GetCafesToUser(long userId);
        Task<List<CompanyOrderScheduleModel>> GetCompanyOrderSchedulesByCafeId(long cafeId);
        Task<double> GetCostOfDelivery(long cafeId, double price, bool corpOrder = false);
        Task<OrderDeliveryPriceModel> GetCostOfDeliveryAsync(long cafeId, double price, bool corpOrder = false, string orderDate = "");
        Task<int> GetDiscountAmount(long cafeId, DateTime date, long? companyId = null);
        Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetFilteredMenuByPatternId(long cafeId, long id, string search);
        Task<List<CafeModel>> GetListOfCafeByUserId(long userId);
        Task<List<CafeModel>> GetCafesForSchedules();
        Task<List<CostOfDeliveryModel>> GetListOfCostOfDelivery(long cafeId, double? price);
        Task<List<CafeModel>> GetManagedCafes();
        Task<List<CafeModel>> GetManagedCaffee();
        Task<List<CafeModel>> GetManagedCaffeeByMail(string email);
        Task<Dictionary<FoodCategoryModel, List<FoodDishModel>>> GetMenuByPatternIdAsync(long cafeId, long id);
        Task<List<CafeMenuPatternModel>> GetMenuPatternsByCafeIdAsync(long cafeId);
        Task<List<CafeNotificationContactModel>> GetNotificationContactsToCafe(long cafeId, NotificationChannelModel? notificationChannel);
        Task<OrderDeliveryPriceModel> GetShippingCostToSingleCompanyAddress(string address, long companyOrderId, double extraCash);
        Task<OrderDeliveryPriceModel> GetShippingCostToSingleCompanyAddress(string address, long companyId, long cafeId, double extraCash, DateTime date);
        Task<bool> IsUserOfManagerCafe(long cafeId);
        Task RemoveCafe(long cafeId);
        Task<bool> RemoveCafeAsync(long cafeId);
        Task<HttpResponseMessage> RemoveCafeMenuPatternAsync(long cafeId, long patternId);
        Task RemoveCafeNotificationContact(long id);
        Task<bool> RemoveCostOfDelivery(long costOfDeliveryId);
        Task<bool> RemoveUserCafeLink(long cafeId, long userId);
        Task SetCafeBusinessHours(CafeBusinessHoursModel cafeBusinessHours);
        Task SetCafeInfo(CafeInfoModel cafeInfo);
        Task<HttpResult> UpdateCafeMenuPatternAsync(CafeMenuPatternModel pattern);
        Task<bool> UpdateCafeNotificationContact(CafeNotificationContactModel cafeNotificationContact);
        Task<bool> СheckUniqueName(string name, long cafeId = -1);
    }
}