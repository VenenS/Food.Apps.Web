using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CompanyOrderModel = ITWebNet.Food.Site.Models.CompanyOrderModel;
using OrderModel = ITWebNet.Food.Core.DataContracts.Common.OrderModel;

namespace ITWebNet.Food.Site.Areas.Curator.Controllers
{
    public class ReportsController : BaseController
    {
        private ICompanyService companyService;
        private IReportService reportServiceClient;
        private IOrderService orderServiceClient;
        private ICafeService cafeServiceClient;

        public ReportsController(
            ICompanyService companyService, 
            IReportService reportServiceClient, 
            IOrderService orderServiceClient, 
            ICafeService cafeServiceClient)
        {
            this.companyService = companyService;
            this.reportServiceClient = reportServiceClient;
            this.orderServiceClient = orderServiceClient;
            this.cafeServiceClient = cafeServiceClient;
        }

        public async Task<ActionResult> Index()
        {
            var company = await companyService.GetCuratedCompany();
            ReportsModel model = new ReportsModel();
            ReportViewFilter filter = new ReportViewFilter();

            if (company != null)
            {
                filter = GetReportViewFilter(company.Id);
                filter.Company = company;
                model = await GetReportsDataAsync(filter, company.Id);
            }
            else
            {
                model.Filter = filter;
            }

            return View(model);
        }
        
