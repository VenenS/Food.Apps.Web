using ITWebNet.Food.Core.DataContracts.Manager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface IDiscountService
    {
        Task<long> AddDiscount(DiscountModel model);
        Task<bool> EditDiscount(DiscountModel model);
        Task<int> GetDiscountAmount(long cafeId, DateTime date, long? companyID);
        Task<List<DiscountModel>> GetDiscounts(long[] discountIdList);
        Task<List<DiscountModel>> GetUserDiscounts(long userId);
        Task<bool> RemoveDiscount(long discountId);
    }
}