using Food.Services.Models;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrderModel = ITWebNet.Food.Core.DataContracts.Common.OrderModel;

namespace ITWebNet.Food.Site.Areas.Manager.Controllers
{
    public class ReportsController : BaseController
    {
        private IReportService reportServiceClient;
        private IOrderService orderServiceClient;
        private IBanketsService _banketsService;
        private IOrderItemService orderItemServiceClient;

        [ActivatorUtilitiesConstructor]
        public ReportsController(
            IMemoryCache cache, 
            ICafeService cafeService, 
            IReportService reportServiceClient, 
            IOrderService orderServiceClient, 
            IBanketsService banketsService, 
            IOrderItemService orderItemServiceClient)
            : base(cache, cafeService)
        {
            this.reportServiceClient = reportServiceClient;
            this.orderServiceClient = orderServiceClient;
            _banketsService = banketsService;
            this.orderItemServiceClient = orderItemServiceClient;
        }

        [AjaxOnly]
        public async Task<ActionResult> ChangeStatus(long id, long cafeId, OrderStatusEnum status, bool details = false)
        {
            if (await orderServiceClient.ChangeOrderStatus(cafeId, null, id, status))
            {
                ModelState.Clear();

                if (details)
                    return RedirectToRoute("ManagerCafe",
                        new {action = "Details", controller = "Reports", id, cafeId, orderType = EnumOrderType.Individual});

                ReportDetailsModel model = await GetDetailsForOrderAsync(cafeId, id, EnumOrderType.Individual);

                if (model.UserOrder == null)
                    return NotFound();
                
                return PartialView("_UserReportsItem", model.UserOrder);
            }

            return NoContent();
        }