        [AjaxOnly]
        public async Task<ActionResult> UserOrders(long companyOrderId)
        {
            var orders = await orderServiceClient.GetUserOrdersInCompanyOrder(companyOrderId);
            var models = OrderViewModel.AsModelList(orders);
            ViewData["collapseScope"] = "#company-order-" + companyOrderId;

            return PartialView("_UserReports", models);
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

        public async Task<ActionResult> Details(long cafeId, long companyId, long id, EnumOrderType orderType)
        {
            ModelState.Clear();

            ReportDetailsModel model = await GetDetailsForOrderAsync(cafeId, companyId, id, orderType);
            var cafe = await cafeServiceClient.GetCafeById(cafeId);
            ViewBag.CafeName = cafe.FullName;

            if (model.CompanyOrder == null && model.UserOrder == null)
                return NoContent();

            return View(model);
        }

        #region Получение списка заказов
        public async Task<ReportsModel> GetReportsDataAsync(ReportViewFilter filter, long companyId, long? cafeId = null)
        {
            ReportsModel model = new ReportsModel();
            TempData["CompanyId"] = companyId;

            bool isCompaniesSelected = filter.CompanyOrders != null && filter.CompanyOrders.Count > 0;
            var isUsersSelected = filter.UserOrders != null && filter.UserOrders.Count > 0 && filter.IsCustomer &&
                                  cafeId.HasValue;

            bool loadCompanyOrders = filter.IsCompany || isCompaniesSelected;

            var reportFilter = filter.AsReportFilter(null, companyId);

            if (loadCompanyOrders)
            {
                if (!string.IsNullOrEmpty(filter.SearchQuery.SearchString))
                {
                    reportFilter.LoadUserOrders = true;
                    reportFilter.LoadOrderItems = true;
                    ViewData["filter-data"] = true;
                }
                var companyOrders = await orderServiceClient.GetCompanyOrderByFilters(reportFilter);
                if (companyOrders != null)
                    model.CompanyOrders = CompanyOrderModel.AsModelList(companyOrders);
                reportFilter.LoadUserOrders = false;
                reportFilter.LoadOrderItems = false;
                foreach (var item in model.CompanyOrders)
                {
                    item.AvailableStatuses = GetAvailableOrderStatuses(item.OrderStatus);

                    foreach (var order in item.UserOrders)
                    {
                        order.AvailableStatuses = GetAvailableOrderStatuses(order.Status);
                    }
                }
            }


            if (isUsersSelected)
            {
                reportFilter.CafeId = cafeId.Value;
                var userOrder = await orderServiceClient.GetOrderByFilters(reportFilter);

                model.UserOrders =
                    OrderViewModel.AsModelList(new List<OrderModel>() { userOrder.First() });

                foreach (var item in model.UserOrders)
                {
                    item.AvailableStatuses = GetAvailableOrderStatuses(item.Status);
                }
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery.SearchString))
            {
                var workingSearchString = filter.SearchQuery.SearchString.Trim().ToLower();
                switch (filter.SearchQuery.Type)
                {
                    case SearchType.SearchByName:
                        model.CompanyOrders =
                            model.CompanyOrders.Where(
                                co =>
                                    co.UserOrders.Any(
                                        c =>
                                            !string.IsNullOrEmpty(c.Creator.UserFullName) &&
                                            c.Creator.UserFullName.ToLower()
                                                .Contains(workingSearchString)))
                                .ToList();
                        for (int i = 0; i < model.CompanyOrders.Count; i++)
                        {
                            var privateOrders = model.CompanyOrders.ElementAt(i).UserOrders.Where(c =>
                                !string.IsNullOrEmpty(c.Creator.UserFullName) &&
                                c.Creator.UserFullName.ToLower()
                                    .Contains(workingSearchString));

                            model.CompanyOrders.ElementAt(i).UserOrders = privateOrders.ToList();
                        }
                        break;
                    case SearchType.SearchByDish:
                        model.CompanyOrders =
                            model.CompanyOrders.Where(
                                co =>
                                    co.UserOrders.Any(
                                        c =>
                                            c.OrderItems.Any(
                                                i =>
                                                    !string.IsNullOrEmpty(i.DishName) &&
                                                    i.DishName.ToLower()
                                                        .Contains(workingSearchString)))).ToList();
                        for (int i = 0; i < model.CompanyOrders.Count; i++)
                        {
                            var privateOrders = model.CompanyOrders.ElementAt(i).UserOrders.Where(c =>
                                            c.OrderItems.Any(
                                                k =>
                                                    !string.IsNullOrEmpty(k.DishName) &&
                                                    k.DishName.ToLower()
                                                        .Contains(workingSearchString)));

                            model.CompanyOrders.ElementAt(i).UserOrders = privateOrders.ToList();
                        }
                        break;
                    case SearchType.SearchByPhone:
                        model.CompanyOrders =
                            model.CompanyOrders.Where(
                                co =>
                                    co.UserOrders.Any(
                                        c =>
                                            (!string.IsNullOrEmpty(c.PhoneNumber) &&
                                             c.PhoneNumber.ToLower()
                                                 .Contains(workingSearchString)) ||
                                            (!string.IsNullOrEmpty(c.Creator.PhoneNumber) &&
                                             c.Creator.PhoneNumber.ToLower()
                                                 .Contains(workingSearchString))))
                                .ToList();
                        for (int i = 0; i < model.CompanyOrders.Count; i++)
                        {
                            var privateOrders = model.CompanyOrders.ElementAt(i).UserOrders.Where(c =>
                                           (!string.IsNullOrEmpty(c.PhoneNumber) &&
                                            c.PhoneNumber.ToLower()
                                                .Contains(workingSearchString)) ||
                                           (!string.IsNullOrEmpty(c.Creator.PhoneNumber) &&
                                            c.Creator.PhoneNumber.ToLower()
                                                .Contains(workingSearchString)));

                            model.CompanyOrders.ElementAt(i).UserOrders = privateOrders.ToList();
                        }
                        break;
                    case SearchType.SearchByOrderNumber:
                        long num;
                        if (long.TryParse(workingSearchString, out num))
                        {
                            var corpOrders = model.CompanyOrders.Where(o => o.Id == num).ToList();
                            var corpIds = corpOrders.Select(c => c.Id).ToList();
                            List<OrderViewModel> privateOrders = new List<OrderViewModel>();
                            for (var i = 0; i < model.CompanyOrders.Count; i++)
                            {
                                if (!corpIds.Contains(model.CompanyOrders.ElementAt(i).Id))
                                {
                                    privateOrders =
                                        model.CompanyOrders.ElementAt(i).UserOrders.Where(o => o.Id == num).ToList();
                                    model.CompanyOrders.ElementAt(i).UserOrders = privateOrders;
                                }
                            }

                            model.CompanyOrders =
                                model.CompanyOrders.Where(c => c.UserOrders.Count > 0 || corpIds.Contains(c.Id))
                                    .ToList();
                        }
                        else
                        {
                            model = new ReportsModel();
                        }
                        break;
                    case SearchType.SearchByCafe:
                        model.CompanyOrders =
                            model.CompanyOrders.Where(c => c.Cafe.FullName.ToLower().Contains(workingSearchString))
                                .ToList();
                        break;
                }
            }

            switch (filter.SortType)
            {
                case SortType.OrderByDate:
                    for (int i = 0; i < model.CompanyOrders.Count; i++)
                    {
                        var privateOrders = model.CompanyOrders.ElementAt(i).UserOrders.OrderBy(o => o.DeliverDate);
                        model.CompanyOrders.ElementAt(i).UserOrders = privateOrders.ToList();
                    }
                    model.CompanyOrders =
                        model.CompanyOrders.OrderBy(item => item.OrderAutoCloseDate).ToList();
                    break;
                case SortType.OrderByStatus:
                    for (int i = 0; i < model.CompanyOrders.Count; i++)
                    {
                        var privateOrders = model.CompanyOrders.ElementAt(i).UserOrders.OrderBy(o => o.Status);
                        model.CompanyOrders.ElementAt(i).UserOrders = privateOrders.ToList();
                    }
                    model.CompanyOrders =
                        model.CompanyOrders.OrderBy(item => item.OrderStatus).ToList();
                    break;
                case SortType.OrderByPrice:
                    for (int i = 0; i < model.CompanyOrders.Count; i++)
                    {
                        var privateOrders = model.CompanyOrders.ElementAt(i).UserOrders.OrderBy(o => o.TotalSum);
                        model.CompanyOrders.ElementAt(i).UserOrders = privateOrders.ToList();
                    }
                    model.CompanyOrders =
                        model.CompanyOrders.OrderBy(item => item.UserOrders.Sum(o => o.TotalSum)).ToList();
                    break;
                case SortType.OrderByOrderNumber:
                    for (int i = 0; i < model.CompanyOrders.Count; i++)
                    {
                        var privateOrders = model.CompanyOrders.ElementAt(i).UserOrders.OrderBy(o => o.Id);
                        model.CompanyOrders.ElementAt(i).UserOrders = privateOrders.ToList();
                    }
                    model.CompanyOrders =
                        model.CompanyOrders.OrderBy(item => item.Id).ToList();
                    break;
                case SortType.OrderByCafeName:
                    model.CompanyOrders =
                        model.CompanyOrders.OrderBy(item => item.Cafe.FullName).ToList();
                    break;
                default:
                    break;
            }

            filter.IsCompany = true;
            filter.IsCustomer = true;
            model.Filter = filter;

            SetReportViewFilter(filter, companyId);

            return model;
        }
        #endregion

