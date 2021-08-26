using FoodApps.Web.NetCore.Extensions;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.AuthorizationService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Controllers
{
    public class BaseController : Controller
    {
        public BaseController() { }

        public BaseController(ServiceManager manager)
        {
            AccountService = (IAccountService)manager.AccountSevice;
            ProfileClient = (IProfileService)manager.ProfileClient;
            AddressServiceClient = (IAddressesService)manager.AddressServiceClient;
            _banketsService = (IBanketsService)manager.BanketsService;
            _cafeService = manager.CafeService;
            CompanyService = (ICompanyService)manager.CompanyService;
            DishCategoryService = manager.DishCategoryService;
            _dishService = manager.DishService;
            CompanyOrderServiceClient = (ICompanyOrderService)manager.CompanyOrderServiceClient;
            OrderItemServiceClient = (IOrderItemService)manager.OrderItemServiceClient;
            OrderServiceClient = (IOrderService)manager.OrderServiceClient;
            RatingServiceClient = (IRatingService)manager.RatingServiceClient;
            TagServiceClient = (ITagService)manager.TagServiceClient;
            UserServiceClient = (IUsersService)manager.UserServiceClient;
            MenuServiceClient = manager.MenuServiceClient;
        }

        public BaseController(
            IAccountService accountService,
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
            ICityService cityService)
        {
            AccountService = accountService;
            ProfileClient = profileClient;
            AddressServiceClient = addressServiceClient;
            _banketsService = banketsService;
            _cafeService = cafeService;
            CompanyService = companyService;
            DishCategoryService = dishCategoryService;
            _dishService = dishService;
            CompanyOrderServiceClient = companyOrderServiceClient;
            OrderItemServiceClient = orderItemServiceClient;
            OrderServiceClient = orderServiceClient;
            RatingServiceClient = ratingServiceClient;
            TagServiceClient = tagServiceClient;
            UserServiceClient = userServiceClient;
            MenuServiceClient = menuServiceClient;
            CityService = cityService;
        }

        public IAccountService AccountService { get; set; }
        public IProfileService ProfileClient { get; set; }
        public IAddressesService AddressServiceClient { get; set; }
        public IBanketsService _banketsService { get; set; }
        public ICafeService _cafeService { get; set; }
        public ICompanyService CompanyService { get; set; }
        public IDishCategoryService DishCategoryService { get; set; }
        public IDishService _dishService { get; set; }
        public ICompanyOrderService CompanyOrderServiceClient { get; set; }
        public IOrderItemService OrderItemServiceClient { get; set; }
        public IOrderService OrderServiceClient { get; set; }
        public IRatingService RatingServiceClient { get; set; }
        public ITagService TagServiceClient { get; set; }
        public IUsersService UserServiceClient { get; set; }
        public IMenuService MenuServiceClient { get; set; }
        public ICityService CityService { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (AccountService != null)
                {
                    AccountService.Dispose();
                    AccountService = null;
                }

                if (ProfileClient != null)
                {
                    ProfileClient.Dispose();
                    ProfileClient = null;
                }
            }

            base.Dispose(disposing);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var token = User.Identity.GetJwtToken();

            ((BanketsService)_banketsService)?.AddAuthorization(token);
            ((CafeService)_cafeService)?.AddAuthorization(token);
            ((DishService)_dishService)?.AddAuthorization(token);
            ((CompanyService)CompanyService)?.AddAuthorization(token);
            ((UsersService)UserServiceClient)?.AddAuthorization(token);
            ((DishCategoryService)DishCategoryService)?.AddAuthorization(token);
            ((CompanyOrderService)CompanyOrderServiceClient)?.AddAuthorization(token);
            ((AddressesService)AddressServiceClient)?.AddAuthorization(token);
            ((OrderService)OrderServiceClient)?.AddAuthorization(token);
            ((RatingService)RatingServiceClient)?.AddAuthorization(token);
            ((TagService)TagServiceClient)?.AddAuthorization(token);
            ((MenuService)MenuServiceClient)?.AddAuthorization(token);
            ((OrderItemService)OrderItemServiceClient)?.AddAuthorization(token);
            ((AccountService)AccountService)?.AddAuthorization(token);
            ((ProfileService)ProfileClient)?.AddAuthorization(token);

            // true если пользователь не авторизован или является физическим лицом исключительно в роли пользователя
            ViewBag.IsPerson = !User.Identity.IsAuthenticated
                || (!User.IsInRole("Manager")
                && !User.IsInRole("Admin")
                && !User.IsInRole("Consolidator")
                && AsyncHelper.RunSync(() => ProfileClient.GetUserCompanyId()) is null);

            // Если запрос не Ajax, находимся в субдомене, и запрашивается не страница ошибки (чтобы исключить зацикливание).
            if (!Request.IsAjaxRequest() && RouteData.Values.ContainsKey("name") && (filterContext.HttpContext.Request.Path.Value == null ||
                (filterContext.HttpContext.Request.Path.Value != null && !filterContext.HttpContext.Request.Path.Value.ToLowerInvariant().StartsWith("/error"))))
            {
                if (User.IsInRole("Manager") || User.IsInRole("Admin") || User.IsInRole("Consolidator") || AsyncHelper.RunSync(() => ProfileClient.GetUserCompanyId()).HasValue)
                {
                    HttpContext.Session.RemoveCity();
                }

                // Получаем информацию о кафе
                var cafeName = RouteData.Values["name"].ToString();
                var cafe = AsyncHelper.RunSync(() => _cafeService.GetCafeByCleanUrl(cafeName));
                if (!string.IsNullOrEmpty(cafeName))
                {
                    if (cafe != null)
                    {
                        ViewBag.Cafe = cafe;
                        // Получаем меню для кафе
                        var menu = AsyncHelper.RunSync(() => _cafeService.GetMenuPatternsByCafeIdAsync(cafe.Id));
                        // Если есть меню и кафе в данный момент работает
                        if (menu != null && menu.Count > 0 && !cafe.IsClosed())
                        {
                            // Если не аноним
                            //TODO: Авторизация через соцсети. Удалить
                            if (User.Identity.IsAuthenticated && !User.Identity.IsAnonymous()/* && !User.Identity.IsExternal()*/)
                            {
                                // Если не админ
                                if (!User.IsInRole("Admin"))
                                {
                                    var userId = User.Identity.GetUserId<long>();
                                    var userCompany = AsyncHelper.RunSync(() => ProfileClient.GetUserCompanyId());
                                    var schedules = new List<CompanyOrderScheduleModel>();
                                    // Если сотрудник
                                    if (userCompany != null)
                                    {
                                        // Получаем расписания для компании
                                        schedules = AsyncHelper.RunSync(() =>
                                        CompanyOrderServiceClient.GetListOfCompanyOrderScheduleByCompanyId(userCompany.Value));
                                    }

                                    // Если  обычный пользователь и кафе не только для юр. лиц
                                    if (!User.IsInRole("Consolidator") && !User.IsInRole("Manager") && userCompany == null && cafe.CafeType != CafeType.CompanyOnly)
                                    {
                                        return;
                                    }

                                    // Если сотрудник и кафе не только для физ. лиц
                                    if (!User.IsInRole("Consolidator") && !User.IsInRole("Manager") && userCompany != null
                                        && schedules != null && schedules.Count > 0 && cafe.CafeType != CafeType.PersonOnly)
                                    {
                                        return;
                                    }

                                    // Если куратор с компанией и кафе не только для физ. лиц
                                    if (User.IsInRole("Consolidator") && userCompany != null &&
                                        schedules != null && schedules.Count > 0 && cafe.CafeType != CafeType.PersonOnly)
                                    {
                                        return;
                                    }

                                    // Если менеджер
                                    if (User.IsInRole("Manager") && AsyncHelper.RunSync(() => _cafeService.GetManagedCafes()).Select(c => c.Id).Contains(cafe.Id))
                                    {
                                        return;
                                    }
                                }
                            }
                            // Если аноним и кафе не только для юр. лиц
                            else if (cafe.CafeType != CafeType.CompanyOnly)
                            {
                                return;
                            }
                        }

                        var city = HttpContext.Session.GetCurrentCity();
                        if (city != null && cafe.CityId != city.Id)
                        {
                            filterContext.HttpContext.Response.StatusCode = 404;
                            filterContext.Result = View("CafeNotAvailableInCity");
                            return;
                        }

                        // Очищаем если кафе недостуно
                        ViewBag.Cafe = null;
                        // Если предыдущий запрос из AccountController
                        var refererString = filterContext.HttpContext.Request.Headers["Referer"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(refererString))
                        {
                            var refererUri = new Uri(refererString);
                            if (refererUri.LocalPath.ToLowerInvariant().StartsWith("/account"))
                            {
                                // Создаем ссылку на страницу ошибки без субдомена.
                                var hostBuilder = new StringBuilder();
                                // Добавляем http или https.
                                hostBuilder.Append($"{HttpContext.Request.Scheme}://");
                                var config = HttpContext.RequestServices?.GetRequiredService<IConfiguration>();
                                var domainName = config?.GetSection("AppSettings:Domain")?.Value.ToLowerInvariant();
                                // Добавляем домен.
                                hostBuilder.Append(domainName);
                                // Добавляем порт при наличии.
                                if (HttpContext.Request.Host.Port != null)
                                    hostBuilder.Append(":").Append(HttpContext.Request.Host.Port.Value.ToString());
                                filterContext.Result = Redirect(hostBuilder.ToString());
                                return;
                            }
                        }

                        #region old
                        //// Получаем меню для кафе
                        //var menu = AsyncHelper.RunSync(() => _cafeService.GetMenuPatternsByCafeIdAsync(cafe.Id));
                        //// Если кафе существует, активно, работает в данный момент, есть меню и при этом пользователь не менеджер
                        //if (((cafe == null || !cafe.IsActive || cafe.IsClosed() || menu == null || menu.Count == 0) && !User.IsInRole("Manager"))
                        //    // Или если пользователь менеджер и является менеджером текущего кафе
                        //    || (User.IsInRole("Manager") && currentCafe != null
                        //    && !AsyncHelper.RunSync(() => _cafeService.GetManagedCafes()).Select(c => c.Id).Contains(currentCafe.Value))
                        //    || currentCafe == null)
                        //{
                        //    // Отменяем действия и возвращаем NotFound
                        //    filterContext.Result = NotFound();
                        //    return;
                        //}
                        //// Если пользователь авторизован, не анонимный и не внешний
                        //if (User.Identity.IsAuthenticated && !User.Identity.IsAnonymous() && !User.Identity.IsExternal())
                        //{
                        //    // Если пользователь не менеджер
                        //    if (!User.IsInRole("Manager"))
                        //    {
                        //        var userId = User.Identity.GetUserId<long>();
                        //        var userCompany = AsyncHelper.RunSync(() => ProfileClient.GetUserCompanyId());
                        //        var schedules = new List<CompanyOrderScheduleModel>();
                        //        // Если сотрудник
                        //        if (userCompany != null)
                        //        {
                        //            // Получаем расписания для компании
                        //            schedules = AsyncHelper.RunSync(() =>
                        //            CompanyOrderServiceClient.GetListOfCompanyOrderScheduleByCompanyId(userCompany.Value));
                        //        }
                        //        // Список идентификаторов кафе доступных пользователю
                        //        var cafes = AsyncHelper.RunSync(() => _cafeService.GetCafesToUser(userId))?.Select(c => c.Id) ?? new List<long>();
                        //        // Если кафе недоступно
                        //        if ((currentCafe != null && !cafes.Contains(currentCafe.Value))
                        //            // Или если кафе только для компаний и пользователь не куратор и не сотрудник
                        //            || (((cafe.CafeType == CafeType.CompanyOnly && !User.IsInRole("Consolidator") && userCompany == null)
                        //            // Или если кафе только для физ. лиц и пользователь - куратор или сотрудник
                        //            || (cafe.CafeType == CafeType.PersonOnly && (User.IsInRole("Consolidator") || userCompany != null)))
                        //            // И отсутствует расписание для компании в текущем кафе
                        //            && (schedules == null && schedules.Count == 0 && schedules.Any(s => s.CafeId == currentCafe)))
                        //            // Или пользователь - админ
                        //            || User.IsInRole("Admin"))
                        //        {
                        //            // Отменяем действия и возвращаем NotFound
                        //            filterContext.Result = NotFound();
                        //            return;
                        //        }
                        //    }
                        //}
                        //// Если пользователь не авторизован
                        //else
                        //{
                        //    // Если кафе нет или если кафе только для компаний
                        //    if (cafe == null || (cafe != null && cafe.CafeType == CafeType.CompanyOnly))
                        //    {
                        //        // Отменяем действия и возвращаем NotFound
                        //        filterContext.Result = NotFound();
                        //        return;
                        //    }
                        //}
                        #endregion
                    }

                    // Отменяем действия и возвращаем NotFound
                    filterContext.HttpContext.Response.StatusCode = 404;
                    filterContext.Result = View("~/Views/Error/NotFound.cshtml");
                    return;
                }
            }
        }

        protected async Task<MenuViewModel> GetMenuModel(DateTime? date)
        {
            var model = new MenuViewModel();
            var cafe = (CafeModel)ViewBag.Cafe;
            var menu = new Dictionary<FoodCategoryModel, List<FoodDishModel>>();
            var bankets = await ProfileClient.GetAvailableBankets(cafe.Id);
            var banket = bankets.OrderBy(c => c.EventDate).FirstOrDefault();
            if ((string)ControllerContext.RouteData.Values["controller"] ==
                nameof(BanketController).Replace("Controller", string.Empty).ToLower() && banket != null)
                menu = await _cafeService.GetMenuByPatternIdAsync(cafe.Id, banket.MenuId);
            else
                menu = await MenuServiceClient.GetDishesByScheduleByDate(cafe.Id, date ?? DateTime.Now);

            model.Menu = menu;
            model.Banket = banket;
            var clientDiscount = await _cafeService.GetDiscountAmount(cafe.Id, DateTime.Now, null);
            model.ClientDiscount = clientDiscount;
            ViewBag.ClientDiscount = clientDiscount;
            model.Cafe = cafe;

            return model;
        }

        protected void SetModelErrors(HttpResult result)
        {
            if (result.Succeeded)
                return;

            foreach (var item in result.Errors) ModelState.AddModelError(string.Empty, item);
        }
    }
}