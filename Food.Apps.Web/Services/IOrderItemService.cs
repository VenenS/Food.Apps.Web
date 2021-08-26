using ITWebNet.Food.Core.DataContracts.Common;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface IOrderItemService
    {
        Task<OrderStatusModel> ChangeOrderItem(OrderItemModel orderItem);
        Task<OrderStatusModel> PostOrderItems(OrderItemModel[] orderItems, long orderId);
        Task<bool> RemoveOrderItem(long orderItemId);
    }
}