        [AjaxOnly, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpPost]
        public async Task<ActionResult> Filter(ReportViewFilter filter, long companyId)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (filter.SortType == SortType.OrderByEmployeeName)
            {
                //Получение заказов. Группировка по сотрудникам
                var model = await reportServiceClient.GetEmployeesReport(filter.AsReportFilter(null, companyId));
                TempData["CompanyId"] = companyId;
                return PartialView("_EmployeeReports", model);
            }
            else
            {
                ReportsModel model = await GetReportsDataAsync(filter, companyId);

                return PartialView("_CompanyReports", model.CompanyOrders);
            }
        }

        private async Task<ReportDetailsModel> GetDetailsForOrderAsync(long cafeId, long companyId, long id, EnumOrderType orderType)
        {
            ReportDetailsModel model = new ReportDetailsModel();
            var company = await companyService.GetCuratedCompany();

            if (company != null)
            {
                ReportViewFilter filter = GetReportViewFilter(companyId);

                ReportsModel data;



                switch (orderType)
                {
                    case EnumOrderType.Collective:
                        filter.CompanyOrders = new List<long> {id};
                        filter.IsCompany = true;
                        filter.IsCustomer = false;
                        filter.LoadUserOrders = true;
                        data = await GetReportsDataAsync(filter, companyId);
                        filter.LoadUserOrders = false;
                        filter.CompanyOrders = null;

                        if (data.CompanyOrders.Count == 1)
                        {
                            var order = data.CompanyOrders.First();

                            if (order.OrderAutoCloseDate.HasValue && filter.Start.Equals(DateTime.MinValue))
                                filter.Start = order.OrderAutoCloseDate.Value;

                            model.CompanyOrder = order;
                        }
                        break;

                    case EnumOrderType.Individual:
                        filter.UserOrders = new List<long> {id};
                        filter.IsCustomer = true;
                        filter.IsCompany = false;
                        data = await GetReportsDataAsync(filter, companyId, cafeId);

                        if (data.UserOrders.Count == 1)
                        {
                            var order = data.UserOrders.First();

                            if (filter.Start.Equals(DateTime.MinValue))
                                filter.Start = order.Create;

                            model.UserOrder = order;
                        }
                        break;
                }
            }

            return model;
        }

