using FoodApps.Web.NetCore.Extensions;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Areas.Administrator.Controllers
{
    public class ReportsController : BaseController
    {
        private IReportService reportServiceClient;
        private IOrderService orderServiceClient;
        private IBanketsService _banketsService;
        private ICafeService _cafeService;

        public ReportsController(
            IReportService reportServiceClient, 
            IOrderService orderServiceClient, 
            IBanketsService banketsService, 
            ICafeService cafeService)
        {
            this.reportServiceClient = reportServiceClient;
            this.orderServiceClient = orderServiceClient;
            _banketsService = banketsService;
            _cafeService = cafeService;
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult> Index(long cafeId)
        {
            ModelState.Clear();

            ReportViewFilter filter = GetReportViewFilter(cafeId);

            ReportsModel model = await GetReportsDataAsync(filter, cafeId);

            ViewBag.CafeId = cafeId;
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
        public async Task<ActionResult> UserOrders(long companyOrderId)
        {
            ViewBag.CompanyOrderId = companyOrderId;
            var orders = await orderServiceClient.GetUserOrdersInCompanyOrder(companyOrderId);
            var models = OrderViewModel.AsModelList(orders);

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
        
        
        [HttpPost, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult> Filter(ReportViewFilter filter, long cafeId)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            ReportsModel model = await GetReportsDataAsync(filter, cafeId);

            return View("Index", model);
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
            filter.LoadOrderItems = false;
            filter.LoadUserOrders = false;
            SetReportViewFilter(filter, cafeId);

            return model;
        }

        private ReportViewFilter GetReportViewFilter(long cafeId)
        {
            ReportViewFilter filter = HttpContext.Session.Get<ReportViewFilter>("admin-filter-" + cafeId);
            if (filter == null)
                filter = new ReportViewFilter().InitDefaults();

            return filter;
        }

        private void SetReportViewFilter(ReportViewFilter filter, long cafeId)
        {
            HttpContext.Session.Set("admin-filter-" + cafeId, filter);
        }
        
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string token = User.Identity.GetJwtToken();

            ((ReportService)reportServiceClient)?.AddAuthorization(token);
            ((OrderService)orderServiceClient)?.AddAuthorization(token);
            ((BanketsService)_banketsService)?.AddAuthorization(token);
            ((CafeService)_cafeService)?.AddAuthorization(token);

            if (!filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.RouteData.Values.ContainsKey("cafeId"))
            {
                long cafeId = long.Parse(filterContext.RouteData.Values["cafeId"].ToString());
                CafeModel cafe = AsyncHelper.RunSync(() => _cafeService.GetCafeById(cafeId));
                ViewBag.Cafe = cafe;
            }
        }
    }
}