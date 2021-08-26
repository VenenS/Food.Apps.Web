using Food.Services.Models;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.AuthorizationService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using OrderModel = ITWebNet.Food.Core.DataContracts.Common.OrderModel;

namespace ITWebNet.Food.Site.Controllers
{
    public class OrdersController : BaseController
    {
        private readonly IConfiguration configuration;

        [ActivatorUtilitiesConstructor]
        public OrdersController(
            IAccountService accountSevice,
            IProfileService profileClient,
            IAddressesService addressServiceClient,
            IBanketsService banketsService,
            ICafeService cafeService,
            ICompanyService companyService,
            IDishCategoryService dishCategoryService,
            IDishService dishService,
            ICompanyOrderService companyOrderServiceClient,
            IOrderItemService orderItemServiceClient,
            IOrderService orderServiceClient,
            IRatingService ratingServiceClient,
            ITagService tagServiceClient,
            IUsersService userServiceClient,
            IMenuService menuServiceClient,
            ICityService cityService,
            IConfiguration configuration) :
            base(accountSevice, profileClient, addressServiceClient,
                banketsService, cafeService, companyService, dishCategoryService,
                dishService, companyOrderServiceClient, orderItemServiceClient,
                orderServiceClient, ratingServiceClient, tagServiceClient,
                userServiceClient, menuServiceClient, cityService)
        {
            this.configuration = configuration;
        }

        public OrdersController(IConfiguration config) : base()
        {
            configuration = config;
        }

        public OrdersController(ServiceManager manager, IConfiguration configuration) : base(manager)
        {
            this.configuration = configuration;
        }


        [HttpPost]
        public async Task<ActionResult> ChangeAddress(long OrderId, long AddressId)
        {
            var deliveryInfo = await OrderServiceClient.GetOrder(OrderId);
            deliveryInfo.DeliveryAddressId = AddressId;
            var address = await AddressServiceClient.GetAddressById(AddressId);
            deliveryInfo.OrderInfo.OrderAddress = address.GetAddressString();
            await OrderServiceClient.ChangeOrder(deliveryInfo);
            HttpContext.Session.SetCurrentDeliveryAddress(deliveryInfo.OrderInfo.OrderAddress);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            DateTime end = DateTime.Now;
            DateTime start = end.AddDays(-7);

            var viewModel = new OrderHistoryListViewModel();
            viewModel.OrderHistoryList = await LoadHistoryAsync(start, end);

            return View(viewModel);
        }

        private async Task<List<OrderHistoryViewModel>> LoadHistoryAsync(DateTime start, DateTime end)
        {
            List<OrderHistoryViewModel> history = new List<OrderHistoryViewModel>();
            int blockSize;

            if (!Int32.TryParse(configuration.GetSection("blockSize").Value, out blockSize))
                blockSize = 10;
            var currentUser = long.Parse(User.Identity.GetUserId());
            var orders =
                await OrderServiceClient.GetUserOrdersInDateRange(currentUser, start, end);
            if (orders == null)
                return history;

            foreach (var order in orders)
            {
                try
                {
                    history.Add(OrderHistoryViewModel.FromDTO(order));
                }
                catch (Exception ex)
                {
                }
            }
            return history.OrderByDescending(item => item.Created).ToList();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> DeleteOrderItem(long orderId, long orderItemId)
        {
            var order = await OrderServiceClient.GetOrder(orderId);
            if (order == null)
                return BadRequest();

            // удаляем если только в статусе новый
            if (order.Status != (long)OrderStatusEnum.Created)
            {
                ModelState.AddModelError("", "Невозможно удалить блюдо из заказа в данном статусе");
            }
            if (order.CompanyOrderId.HasValue)
            {
                var companyOrder = await base.CompanyOrderServiceClient.GetCompanyOrder(order.CompanyOrderId.Value);
                if (companyOrder != null)
                {
                    if (
                        (
                            companyOrder.OrderAutoCloseDate != null
                            && companyOrder.OrderAutoCloseDate < DateTime.Now
                        )
                        ||
                        (
                            companyOrder.OrderOpenDate != null
                            && companyOrder.OrderOpenDate > DateTime.Now
                        )
                    )
                    {
                        ModelState.AddModelError("",
                            "Невозможно удалить блюдо из корпоративного заказа, который был уже доставлен");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var response = await OrderItemServiceClient.RemoveOrderItem(orderItemId);
                if (!response)
                    return BadRequest();
                //если в заказе не осталось позиций, то отменяем его
                if (order.OrderItems != null && order.OrderItems.Count == 1 &&
                    order.OrderItems.FirstOrDefault(c => c.Id == orderItemId) != null)
                {
                    OrderModel abortedOrder = new OrderModel
                    {
                        Id = orderId,
                        Status = (long)OrderStatusEnum.Abort
                    };

                    await OrderServiceClient.ChangeOrder(abortedOrder);
                }
                order = await OrderServiceClient.GetOrder(orderId);
            }

            var model = await FillOrderHistory(order);
            ViewBag.CollapseIn = true;
            return PartialView("_HistoryItem", model);
        }

        private async Task<OrderHistoryViewModel> FillOrderHistory(OrderModel order)
        {
            try
            {
                OrderHistoryModel historyModel = await OrderServiceClient.GetOrderHistory(
                    User.Identity.GetUserId<long>(), order);
                return OrderHistoryViewModel.FromDTO(historyModel);
            }
            catch //(Exception ex)
            {
                return null;
            }
        }

        [HttpPost, AjaxOnly]
        public async Task<ActionResult> DiscardOrder(long orderId)
        {
            OrderModel order = await OrderServiceClient.GetOrder(orderId);

            if (order.BanketId.HasValue)
            {
                ModelState.AddModelError("", "Невозможно отменить банкетный заказ");
            }

            if (order.Status == (long)OrderStatusEnum.Abort)
                ModelState.AddModelError("", "Заказ уже отменен.");

            var statuses = await OrderServiceClient.GetAvailableOrderStatuses(orderId, false);

            if (statuses == null || statuses.Count == 0)
                ModelState.AddModelError("", "Невозможно изменить статус заказа.");

            if (!statuses.Contains(OrderStatusEnum.Abort))
                ModelState.AddModelError("", "Заказ уже нельзя отменить, так как он уже принят в работу.");

            if (ModelState.IsValid)
            {
                OrderModel abortedOrder = new OrderModel
                {
                    Id = orderId,
                    Status = (long)OrderStatusEnum.Abort
                };

                var orderStatus = await OrderServiceClient.ChangeOrder(abortedOrder);
            }

            order = await OrderServiceClient.GetOrder(orderId);

            var orderHistory = await FillOrderHistory(order);

            return PartialView("_HistoryItem", orderHistory);
        }

        [HttpGet, AjaxOnly]
        public async Task<ActionResult> FilterHistory(DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date;

            var viewModel = new OrderHistoryListViewModel();

            if (end < start)
                viewModel.Message = MessageViewModel.Error("Дата начала периода не может быть больше даты окончания");
            else
                viewModel.OrderHistoryList = await LoadHistoryAsync(start, end);

            return PartialView("_HistoryList", viewModel);
        }
    }
}