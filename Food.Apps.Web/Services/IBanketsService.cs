using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface IBanketsService
    {
        Task<HttpResult<BanketModel>> CreateBanket(BanketModel model);
        Task<HttpResponseMessage> DeleteBanket(long banketId);
        Task<bool> DeleteOrderItem(long orderItemId);
        Task<HttpResult<BanketModel>> GetBanketById(long banketId);
        Task<List<BanketModel>> GetBanketsByFilter(BanketsFilterModel model);
        Task<HttpResult<OrderModel>> GetOrderByBanketId(long banketId, long userId);
        Task<List<OrderModel>> GetOrdersInBanket(long banketId);
        Task<HttpResult> PostOrders(OrderModel order);
        Task<HttpResult<bool>> UpdateBanket(BanketModel model);
        Task<HttpResult> UpdateOrder(OrderModel order);
    }
}