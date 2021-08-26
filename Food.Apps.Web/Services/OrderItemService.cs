using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public class OrderItemService : BaseClient<OrderItemService>, IOrderItemService
    {
        private static readonly string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static readonly string BaseUrl = ServiceUrl + "api/orderitem";

        public OrderItemService() : base(ServiceUrl)
        {
        }

        public OrderItemService(string baseAddress) : base(baseAddress)
        {
        }

        public OrderItemService(bool debug) : base(debug)
        {
        }

        public OrderItemService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public virtual async Task<OrderStatusModel> ChangeOrderItem(OrderItemModel orderItem)
        {
            var response = await PostAsync<OrderStatusModel>($"{BaseUrl}/changeitem", orderItem);
            return response.Content;
        }

        public virtual async Task<OrderStatusModel> PostOrderItems(OrderItemModel[] orderItems, long orderId)
        {
            var response = await PostAsync<OrderStatusModel>($"{BaseUrl}/additems/{orderId}", orderItems);
            return response.Content;
        }

        public virtual async Task<bool> RemoveOrderItem(long orderItemId)
        {
            var response = await DeleteAsync<bool>($"{BaseUrl}/{orderItemId}");
            return response.Content;
        }
    }
}