        [AjaxOnly]
        public async Task<ActionResult> CancelOrder(long id, long cafeId, long companyId, EnumOrderType orderType)
        {
            ReportDetailsModel model;
            switch (orderType)
            {
                case EnumOrderType.Individual:
                    await orderServiceClient.ChangeOrderStatus(cafeId, companyId, id, OrderStatusEnum.Abort);
                    ModelState.Clear();

                    model = await GetDetailsForOrderAsync(cafeId, companyId, id, orderType);

                    if (model.UserOrder == null)
                        return NotFound();

                    return PartialView("_UserDetails", model.UserOrder);
                    break;
                case EnumOrderType.Collective:
                    await orderServiceClient.ChangeCompanyOrderStatus(cafeId, OrderStatusEnum.Abort, companyId, id);
                    ModelState.Clear();

                    model = await GetDetailsForOrderAsync(cafeId, companyId, id, orderType);

                    if (model.CompanyOrder == null)
                        return NotFound();

                    return PartialView("_CompanyDetails", model.CompanyOrder);
                    break;
                default:
                    return NoContent();
            }
        }

        private ReportViewFilter GetReportViewFilter(long companyId)
        {
            ReportViewFilter filter = HttpContext.Session.Get<ReportViewFilter>("filter-company-" + companyId);
            if (filter == null)
                filter = new ReportViewFilter().InitDefaults();

            return filter;
        }

        private List<OrderStatusEnum> GetAvailableOrderStatuses(OrderStatusEnum status)
        {
            var orderStatuses = new List<OrderStatusEnum>();
            switch (status)
            {
                case OrderStatusEnum.Created:
                    {
                        orderStatuses.Add(OrderStatusEnum.Accepted);
                        orderStatuses.Add(OrderStatusEnum.Abort);
                    }
                    break;
                case OrderStatusEnum.Accepted:
                    {
                        orderStatuses.Add(OrderStatusEnum.Delivery);
                        orderStatuses.Add(OrderStatusEnum.Abort);
                        orderStatuses.Add(OrderStatusEnum.Delivered);
                    }
                    break;
                case OrderStatusEnum.Delivery:
                    {
                        orderStatuses.Add(OrderStatusEnum.Abort);
                        orderStatuses.Add(OrderStatusEnum.Delivered);
                    }
                    break;
                default:
                    break;
            }

            return orderStatuses;
        }

        private void SetReportViewFilter(ReportViewFilter filter, long companyId)
        {
            HttpContext.Session.Set("filter-company-" + companyId, filter);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var token = User.Identity.GetJwtToken();
            ((CompanyService)companyService)?.AddAuthorization(token);
            ((ReportService)reportServiceClient)?.AddAuthorization(token);
            ((OrderService)orderServiceClient)?.AddAuthorization(token);
            ((CafeService)cafeServiceClient)?.AddAuthorization(token);
        }
    }
}