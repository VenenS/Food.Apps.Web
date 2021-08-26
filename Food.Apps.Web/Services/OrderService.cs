using Food.Services.Models;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public class OrderService : BaseClient<OrderService>, IOrderService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static string BaseUrl = ServiceUrl + "api/orders";

        public OrderService() : base(ServiceUrl)
        {
        }

        public OrderService(string baseAddress) : base(baseAddress)
        {
        }

        public OrderService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public OrderService(bool debug) : base(debug)
        {
        }

        public virtual async Task<bool> ChangeCompanyOrderStatus(long companyOrderId, OrderStatusEnum newStatus, long? cafeId, long? companyId)
        {
            string cafeIdPart = cafeId != null ? $"/cafe/{cafeId}" : "";
            string companyIdPart = companyId != null ? $"/company/{companyId}" : "";
            var response = await PostAsync<bool>($"{BaseUrl}/changecompanyorderstatus/{companyOrderId}{cafeIdPart}{companyIdPart}", newStatus);
            return response.Content;
        }

        public virtual async Task<OrderStatusModel> ChangeOrder(OrderModel order)
        {
            var response = await PostAsync<OrderStatusModel>($"{BaseUrl}/changeorder", order);
            return response.Content;
        }

        public virtual async Task<OrderStatusModel[]> ChangeOrders(List<OrderModel> orders)
        {
            var response = await PostAsync<OrderStatusModel[]>($"{BaseUrl}/changeorders", orders);
            return response.Content;
        }

        public virtual async Task<bool> ChangeOrderStatus(long? cafeId, long? companyId, long orderId, OrderStatusEnum newStatus)
        {
            string cafeIdPart = cafeId != null ? $"/cafe/{cafeId}" : "";
            string companyIdPart = companyId != null ? $"/company/{companyId}" : "";
            var response = await GetAsync<bool>($"{BaseUrl}/changestatus/{orderId}{cafeIdPart}{companyIdPart}?newStatus={(int)newStatus}");
            return response.Content;
        }

        public virtual async Task<List<OrderModel>> ChangeStatusOrdersInCompany(OrdersChangeStatusModel model)
        {
            var response = await PostAsync<List<OrderModel>>($"{BaseUrl}/ChangeStatusOrdersInCompany", model);
            return response.Content;
        }

        public virtual async Task<List<OrderStatusEnum>> GetAvailableOrderStatuses(long orderId, bool fromReport = true)
        {
            var response = await GetAsync<List<OrderStatusEnum>>($"{BaseUrl}/availablestatuses/{orderId}/fromreport/{fromReport}");
            return response.Content;
        }

        public virtual async Task<List<CompanyOrderModel>> GetCompanyOrderByFilters(ReportFilter reportFilter)
        {
            var response = await PostAsync<List<CompanyOrderModel>>($"{BaseUrl}/companyordersbyfilter", reportFilter);
            return response.Content;
        }

        public virtual async Task<OrderModel[]> GetCurrentListOfOrdersAndOrderItemsToCafe(long cafeId, DateTime? wantedDateTime)
        {
            string datetime = wantedDateTime != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(wantedDateTime.Value)}" : "";
            var response = await GetAsync<OrderModel[]>($"{BaseUrl}/currentordersbycafewithitems/{cafeId}{datetime}");
            return response.Content;
        }

        public virtual async Task<OrderModel[]> GetCurrentListOfOrdersToCafe(long cafeId, DateTime? wantedDateTime)
        {
            string datetime = wantedDateTime != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(wantedDateTime.Value)}" : "";
            var response = await GetAsync<OrderModel[]>($"{BaseUrl}/currentordersbycafe/{cafeId}{datetime}");
            return response.Content;
        }

        public virtual async Task<List<KeyValuePair<UserModel, OrderModel>>> GetCurrentListOfOrdersToCafeOrderByClients(long cafeId, DateTime? wantedDateTime)
        {
            string datetime = wantedDateTime != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(wantedDateTime.Value)}" : "";
            var response = await GetAsync<List<KeyValuePair<UserModel, OrderModel>>>($"{BaseUrl}/currentordersbycafe/orderbyclients/{cafeId}{datetime}");
            return response.Content;
        }

        public virtual async Task<List<KeyValuePair<UserModel, OrderModel>>> GetCurrentListOfOrdersToCafeOrderByCompanies(long cafeId, DateTime? wantedDateTime)
        {
            string datetime = wantedDateTime != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(wantedDateTime.Value)}" : "";
            var response = await GetAsync<List<KeyValuePair<UserModel, OrderModel>>>($"{BaseUrl}/currentordersbycafe/orderbycompanies/{cafeId}{datetime}");
            return response.Content;
        }

        public virtual async Task<OrderModel> GetOrder(long orderId)
        {
            var response = await GetAsync<OrderModel>($"{BaseUrl}/order/{orderId}");
            return response.Content;
        }

        public virtual async Task<List<OrderModel>> GetOrderByFilters(ReportFilter reportFilter)
        {
            var response = await PostAsync<List<OrderModel>>($"{BaseUrl}/orderbyfilter", reportFilter);
            return response.Content;
        }

        public virtual async Task<List<OrderItemModel>> GetOrderItems(long userId, long orderId)
        {
            var response = await GetAsync<List<OrderItemModel>>($"{BaseUrl}/orderitems/user/{userId}/order/{orderId}");
            return response.Content;
        }

        public virtual async Task<OrderModel> GetOrderForManager(long orderId)
        {
            var response = await GetAsync<OrderModel>($"{BaseUrl}/orderformanager/{orderId}");
            return response.Content;
        }

        public virtual async Task<TotalDetailsModel> GetOrderPriceDetails(OrderModel order)
        {
            var response = await PostAsync<TotalDetailsModel>($"{BaseUrl}/orderpricedetails", order);
            return response.Content;
        }

        public virtual async Task<string> GetOrdersInCompanyOrderByCompanyOrderIdInTxt(long companyOrderId)
        {
            var response = await GetAsync<string>($"{BaseUrl}/companyordersbyorderid/txt/{companyOrderId}");
            return response.Content;
        }

        public virtual async Task<string> GetOrdersInCompanyOrderByCompanyOrderIdInXml(long companyOrderId)
        {
            var response = await GetAsync<string>($"{BaseUrl}/companyordersbyorderid/xml/{companyOrderId}");
            return response.Content;
        }

        public virtual async Task<long> GetTotalOrdersPerDay()
        {
            var response = await GetAsync<long>($"{BaseUrl}/totalordersperday");
            return response.Content;
        }

        public virtual async Task<List<OrderModel>> GetUserCurrentCart()
        {
            var response = await GetAsync<List<OrderModel>>($"{BaseUrl}/currentcart");
            return response.Content;
        }

        public virtual async Task<List<OrderModel>> GetUserOrders(long userId, DateTime? orderDate)
        {
            var datetime = orderDate != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(orderDate.Value)}" : "";
            var response = await GetAsync<List<OrderModel>>($"{BaseUrl}/userorders/{userId}{datetime}");
            return response.Content;
        }

        public virtual async Task<List<OrderModel>> GetUserOrdersInCompanyOrder(long companyOrderId)
        {
            var response = await GetAsync<List<OrderModel>>($"{BaseUrl}/userincompanyorders/{companyOrderId}");
            return response.Content;
        }

        public virtual async Task<List<OrderModel>> GetUserOrdersInDateRange(long userId, DateTime? startOrderDate, DateTime? endOrderDate)
        {
            var from = startOrderDate != null ? $"/from/{DateTimeExtensions.ConvertToUnixTimestamp(startOrderDate.Value)}" : "";
            var to = endOrderDate != null ? $"/to/{DateTimeExtensions.ConvertToUnixTimestamp(endOrderDate.Value)}" : "";
            var response = await GetAsync<List<OrderModel>>($"{BaseUrl}/userordersindates/{userId}{from}{to}");
            return response.Content;
        }

        public virtual async Task<Dictionary<OrderModel, List<FoodDishModel>>> GetUserOrderWithItemsAndDishes(long userId, DateTime? orderDate)
        {
            var datetime = orderDate != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(orderDate.Value)}" : "";
            var response = await GetAsync<Dictionary<OrderModel, List<FoodDishModel>>>($"{BaseUrl}/userorderWithitemsanddishes/{userId}{datetime}");
            return response.Content;
        }

        public virtual async Task<PostOrderResponse> PostOrders(PostOrderRequest orderRequest)
        {
            var response = await PostAsync<PostOrderResponse>($"{BaseUrl}/postorders", orderRequest);
            return response.Content;
        }

        public virtual async Task<OrderStatusModel> SaveUserOrderCartIntoBase(OrderModel userOrder)
        {
            var response = await PostAsync<OrderStatusModel>($"{BaseUrl}/saveuserordercart", userOrder);
            return response.Content;
        }

        public virtual async Task<OrderHistoryModel> GetOrderHistory(long userId, OrderModel order)
        {
            var response = await PostAsync<OrderHistoryModel>($"{BaseUrl}/getOrderHistory/{userId.ToString()}", order);
            if (response.Succeeded)
                return response.Content;
            else
                return null;
        }
    }
}