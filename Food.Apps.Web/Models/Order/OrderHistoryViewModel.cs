using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Site.Models
{
    public class OrderHistoryViewModel : CartViewModel
    {
        public DateTime Created { get; set; }

        public double TotalSum { get; set; }

        public double? DeliverySum { get; set; }

        public BanketModel Banket { get; set; }

        public long Id { get; set; }

        public List<OrderStatusEnum> AvailableStatuses { get; set; }
        public long? CompanyOrderId { get; set; }
        public DeliveryAddressViewModel DeliveryAddress { get; set; }
        public long? DeliveryAddressNow { get; set; }
        public string DeliveryInfoPayType { get; set; }
        public OrderInfoModel OrderInfo { get; set; }

        public bool IsDiscardable => AvailableStatuses.Contains(OrderStatusEnum.Abort);

        public static OrderHistoryViewModel FromDTO(OrderHistoryModel historyModel)
        {
            List<CartItemViewModel> items = new List<CartItemViewModel>();
            foreach (var item in historyModel.Order.OrderItems)
            {
                FoodDishModel dish = historyModel.Dishes.Find(d => d.Id == item.FoodDishId);
                if (dish != null)
                {
                    CartItemViewModel cartItem = CartItemViewModel.FromDish(dish, item.Discount ?? 0, null);
                    cartItem.Count = item.DishCount;
                    cartItem.BasePrice = item.DishBasePrice;
                    cartItem.Comment = item.Comment;
                    cartItem.OrderItemId = item.Id;
                    items.Add(cartItem);
                }
            }

            return new OrderHistoryViewModel
            {
                CartItems = items,
                Created = historyModel.Order.Create,
                DeliverDate = historyModel.Order.DeliverDate.Value,
                Status = (OrderStatusEnum)historyModel.Order.Status,
                Cafe = historyModel.Cafe,
                TotalSum = historyModel.Order.TotalSum.Value,
                Id = historyModel.Order.Id,
                DeliverySum = historyModel.Order.OrderInfo?.DeliverySumm,
                AvailableStatuses = historyModel.AvailableStatuses,
                Banket = historyModel.Banket,
                CompanyOrderId = historyModel.Order.CompanyOrderId,
                DeliveryAddressNow = historyModel.Order.DeliveryAddressId.Value,
                OrderInfo = historyModel.Order?.OrderInfo,
            };
        }

        public static OrderHistoryViewModel FromDTO(OrderModel orderModel)
        {
            List<CartItemViewModel> items = new List<CartItemViewModel>();
            foreach (var item in orderModel.OrderItems)
            {
                CartItemViewModel dish = new CartItemViewModel
                {
                    Id = item.Id,
                    Count = item.DishCount,
                    Name = item.DishName,
                    Kcalories = item.DishKcalories,
                    Weight = item.DishWeight,
                    BasePrice = item.DishBasePrice
                };

                items.Add(dish);
            }

            //if (orderModel.PayType == "CourierCash")
            //    orderModel.PayType = "Наличными курьеру";
            //else if (orderModel.PayType == "CourierCard")
            //    orderModel.PayType = "Банковской картой курьеру";
            //else if (orderModel.PayType == "InternetAcquiring")
            //    orderModel.PayType = "Банковской картой на сайте";
            //else 
            //    orderModel.PayType = "Безналичный расчет";
            return new OrderHistoryViewModel
            {
                CartItems = items,
                Created = orderModel.Create,
                DeliverDate = orderModel.DeliverDate.Value,
                Status = (OrderStatusEnum)orderModel.Status,
                Cafe = orderModel.Cafe,
                TotalSum = orderModel.TotalSum.Value,
                Id = orderModel.Id,
                DeliverySum = orderModel.OrderInfo?.DeliverySumm,
                CompanyOrderId = orderModel.CompanyOrderId,
                DeliveryAddressNow = orderModel.DeliveryAddressId.Value,
                OrderInfo = orderModel.OrderInfo,
                DeliveryInfoPayType = DictionaryOfpaymentTypes.ReturningAValueByKey(orderModel.PayType),
            };
        }
    }
}