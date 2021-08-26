using Food.Services.Models;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface IOrderService
    {
        Task<bool> ChangeCompanyOrderStatus(long companyOrderId, OrderStatusEnum newStatus, long? cafeId, long? companyId);
        Task<OrderStatusModel> ChangeOrder(OrderModel order);
        Task<OrderStatusModel[]> ChangeOrders(List<OrderModel> orders);
        Task<bool> ChangeOrderStatus(long? cafeId, long? companyId, long orderId, OrderStatusEnum newStatus);
        Task<List<OrderModel>> ChangeStatusOrdersInCompany(OrdersChangeStatusModel model);
        Task<List<OrderStatusEnum>> GetAvailableOrderStatuses(long orderId, bool fromReport = true);
        Task<List<CompanyOrderModel>> GetCompanyOrderByFilters(ReportFilter reportFilter);
        Task<OrderModel[]> GetCurrentListOfOrdersAndOrderItemsToCafe(long cafeId, DateTime? wantedDateTime);
        Task<OrderModel[]> GetCurrentListOfOrdersToCafe(long cafeId, DateTime? wantedDateTime);
        Task<List<KeyValuePair<UserModel, OrderModel>>> GetCurrentListOfOrdersToCafeOrderByClients(long cafeId, DateTime? wantedDateTime);
        Task<List<KeyValuePair<UserModel, OrderModel>>> GetCurrentListOfOrdersToCafeOrderByCompanies(long cafeId, DateTime? wantedDateTime);
        Task<OrderModel> GetOrder(long orderId);
        Task<List<OrderModel>> GetOrderByFilters(ReportFilter reportFilter);
        Task<OrderModel> GetOrderForManager(long orderId);
        Task<OrderHistoryModel> GetOrderHistory(long userId, OrderModel order);
        Task<List<OrderItemModel>> GetOrderItems(long userId, long orderId);
        Task<TotalDetailsModel> GetOrderPriceDetails(OrderModel order);
        Task<string> GetOrdersInCompanyOrderByCompanyOrderIdInTxt(long companyOrderId);
        Task<string> GetOrdersInCompanyOrderByCompanyOrderIdInXml(long companyOrderId);
        Task<long> GetTotalOrdersPerDay();
        Task<List<OrderModel>> GetUserCurrentCart();
        Task<List<OrderModel>> GetUserOrders(long userId, DateTime? orderDate);
        Task<List<OrderModel>> GetUserOrdersInCompanyOrder(long companyOrderId);
        Task<List<OrderModel>> GetUserOrdersInDateRange(long userId, DateTime? startOrderDate, DateTime? endOrderDate);
        Task<Dictionary<OrderModel, List<FoodDishModel>>> GetUserOrderWithItemsAndDishes(long userId, DateTime? orderDate);
        Task<PostOrderResponse> PostOrders(PostOrderRequest orderRequest);
        Task<OrderStatusModel> SaveUserOrderCartIntoBase(OrderModel userOrder);
    }
}