        [AjaxOnly]
        public async Task<ActionResult> ChangeStatusOrdersInCompany(List<long> orderIds, OrderStatusEnum status)
        {
            var model = new OrdersChangeStatusModel() { OrderIds = orderIds, Status = status };
            var response = await orderServiceClient.ChangeStatusOrdersInCompany(model);
            var responseViewModel = response.Select(e => new OrderViewModel(e)).ToList();
            return PartialView("_ListUserReportsItem", responseViewModel);
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult> Index(long cafeId)
        {
            ModelState.Clear();

            ReportViewFilter filter = GetReportViewFilter(cafeId);

            ReportsModel model = await GetReportsDataAsync(filter, cafeId);
            await new CafeOrderNotificationService().SetOrdersViewed(cafeId);

            //TODO: Решить нужно ли менять общую сумму динамической
            //Редатирование суммы заказов без учета отмененных
            //foreach (var order in model.CompanyOrders)
            //{
            //    double price = 0;

            //    foreach (var userOrder in model.UserOrders.Where(c => c.CompanyOrderId == order.Id && c.Status != OrderStatusEnum.Abort).ToList())
            //    {
            //        price += userOrder.TotalSum ?? 0;
            //    }
            //    order.TotalPrice = price;
            //}

            return View(model);
        }
        public async Task<ActionResult> Details(long cafeId, long id, EnumOrderType orderType, string returnUrl = null)
        {
            ModelState.Clear();

            ReportDetailsModel model = await GetDetailsForOrderAsync(cafeId, id, orderType);

            if (model.CompanyOrder == null && model.UserOrder == null && model.BanketOrder == null)
                return NotFound();

            if (!string.IsNullOrEmpty(returnUrl))
                ViewBag.ReturnUrl = returnUrl;

            return View(model);
        }

        [AjaxOnly]
        public async Task<ActionResult> UserOrders(long companyOrderId, SortType sortType)
        {
            ViewBag.CompanyOrderId = companyOrderId;
            var orders = await orderServiceClient.GetUserOrdersInCompanyOrder(companyOrderId);
            var models = OrderViewModel.AsModelList(orders);
            models = SortUserOrders(models, sortType);
            return PartialView("_UserReportsGroup", models);
        }

        [AjaxOnly]
        public async Task<ActionResult> BanketOrders(long banketId)
        {
            var orders = await _banketsService.GetOrdersInBanket(banketId);
            var models = OrderViewModel.AsModelList(orders);
            ViewData["banketOrder"] = true;

            return PartialView("_UserReports", models);
        }
        
        public async Task<ActionResult> DeleteOrderItem(long cafeId, long orderId, long orderItemId)
        {
            var order = await orderServiceClient.GetOrder(orderId);
            if (order == null)
                return BadRequest();

            if (order.CafeId != cafeId)
            {
                return BadRequest();
            }

            // удаляем если только в статусе новый
            if (order.Status != (long) OrderStatusEnum.Created)
            {
                ModelState.AddModelError("", "Невозможно удалить блюдо из заказа в данном статусе");
            }

            if (ModelState.IsValid)
            {
                if (order.BanketId.HasValue)
                {
                    if (!await _banketsService.DeleteOrderItem(orderItemId))
                    {
                        return BadRequest();
                    }
                }
                else
                {

                    var response = await orderItemServiceClient.RemoveOrderItem(orderItemId);
                    if (!response)
                        return BadRequest();
                    //если в заказе не осталось позиций, то отменяем его
                    if (order.OrderItems != null && order.OrderItems.Count == 1 &&
                        order.OrderItems.FirstOrDefault(c => c.Id == orderItemId) != null)
                    {
                        OrderModel abortedOrder = new OrderModel
                        {
                            Id = orderId,
                            Status = (long) OrderStatusEnum.Abort
                        };

                        await orderServiceClient.ChangeOrder(abortedOrder);
                    }
                }

                order = await orderServiceClient.GetOrder(orderId);
            }
            
            var model = new OrderViewModel(order);

            ViewData["order"] = model;
            return PartialView("_OrderItem", model.OrderItems.FirstOrDefault(c => c.Id == orderItemId));
        }
        
        [AjaxOnly, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult> Filter(ReportViewFilter filter, long cafeId)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            ReportsModel model = await GetReportsDataAsync(filter, cafeId);

            //TODO: Решить нужно ли менять общую сумму динамической
            //Редатирование суммы заказов без учета отмененных 
            //foreach (var order in model.CompanyOrders)
            //{
            //    double price = 0;

            //    foreach (var userOrder in model.UserOrders.Where(c => c.CompanyOrderId == order.Id && c.Status != OrderStatusEnum.Abort).ToList())
            //    {
            //        price += userOrder.TotalSum ?? 0;
            //    }
            //    order.TotalPrice = price;
            //}

            return PartialView("Index", model);
        }

        [AjaxOnly, HttpPost]
        public async Task<ActionResult> Documents(ReportViewFilter filter, long cafeId)
        {
            bool isReportValid = false;
            int counter = 0;
            ReportModel report;

            do
            {
                report = await reportServiceClient
                    .GetReportFileByFilter(filter
                        .AsReportFilter(cafeId, null));

                if (report == null || report.FileBody.Length == 0)
                    return NotFound("No data present");

                using (MD5 hasher = MD5.Create())
                {
                    var hash = hasher.ComputeHash(report.FileBody);
                    isReportValid = hash.SequenceEqual(report.Hash);
                }
                counter++;

            } while (!isReportValid || counter > 3);

            string reportId = Guid.NewGuid().ToString();
            HttpContext.Session.Set(reportId, report);

            string url = Url.RouteUrl(
                "ManagerCafe",
                new
                {
                    cafeId = cafeId,
                    controller = "Reports",
                    action = "Documents",
                    reportId = reportId,
                    print = filter.ReportExtension
                });

            return Content(url);
        }

        [HttpGet]
        public ActionResult Documents(long cafeId, string reportId, ReportExtension print)
        {
            var report = HttpContext.Session.Get<ReportModel>(reportId);
            if (report == null)
                return RedirectToAction("Index");

            HttpContext.Session.Remove(reportId);
            switch(print)
            {
                case ReportExtension.HTML:
                    return View("_EmptyPage", Encoding.UTF8.GetString(report.FileBody));
                case ReportExtension.XLS:
                    return File(report.FileBody, MediaTypeNames.Application.Octet, report.FileName);
                case ReportExtension.PDF:
                    return File(report.FileBody, MediaTypeNames.Application.Pdf, report.FileName);
                default:
                    return RedirectToAction("Index");
            }

        }

        //формирование отчетов в нужном формате
        [HttpGet]
        public async Task<ActionResult> UserDetailsReport(long cafeId, long id, long user, ReportExtension reportExtension)
        {
            bool isReportValid = false;
            int counter = 0;
            ReportModel report;

            do
            {
                ReportFilter filter = new ReportFilter()
                {
                    CafeId = cafeId,
                    UserId = user,
                    ReportExtension = reportExtension,
                    OrdersIdList = new System.Collections.Generic.List<long>() { id},
                    ReportTypeId = 1 //ид для отчета
                };
                report = await reportServiceClient.GetUserDetailsReport(filter);
                if (report == null || report.FileBody.Length == 0)
                    return NotFound("No data present");

                using (MD5 hasher = MD5.Create())
                {
                    var hash = hasher.ComputeHash(report.FileBody);
                    isReportValid = hash.SequenceEqual(report.Hash);
                }
                counter++;

            } while (!isReportValid || counter > 3);
            switch (reportExtension)
            {
                case ReportExtension.HTML:
                    var content = "<script>setTimeout(function () { window.print(); }, 1000)</script>" + Encoding.UTF8.GetString(report.FileBody);
                    return Content(content);
                case ReportExtension.XLS:
                    return File(report.FileBody, MediaTypeNames.Application.Octet, report.FileName);
                case ReportExtension.PDF:
                    return File(report.FileBody, MediaTypeNames.Application.Pdf, report.FileName);
                default:
                    return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            //if (disposing && client != null)
            //{
            //    client.Close();
            //    client = null;
            //}

            base.Dispose(disposing);
        }

        [AjaxOnly]
        public async Task<ActionResult> OrderItems(long orderId)
        {
            var order = await orderServiceClient.GetOrderForManager(orderId);

            if (order == null) 
                return NotFound();
            var model = new OrderViewModel(order);
            return PartialView("_OrderItems", model);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string token = User.Identity.GetJwtToken();
            //client = new Food.Services.Proxies.FoodServiceManagerClient()
            //    .AddAuthorization(token);

            ((ReportService)reportServiceClient)?.AddAuthorization(token);
            ((OrderService)orderServiceClient)?.AddAuthorization(token);
            ((BanketsService)_banketsService)?.AddAuthorization(token);
            ((OrderItemService)orderItemServiceClient)?.AddAuthorization(token);
        }

        private async Task<ReportDetailsModel> GetDetailsForOrderAsync(long cafeId, long id, EnumOrderType orderType)
        {
            ReportViewFilter filter = GetReportViewFilter(cafeId);

            var helper = new ReportsHelper(orderServiceClient, _banketsService);
            var model = await helper.GetDetailsForOrder(cafeId, filter, id, orderType);
            
            SetReportViewFilter(filter, cafeId);
            return model;
        }

        private async Task<ReportsModel> GetReportsDataAsync(ReportViewFilter filter, long cafeId)
        {
            var helper = new ReportsHelper(orderServiceClient, _banketsService);
            if (!string.IsNullOrEmpty(filter.SearchQuery.SearchString))
            {
                filter.LoadOrderItems = true;
                filter.LoadUserOrders = true;
                ViewData["filter-data"] = true;
            }
            var model = await helper.GetReportsData(filter, cafeId);

            var cafe = await _cafeService.GetCafeById(cafeId);
            var workingTime = new TimeSpan(
                cafe.WorkingTimeFrom.Value.Hour,
                cafe.WorkingTimeFrom.Value.Minute,
                cafe.WorkingTimeFrom.Value.Second);
            // Расскомментировать, если необходимо в качестве даты поступления указывать дату доставки, если заказ не на сегодня
            //foreach (var order in model.UserOrders)
            //{
            //    if (order.DeliverDate != null && order.DeliverDate.Value > order.Create)
            //        order.Create = order.DeliverDate.Value.Add(workingTime);
            //}

            filter.LoadOrderItems = false;
            filter.LoadUserOrders = false;
            model.Filter.ReportTypes = await reportServiceClient.GetXsltFromCafe(cafeId);            
            SetReportViewFilter(filter, cafeId);

            return model;
        }

        private ReportViewFilter GetReportViewFilter(long cafeId)
        {
            ReportViewFilter filter = HttpContext.Session.Get<ReportViewFilter>("filter-" + cafeId);
            if (filter == null)
                filter = new ReportViewFilter().InitDefaults();

            return filter;
        }

        private void SetReportViewFilter(ReportViewFilter filter, long cafeId)
        {
            HttpContext.Session.Set("filter-" + cafeId, filter);
        }

        private List<OrderViewModel> SortUserOrders(List<OrderViewModel> orders, SortType sortType)
        {
            switch (sortType)
            {
                case SortType.OrderByCafeName:
                    orders = orders.OrderBy(o => o.CafeName).ToList();
                    break;
                case SortType.OrderByPrice:
                    orders = orders.OrderBy(o => o.TotalSum).ToList();
                    break;
                case SortType.OrderByDate:
                    orders = orders.OrderBy(o => o.DeliverDate).ToList();
                    break;
                case SortType.OrderByEmployeeName:
                    orders = orders.OrderBy(o => o.CreatorLogin).ToList();
                    break;
                case SortType.OrderByOrderNumber:
                    orders = orders.OrderBy(o => o.Id).ToList();
                    break;
                case SortType.OrderByStatus:
                    orders = orders.OrderBy(o => o.Status).ToList();
                    break;
            }
            return orders;
        }

        [HttpPost]
        public IActionResult CommentManager(string managerComment, long id, long cafeId, OrderStatusEnum status)
        {
            var result = reportServiceClient.SendManagerComment(managerComment, id);
            if (result.Result == true)
            {
                return RedirectToRoute("ManagerCafe",
                    new { action = "Details", controller = "Reports", id, cafeId, orderType = EnumOrderType.Individual });
            }
            else
                return NotFound();
        }
    }
}
