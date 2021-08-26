using Food.Services.Models;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Hubs;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompanyOrderModel = ITWebNet.Food.Core.DataContracts.Common.CompanyOrderModel;
using OrderModel = ITWebNet.Food.Core.DataContracts.Common.OrderModel;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Http;
using ITWebNet.Food.Site.Services.AuthorizationService;
using Microsoft.Extensions.DependencyInjection;

namespace ITWebNet.Food.Site.Controllers
{
    //[ViewComponent(Name = "Cart")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class CartController : BaseController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private ICafeOrderNotificationService _cafeOrderNotificationClient;

        public const string StandardDateFormat = "yyyy-MM-dd";
        public CartMultiCafeViewModel _cartMulti { get; set; }

        public CartController(IHttpContextAccessor httpContextAccessor) : base()
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [ActivatorUtilitiesConstructor]
        public CartController(
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
            IHttpContextAccessor httpContextAccessor,
            ICafeOrderNotificationService cafeOrderNotificationService) :
            base(accountSevice, profileClient, addressServiceClient,
                banketsService, cafeService, companyService, dishCategoryService,
                dishService, companyOrderServiceClient, orderItemServiceClient,
                orderServiceClient, ratingServiceClient, tagServiceClient,
                userServiceClient, menuServiceClient, cityService)
        {
            _httpContextAccessor = httpContextAccessor;
            _cafeOrderNotificationClient = cafeOrderNotificationService;
        }

        public static string GetDishCase(int count)
        {
            string casedDishesVal = DeclensionHelper.GetNoun(count, "блюдо", "блюда", "блюд");

            return casedDishesVal;
        }

        [AjaxOnly]
        public async Task<ActionResult> AddDish(FoodDishModel dish, long cafeId, EnumCartType cartType, DateTime? d, bool preOrder = false)
        {

            //Если пользователь не авторизован, возвращаем пустую корзину
            //Закомментировал т. к. редирект должен быть после перехода к оформлению заказа
            //if (User.Identity.IsAnonymous())
            //    return Content($"window.location = '{Url.Action("Login", "Account", new { returnUrl = "/" })}'", "text/javascript");

            // Проверить можно ли добавить товар в корзину.
            //
            // * Если у пользователя нет компаний, всега добавлять товар в корзину.
            // * Если пользователь является сотрудником компании, убедиться что выбранная компания
            //   обслуживается тем кафе, от которого идет новое блюдо.
            long? currentCompany = null;
            CompanyServersModel currentCompanyCafes = null;
            if (!User.Identity.IsAnonymous())
            {
                var orderDate = d != null ? d.Value.Date : DateTime.Now.Date;
                var servingCafes = await _cafeService.GetCafesAvailableToEmployee(new List<DateTime> { orderDate });
                currentCompany = await ProfileClient.GetUserCompanyId();
                currentCompanyCafes = servingCafes.FirstOrDefault(x => x.CompanyId == currentCompany && x.Date == orderDate);
            }

            if (currentCompany == null || (currentCompany != null && (currentCompanyCafes?.ServingCafes?.Contains(dish.CafeId) != null)))
            {
                // Получаем карт для данного кафе. Если её нет - делаем новую:
                CartViewModel cart;
                if (_cartMulti.CafeCarts.ContainsKey(cafeId))
                {
                    cart = _cartMulti.CafeCarts[cafeId];
                }
                else
                {
                    cart = new CartViewModel();
                    cart.Type = _cartMulti.Type;
                    _cartMulti.CafeCarts.Add(cafeId, cart);
                }
                if (cart.Cafe == null)
                {
                    var cafe = await _cafeService.GetCafeById(cafeId);
                    if (cafe.IsClosed() && !preOrder)
                    {
                        return Content("$('#cafeIsClosedModal').modal();", "text/javascript");
                    }
                    cart.Cafe = cafe;
                    cart.ClientDiscount =
                        await _cafeService.GetDiscountAmount(cart.Cafe.Id, DateTime.Now);
                }

                var cartItem = (cart.Cafe.WeekMenuIsActive || cart.Cafe.DeferredOrder) && d.HasValue && d.Value != DateTime.MinValue && cart.Type != EnumCartType.Banket
                    ? cart.CartItems.FirstOrDefault(c => c.Id == dish.Id && c.DeliveryDate.Date == d.Value.Date)
                    : cart[dish.Id];
                if (cartItem != null)
                {
                    if (cartItem.Count < 99)
                        cartItem.Count++;
                }
                else
                {
                    if (!(cart.Cafe.WeekMenuIsActive || cart.Cafe.DeferredOrder) && d.HasValue && d.Value.Date != DateTime.Now.Date)
                    {
                        return GetCartList(cartType, _cartMulti);
                    }

                    cart.CartItems.Add(CartItemViewModel.FromDish(dish, cart.ClientDiscount, d));
                }
                if (d.HasValue)
                {
                    ViewBag.Date = d;
                }
            }

            return GetCartList(cartType, _cartMulti);
        }

        private CartItemViewModel GetCafeCartItem(long CafeId, long DishId, DateTime? d)
        {
            CartItemViewModel cartItem = null;
            if (_cartMulti.CafeCarts.ContainsKey(CafeId))
            {
                CartViewModel cafeCart = _cartMulti.CafeCarts[CafeId];
                if (cafeCart.Cafe != null)
                {
                    cartItem = cafeCart.Cafe != null &&
                        (cafeCart.Cafe.WeekMenuIsActive || cafeCart.Cafe.DeferredOrder) && d.HasValue
                        ? cafeCart.CartItems.FirstOrDefault(c => c.Id == DishId && c.DeliveryDate.Date == d.Value.Date)
                        : cafeCart[DishId];
                }
            }
            return cartItem;
        }

        private ActionResult RemoveCartItem(long CafeId, EnumCartType cartType, CartItemViewModel cartItem)
        {

            if (_cartMulti.CafeCarts.ContainsKey(CafeId) && cartItem != null)
            {
                CartViewModel cafeCart = _cartMulti.CafeCarts[CafeId];
                cafeCart.CartItems.Remove(cartItem);
                if (cafeCart.Count <= 0)
                    _cartMulti.CafeCarts.Remove(CafeId);

                if (_cartMulti.CafeCarts.Count <= 0)
                    return Ok();
            }
            return GetCartItem(CafeId, cartType, null);
        }

        [AjaxOnly]
        public ActionResult RemoveDish(long CafeId, long DishId, EnumCartType cartType, DateTime? d)
        {
            return RemoveCartItem(CafeId, cartType, GetCafeCartItem(CafeId, DishId, d));
        }

        [AjaxOnly]
        public ActionResult ChangeDishCount(long CafeId, long DishId, EnumCartType cartType, string count, DateTime? d)
        {
            CartItemViewModel cartItem = GetCafeCartItem(CafeId, DishId, d);
            if (cartItem == null)
                return GetCartItem(CafeId, cartType, null);
            if (!int.TryParse(count, out var result))
                return GetCartItem(CafeId, cartType, cartItem);
            cartItem.Count = (result <= 99) ? result : 99;

            return cartItem.Count <= 0
                ? RemoveCartItem(CafeId, cartType, cartItem)
                : GetCartItem(CafeId, cartType, cartItem);
        }

        [AjaxOnly]
        public ActionResult IncreaseDishCount(long CafeId, long DishId, EnumCartType cartType, DateTime? d)
        {
            CartItemViewModel cartItem = GetCafeCartItem(CafeId, DishId, d);
            if (cartItem != null && cartItem.Count < 99)
            {
                cartItem.Count++;
            }
            //return GetCartItem(cartType, cartItem); //11554
            return GetCartItem(CafeId, cartType, cartItem);
        }

        [AjaxOnly]
        public ActionResult DecreaseDishCount(long CafeId, long DishId, EnumCartType cartType, DateTime? d)
        {
            CartItemViewModel cartItem = GetCafeCartItem(CafeId, DishId, d);
            if (cartItem != null)
            {
                --cartItem.Count;

                if (cartItem.Count <= 0)
                    return RemoveCartItem(CafeId, cartType, cartItem);
            }
            //return GetCartItem(cartType, cartItem); //11554
            return GetCartItem(CafeId, cartType, cartItem);
        }

        [AjaxOnly]
        public ActionResult SetComment(long CafeId, long DishId, EnumCartType cartType, string comment, DateTime? d)
        {
            CartItemViewModel cartItem = GetCafeCartItem(CafeId, DishId, d);
            if (cartItem != null)
                cartItem.Comment = comment;
            return GetCartItem(CafeId, cartType, cartItem);
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public ActionResult GetCart(EnumCartType cartType, long cafeId = -1)
        {
            if (cartType == EnumCartType.Nav)
                AsyncHelper.RunSync(RecalculateCart);

            if (cafeId >= 0)
                _cartMulti.CurrentCafeId = cafeId;

            ClearCartByUserCompany();
            return GetCartList(cartType, _cartMulti);
        }

        [AjaxOnly]
        public async Task<ActionResult> UpdateCompanyAddress(bool isCompanyOrder)
        {
            var switchCompany = await GetCompanyForOrder();
            DeliveryAddressViewModel model = new DeliveryAddressViewModel()
            {
                CompanyAddresses = switchCompany.Addresses,
                CompanyName = switchCompany.Name,
            };

            return PartialView("_DeliveryAddress", model);
        }

        [AjaxOnly, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public ActionResult GetTotals(EnumCartType cartType)
        {
            AsyncHelper.RunSync(RecalculateCart);
            if (cartType == EnumCartType.Nav) cartType = EnumCartType.Short;

            switch (cartType)
            {
                case EnumCartType.Full:
                    return PartialView("_Totals", _cartMulti);

                case EnumCartType.Short:
                case EnumCartType.DeliveryCost:
                case EnumCartType.Banket:
                    return PartialView("_SideBarTotals", _cartMulti);
                default:
                    return BadRequest("Invalid Cart Type");
            }
        }

        /// <summary>
        /// Метод для обновления стоимости доставки. Применяется для заказов "на неделю" и/или с отложенной доставкой, у которых в одном заказе может быть доставка на разные даты
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе</param>
        /// <param name="date">Дата, на которую назначена доставка заказа</param>
        /// <returns></returns>
        [AjaxOnly]
        [HttpGet]
        public async Task<ActionResult> GetDeliveryCostForDate(long cafeId, string date)
        {
            if (!_cartMulti.CafeCarts.ContainsKey(cafeId))
                return NotFound();
            CartViewModel cart = _cartMulti.CafeCarts[cafeId];
            if (!DateTime.TryParseExact(date, StandardDateFormat, Thread.CurrentThread.CurrentCulture, System.Globalization.DateTimeStyles.AssumeLocal, out var dateOfOrder))
                return BadRequest($"Invalid date format, should be \"{StandardDateFormat}\"");
            double priceForDate = cart.CartItems.Where(c => c.DeliveryDate.Date == dateOfOrder.Date)
                .Select(ci => ci.TotalPrice).DefaultIfEmpty().Sum();

            var companyId = await ProfileClient.GetUserCompanyId();
            OrderDeliveryPriceModel delivery =
                    !cart.IsCompanyEmployee || companyId == null || _httpContextAccessor.HttpContext.Session.GetCurrentDeliveryAddress() == null
                        ? await _cafeService.GetCostOfDeliveryAsync(cafeId, priceForDate, cart.IsCompanyEmployee, date)
                        : await _cafeService.GetShippingCostToSingleCompanyAddress(_httpContextAccessor.HttpContext.Session.GetCurrentDeliveryAddress(),
                                                                                   companyId.Value,
                                                                                   cafeId,
                                                                                   priceForDate,
                                                                                   dateOfOrder);

            return PartialView("_SideBarDeliveryCost", delivery);
        }

        public async Task<ActionResult> Index(string name)
        {
            if (User.IsInRole("Manager") || User.IsInRole("Admin")) return Redirect("/");
            if (User.Identity.IsAnonymous())
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Cart", new { name }) });

            ClearCartByUserCompany();
            if (_cartMulti == null || _cartMulti.CafeCarts == null || _cartMulti.CafeCarts.Count == 0 || !_cartMulti.CafeCarts.Any(c => c.Value.CartItems.Count > 0))
                return Redirect("/");

            var viewModel = new OrderMultiCafeViewModel();

            DeliveryInfoViewModel deliveryInfoModel = new DeliveryInfoViewModel()
            {
                PhoneNumber = User.Identity.GetUserPhone(),
                Email = User.Identity.GetUserEmail(),
                ConfirmAgreement = false
            };

            //Адрес доставки
            deliveryInfoModel.Address = new DeliveryAddressViewModel();

            //TODO: Для отложенного заказа нужно проверить, чтобы на каждую дату заказа был корпоративный заказ
            //Проверить PreOrder
            //Думаю переделать GetCompanyForOrder и вместо одной даты передавать Массив дат из заказов

            var switchCompany = await GetCompanyForOrder();
            if (switchCompany != null)
            {
                //Если существует компания, значит оформляем корп. заказ
                deliveryInfoModel.IsCompanyOrder = true;
                //Адреса доставки для выпадающего списка
                deliveryInfoModel.Address.CompanyAddresses = switchCompany.Addresses;
                deliveryInfoModel.Address.CompanyName = switchCompany.Name;

                deliveryInfoModel.SmsEnabledCompany = switchCompany.SmsNotify;
            }
            else
            {
                //TODO сделать подтягивание адреса и доступности смс из профиля
                var result = await ProfileClient.GetUserInfo();
                if (result.Succeeded)
                {
                    deliveryInfoModel.Address.DeliveryAddress = string.Join(", ", new List<string> {
                        !string.IsNullOrWhiteSpace(result.Content.Street) ? $"ул. {result.Content.Street}" : "",
                        !string.IsNullOrWhiteSpace(result.Content.House) ? $"д. {result.Content.House}" : "",
                        !string.IsNullOrWhiteSpace(result.Content.Building) ? $"стр. {result.Content.Building}" : "",
                        !string.IsNullOrWhiteSpace(result.Content.Flat) ? $"кв. {result.Content.Flat}" : "",
                        !string.IsNullOrWhiteSpace(result.Content.Office) ? $"оф. {result.Content.Building}" : "",
                        !string.IsNullOrWhiteSpace(result.Content.Entrance) ? $"под. {result.Content.Entrance}" : "",
                        !string.IsNullOrWhiteSpace(result.Content.Storey) ? $"э. {result.Content.Storey}" : "",
                        !string.IsNullOrWhiteSpace(result.Content.Intercom) ? $"дом. {result.Content.Intercom}" : ""
                    }.Where(x => x.Length > 0));
                    deliveryInfoModel.Address.Street = result.Content.Street;
                    deliveryInfoModel.Address.House = result.Content.House;
                    deliveryInfoModel.Address.Building = result.Content.Building;
                    deliveryInfoModel.Address.Flat = result.Content.Flat;
                    deliveryInfoModel.Address.Office = result.Content.Office;
                    deliveryInfoModel.Address.Entrance = result.Content.Entrance;
                    deliveryInfoModel.Address.Storey = result.Content.Storey;
                    deliveryInfoModel.Address.Intercom = result.Content.Intercom;
                }
            }

            UserModel currentUser = await UserServiceClient.GetUserByLogin(User.Identity.Name);
            deliveryInfoModel.SmsEnabledUser = currentUser.SmsNotify;
            deliveryInfoModel.SmsUser_BeforeChange = currentUser.SmsNotify;

            foreach (long key in _cartMulti.CafeCarts.Keys)
            {
                CartViewModel cafeCart = _cartMulti.CafeCarts[key];
                if (cafeCart.Cafe != null)
                    await RecalculateCartDeliveryPrice(cafeCart);
            }

            ViewBag.CartType = EnumCartType.Nav;

            viewModel.DeliveryInfo = deliveryInfoModel;
            viewModel.CartMulti = _cartMulti;

            if (TempData.ContainsKey("ModelState"))
            {
                ModelState.Merge(TempData["ModelState"] as ModelStateDictionary);
            }

            return View(viewModel);
        }

        /// <summary>
        /// Получить компанию из корпоративных заказов НА СЕГОДНЯ, для которой делается корп. заказ
        /// </summary>
        private async Task<CompanyModel> GetCompanyForOrder(HashSet<DateTime> dates = null)
        {
            var companyOrders = await GetCompanyOrdersForOrder(dates);
            if (companyOrders != null && companyOrders.Count > 0 && companyOrders[0].Company != null)
            {
                //Возвращаем компанию в рамках, которой будет происходить заказ
                return companyOrders[0].Company;
            }

            return null;
        }

        /// <summary>
        /// Получить Корпоративные заказы для компании выбранной из выпадающего списка на даты
        /// </summary>
        private async Task<List<CompanyOrderModel>> GetCompanyOrdersForOrder(HashSet<DateTime> dates = null)
        {
            if (dates == null)
            {
                dates = new HashSet<DateTime>() { DateTime.Now };
            }

            //Получаем для пользователя корпоративные заказы для кафешек на сегодня,
            long? switchCompanyId = await ProfileClient.GetUserCompanyId();
            var request = new RequestCartCompanyOrderModel()
            {
                CafeIds = new HashSet<long>(_cartMulti.CafeCarts.Keys.Select(e => e).ToList()),
                Dates = dates,
                CompanyId = switchCompanyId.HasValue ? switchCompanyId.Value : 0
            };

            var response = CompanyOrderServiceClient.GetCompanyOrderForUserByCompanyId(request).GetAwaiter().GetResult();
            if (response.Status == 0)
            {
                var companyOrders = new List<CompanyOrderModel>();

                //Если запрос успешен, то извлекаем корп. заказы пользователя
                if (response.Result != null)
                    return JsonConvert.DeserializeObject<List<CompanyOrderModel>>(response.Result.ToString());
            }

            return null;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(DeliveryInfoViewModel model, bool paidPoints = false, bool preOrder = false, string name = null, [FromServices] INotificationsHub hub = null)
        {
            if (String.IsNullOrEmpty(model.MoneyComment) && model.NoChange == false)
                model.MoneyComment = "0";
            if (model.PayType == EnumOrderPayType.CourierCard)
                model.MoneyComment = "terminal";
            OrderMultiCafeViewModel orderMulti = new OrderMultiCafeViewModel
            {
                DeliveryInfo = model,
                CartMulti = _cartMulti
            };
            if (!model.ConfirmAgreement)
            {
                ModelState.AddModelError("ConfirmAgreement",
                    "Необходимо согласиться с условиями пользовательского соглашения");
            }

            // Если пользователь изменил флаг СМС-оповещения - надо  сохранить этот флаг:
            if (model.SmsEnabledUser != model.SmsUser_BeforeChange)
                await UserServiceClient.SetUserSmsNotifications(User.Identity.Name, model.SmsEnabledUser);

            if (model.IsCompanyOrder)
            {
                var companyId = await ProfileClient.GetUserCompanyId();
                if (companyId != null)
                {
                    var company = await CompanyService.GetCompanyById(companyId.Value);
                    if (company != null && company.Addresses != null && company.Addresses.Count > 0 && _httpContextAccessor.HttpContext.Session.GetCurrentDeliveryAddress() == null)
                    {
                        _httpContextAccessor.HttpContext.Session.SetCurrentDeliveryAddress(company.Addresses[0].GetAddressString());
                    }
                }

                #region Оформление корпоративного заказа

                //Получаем даты, на которые были сделаны заказы и относятся ли все кафе к корпоративным
                HashSet<DateTime> hstDateTimes = new HashSet<DateTime>();
                foreach (long key in _cartMulti.CafeCarts.Keys)
                {
                    var cafeCart = _cartMulti.CafeCarts[key];

                    if (cafeCart.Cafe.CafeType == CafeType.PersonOnly)
                    {
                        ModelState.AddModelError(string.Empty, $"Кафе \"{cafeCart.Cafe.Name}\" не обслуживает корпоративных клиентов.");
                        break;
                    }

                    //Формируем множество дат из корзины, на которые были сделаны заказы
                    cafeCart.CartItems?.Select(e => e.DeliveryDate).ToList().ForEach(e => hstDateTimes.Add(e));
                }

                // Если в модели ошибки - показываем их:
                if (!ModelState.IsValid)
                {
                    return await Index(name);
                }

                // Получаем доступные корпоративные заказы на даты, которые указаны в корзинах
                List<CompanyOrderModel> companyOrders = await GetCompanyOrdersForOrder(hstDateTimes);

                // Разные проверки корзин всех кафе, участвующих в заказе.
                foreach (long key in _cartMulti.CafeCarts.Keys)
                {
                    // Получаем заказ из очередного кафе:
                    CartViewModel cafeCart = _cartMulti.CafeCarts[key];

                    // Если заказ пустой - ничего с ним делать не нужно:
                    if (cafeCart == null && cafeCart.Count <= 0) continue;

                    // Проверка доступности корпоративного заказа в данном кафе, на указанную дату:
                    foreach (var item in cafeCart.CartItems)
                    {
                        var companyOrderCafe = companyOrders.FirstOrDefault(co => co.CafeId == cafeCart.Cafe.Id && item.DeliveryDate.Date == co.OrderOpenDate.Value.Date);
                        if (companyOrderCafe == null)
                            ModelState.AddModelError(string.Empty, $"В кафе '{cafeCart.Cafe.Name}' на {item.DeliveryDate.ToShortDateString()} отсутствует корп. заказ");
                    }
                }

                // Если в модели ошибки - показываем их:
                if (!ModelState.IsValid)
                {
                    return await Index(name);
                }

                // Критических ошибок не обнаружено, заказ можно оформлять.
                orderMulti.Message = new MessageViewModel() { Text = "Ваш заказ принят в обработку. В ближайшее время с Вами свяжется менеджер по указанному Вами телефону." };

                try
                {
                    // Переменная для обработки строк с ошибками заказов:
                    System.Text.StringBuilder sbOrderExceptions = new System.Text.StringBuilder();
                    // Перебор кафе и отправка им заказов
                    foreach (long key in _cartMulti.CafeCarts.Keys)
                    {
                        // Получаем заказ из очередного кафе:
                        CartViewModel cafeCart = _cartMulti.CafeCarts[key];

                        long? companyOrderId = null;
                        var companyOrder = companyOrders?.FirstOrDefault(co => co.CafeId == cafeCart.Cafe.Id);

                        if (companyOrder != null && model.IsCompanyOrder)
                            companyOrderId = companyOrder.Id;

                        var orderInfo = new OrderInfoModel()
                        {
                            PaymentType = (long)EnumOrderPayType.CourierCash,
                            OrderEmail = model.Email,
                            OrderAddress = model.Address.DeliveryAddress,
                            OrderPhone = model.PhoneNumber
                        };

                        List<string> exceptions;
                        var orderItems = new List<OrderItemModel>();

                        foreach (var order in cafeCart.CartItems.OrderBy(c => c.DeliveryDate).GroupBy(c => c.DeliveryDate))
                        {
                            foreach (var item in order)
                            {
                                if (item.Count > 0)
                                {
                                    orderItems.Add(new OrderItemModel()
                                    {
                                        Discount = cafeCart.ClientDiscount,
                                        DishCount = item.Count,
                                        FoodDishId = item.Id,
                                        Comment = item.Comment
                                    });
                                }
                            }

                            companyOrder = companyOrders.FirstOrDefault(co => co.CafeId == cafeCart.Cafe.Id && order.Key.Date == co.OrderOpenDate.Value.Date);

                            if (TempData.ContainsKey("preOrder"))
                                preOrder = (bool)TempData["preOrder"];

                            PostOrderResponse orderResponse = await OrderServiceClient.PostOrders(
                                new PostOrderRequest()
                                {
                                    Order = new OrderModel()
                                    {
                                        CafeId = cafeCart.Cafe.Id,
                                        CompanyOrderId =
                                            model.IsCompanyOrder && companyOrder != null
                                                ? companyOrder.Id
                                                : (long?)null,
                                        DeliverDate = order.Key,
                                        OrderItems = orderItems,
                                        Status = (long)cafeCart.Status,
                                        PhoneNumber = model.PhoneNumber,
                                        Comment = model.OrderComment,
                                        OrderInfo = orderInfo,
                                        BanketId = null,
                                        PreOrder = preOrder
                                    },
                                    FailureURL = string.Empty,
                                    SuccessURL = string.Empty,
                                    //PaymentType = paidPoints? PaymentTypeEnum.PaidPoints: PaymentTypeEnum.Delivery
                                    PaymentType = model.PayTypeLegalEntity,
                                });

                            if (orderResponse != null
                                && orderResponse.OrderStatus.ExceptionList.Count > 0
                            )
                            {
                                exceptions =
                                    orderResponse.OrderStatus.ExceptionList.Select(item => item.Message).ToList();
                                if (sbOrderExceptions.Length > 0)
                                    sbOrderExceptions.Append("\n");
                                sbOrderExceptions.Append(string.Join("\n", exceptions));
                            }
                            orderItems.Clear();
                        }
                    }

                    if (sbOrderExceptions.Length > 0)
                        orderMulti.Message.Text = sbOrderExceptions.ToString();
                    orderMulti.OrderIsDone = true;

                    /*else//если без ошибок
                    if(Cart.Cafe.AllowPaymentByPoints)
                    {
                        if (Cart.DeliveryCost < 0) Cart.DeliveryCost = 0;
                        if (!paidPoints)// не оплачено баллами, то начисляем
                        {
                            bool tmp = await FoodClient.EditUserPointsByLoginAndTotalPriceAsync(User.Identity.Name, orderModel.Cart.TotalPrice + Cart.DeliveryCost);
                            if (!tmp) resultMessage += "<br/>Не удалось начислить баллы";
                        }
                        else//заплатил баллами, то списываем
                        {
                            bool tmp = await FoodClient.EditUserPointsByLoginAsync(User.Identity.Name, 0, -orderModel.Cart.TotalPrice - Cart.DeliveryCost);
                            if (!tmp) resultMessage += "<br/>Не удалось списать баллы";
                        }
                    }*/
                }
                catch (Exception ex)
                {
                    orderMulti.Message.Text = ex.Message;
                    orderMulti.OrderIsDone = false;
                }

                #endregion
            }
            else
            {
                /*orderMulti.Message = new MessageViewModel() { Text = "Возможность совершения индивидуального заказа отключена" };*/

                #region Оформление Индивидуального заказа. 
                //Сейчас нельзя делать Инд. заказы. Поэтому выдаем ошибку о невозможности его произвести
                // Если в модели ошибки - показываем их:
                if (!ModelState.IsValid)
                {
                    return await Index(name);
                }

                // Критических ошибок не обнаружено, заказ можно оформлять.
                orderMulti.Message = new MessageViewModel() { Text = "Ваш заказ принят в обработку. В ближайшее время с Вами свяжется менеджер по указанному Вами телефону." };

                try
                {
                    int i = 0;

                    // Переменная для обработки строк с ошибками заказов:
                    System.Text.StringBuilder sbOrderExceptions = new System.Text.StringBuilder();
                    // Перебор кафе и отправка им заказов
                    foreach (long key in _cartMulti.CafeCarts.Keys)
                    {
                        // Получаем заказ из очередного кафе:
                        CartViewModel cafeCart = _cartMulti.CafeCarts[key];
                        if (model.NoChange == true)
                        {
                            model.MoneyComment = "Сумма заказа -";
                        }

                        var orderInfo = new OrderInfoModel()
                        {
                            PaymentType = (long)model.PayType,
                            OrderEmail = model.Email,
                            OrderAddress = model.Address.DeliveryAddress,
                            OrderPhone = model.PhoneNumber,
                        };

                        List<string> exceptions;
                        var orderItems = new List<OrderItemModel>();

                        foreach (var order in cafeCart.CartItems.OrderBy(c => c.DeliveryDate.Date).GroupBy(c => c.DeliveryDate.Date))
                        {
                            foreach (var item in order)
                            {
                                if (item.Count > 0)
                                {
                                    orderItems.Add(new OrderItemModel()
                                    {
                                        Discount = cafeCart.ClientDiscount,
                                        DishCount = item.Count,
                                        FoodDishId = item.Id,
                                        Comment = item.Comment
                                    });
                                }
                            }

                            if (TempData.ContainsKey("preOrder"))
                                preOrder = (bool)TempData["preOrder"];

                            PostOrderResponse orderResponse = await OrderServiceClient.PostOrders(
                                new PostOrderRequest()
                                {
                                    Order = new OrderModel()
                                    {
                                        CafeId = cafeCart.Cafe.Id,
                                        CompanyOrderId = null,
                                        DeliverDate = model.DateDelivery[i++],
                                        OrderItems = orderItems,
                                        Status = (long)cafeCart.Status,
                                        PhoneNumber = model.PhoneNumber,
                                        Comment = model.OrderComment,
                                        OrderInfo = orderInfo,
                                        BanketId = null,
                                        PreOrder = preOrder,
                                        OddMoneyComment = model.MoneyComment,
                                    },
                                    FailureURL = string.Empty,
                                    SuccessURL = string.Empty,
                                    PaymentType = model.PayType.ToString(),
                                    // Расскоментировать после добавления способов оплаты
                                    //PaymentType = paidPoints? PaymentTypeEnum.PaidPoints: PaymentTypeEnum.Delivery
                                    //PaymentType = PaymentTypeEnum.Delivery
                                });

                            if (orderResponse != null && orderResponse.OrderStatus.ExceptionList.Count > 0)
                            {
                                exceptions = orderResponse.OrderStatus.ExceptionList.Select(item => item.Message).ToList();
                                if (sbOrderExceptions.Length > 0)
                                    sbOrderExceptions.Append("\n");
                                sbOrderExceptions.Append(string.Join("\n", exceptions));
                            }
                            orderItems.Clear();
                        }
                    }

                    if (sbOrderExceptions.Length > 0)
                        orderMulti.Message.Text = sbOrderExceptions.ToString();
                    orderMulti.OrderIsDone = true;
                }
                catch (Exception ex)
                {
                    orderMulti.Message.Text = ex.Message;
                    orderMulti.OrderIsDone = false;
                }
                #endregion
            }

            //TODO: А если у нас в каком-то кафе оформился заказ, а в др. нет. То OrderIsDone будет false. 
            //Получиться, что не будет уведомления для менеджера кафе, в котором заказ прошел успешно
            if (orderMulti.OrderIsDone)
            {
                // Перебор кафе и отправка оповещений
                foreach (long key in _cartMulti.CafeCarts.Keys)
                {
                    // Отправка оповещения менеджерам очередного кафе:
                    CartViewModel cafeCart = _cartMulti.CafeCarts[key];
                    foreach (var group in cafeCart.CartItems.GroupBy(c => c.DeliveryDate))
                    {
                        var groupDelivery = group.FirstOrDefault();
                        if (groupDelivery != null)
                        {
                            var managerEmails = await _cafeOrderNotificationClient.NewOrder(cafeCart.Cafe.Id, groupDelivery.DeliveryDate);
                            if (groupDelivery.DeliveryDate.Date == DateTime.Now.Date)
                                hub?.NorifyAboutOrder(managerEmails, Url.Action("CheckNewOrders", "Cafe", new { area = "Manager" }, "http"));
                        }
                    }
                }
            }
            _cartMulti = new CartMultiCafeViewModel();
            return View(orderMulti);
        }

        /// <summary>
        /// Обновляет время доставки заказа
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе</param>
        /// <param name="index">Индекс CafeCart</param>
        /// <param name="date">Дата</param>
        /// <param name="time">Новое время</param>
        /// <returns></returns>
        public async Task<IActionResult> UpdateCart(long cafeId, int index, DateTime date, string time)
        {
            var cart = _cartMulti.CafeCarts[cafeId];
            var timeParts = time.Split(':');
            var newDateTime = date.Date + new TimeSpan(int.Parse(timeParts[0]), int.Parse(timeParts[1]), 0);
            if (index == 0)
            {
                cart.DeliverDate = date;
            }
            cart.CartItems.Where(c => c.DeliveryDate.Date == date.Date).ToList().ForEach(c => c.DeliveryDate = newDateTime);
            var oldDate = cart.DeliveryPriceInfoByDates.Keys.FirstOrDefault(c => c.Date == date.Date);
            if (oldDate != default)
            {
                cart.DeliveryPriceInfoByDates.Remove(oldDate);
                cart.DeliveryPriceInfoByDates.Keys.ToList().ForEach(t =>
                {
                    if (t.Date == date.Date)
                    {
                        cart.DeliveryPriceInfoByDates.Remove(t);
                    }
                });
                if (!cart.DeliveryPriceInfoByDates.ContainsKey(newDateTime))
                {
                    cart.DeliveryPriceInfoByDates.Add(newDateTime, null);
                }
            }
            await RecalculateCartDeliveryPrice(cart);
            return Ok();
        }

        public ActionResult ResetOrder(EnumCartType cartType)
        {
            _cartMulti = new CartMultiCafeViewModel();
            return GetCartList(cartType, _cartMulti);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            AsyncHelper.RunSync(RecalculateCart);
            SaveSessionCart();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.ActionArguments.ContainsKey("cartType"))
            {
                LoadSessionCart((EnumCartType)filterContext.ActionArguments["cartType"]);
            }
            else
            {
                LoadSessionCart();
            }
            //_cartMulti = CartMultiCafe.Load(Session, cartType);
            //ViewBag.CartType = _cartMulti.Type;
        }

        private ActionResult GetCartItem(long CafeId, EnumCartType cartType, CartItemViewModel item)
        {
            if (!_cartMulti.CafeCarts.ContainsKey(CafeId))
                return new EmptyResult();
            CartViewModel cafeCart = _cartMulti.CafeCarts[CafeId];
            if (cafeCart == null || cafeCart.Count == 0 || item == null)
                return new EmptyResult();

            ViewBag.CartType = _cartMulti.Type;

            switch (cartType)
            {
                case EnumCartType.Full:
                    return PartialView("_CartItem", item);

                case EnumCartType.Nav:
                case EnumCartType.Short:
                case EnumCartType.Banket:
                    return PartialView("_ShortCartItem", item);

                default:
                    return BadRequest("Invalid Cart Type");
            }
        }

        private ActionResult GetCartList(EnumCartType cartType, CartMultiCafeViewModel cartMulti)
        {
            foreach (long key in cartMulti.CafeCarts.Keys)
            {
                CartViewModel cart = cartMulti.CafeCarts[key];
                if (cart.Cafe != null)
                {
                    var stringId = _httpContextAccessor.HttpContext.User.Identity.GetUserId();
                    var userId = stringId == null ? null : long.Parse(stringId) as long?;
                    if (userId == null) break;
                    cart.IsCompanyEmployee = AsyncHelper.RunSync(() => CompanyOrderServiceClient.CheckUserIsEmployee(userId.Value, cart.Cafe.Id, DateTime.Now));
                }
            }
            cartMulti.Type = cartType;
            switch (cartType)
            {
                case EnumCartType.Full:
                    return PartialView("_List", new OrderMultiCafeViewModel() { CartMulti = cartMulti });

                case EnumCartType.Short:
                case EnumCartType.Nav:
                case EnumCartType.Banket:
                    ViewBag.CartType = cartType;
                    return PartialView("_SideBarCart", cartMulti);

                default:
                    return BadRequest("Invalid Cart Type");
            }
        }

        private void LoadSessionCart(EnumCartType cartType = EnumCartType.Short)
        {
            _cartMulti = CartMultiCafeViewModel.Load(_httpContextAccessor.HttpContext.Session, cartType);
        }

        private async Task RecalculateCart()
        {
            foreach (long key in _cartMulti.CafeCarts.Keys)
            {
                CartViewModel nextCart = _cartMulti.CafeCarts[key];

                if (nextCart.Cafe == null)
                    return;

                nextCart.ClientDiscount = nextCart.Type != EnumCartType.Banket ? await _cafeService.GetDiscountAmount(nextCart.Cafe.Id, DateTime.Now) : 0;

                foreach (var item in nextCart.CartItems)
                    item.Discount = nextCart.ClientDiscount;

                // Вычисление стоимости доставки:
                await RecalculateCartDeliveryPrice(nextCart);
            }
        }

        private async Task RecalculateCartDeliveryPrice(CartViewModel cart)
        {
            // Вычисление стоимости доставки.
            // Если у кафе включены отложенная доставка и/или меню на неделю - надо будет вычислять стоимость доставки отдельно на каждую дату:
            if (cart.Cafe.WeekMenuIsActive || cart.Cafe.DeferredOrder)
            {
                // Объект общей стоимости доставки:
                cart.DeliveryPriceInfo = new OrderDeliveryPriceModel();
                // Группируем заказы по датам:
                foreach (var order in cart.CartItems.OrderBy(c => c.DeliveryDate).GroupBy(c => c.DeliveryDate))
                {
                    // Получаем общую стоимость блюд, заказанных на дату:
                    double totalPriceOfDishes = order.Sum(d => d.TotalPrice);

                    // Получаем стоимость доставки на указанную дату:
                    OrderDeliveryPriceModel delivery = null;

                    if (!cart.IsCompanyEmployee || await ProfileClient.GetUserCompanyId() == null || _httpContextAccessor.HttpContext.Session.GetCurrentDeliveryAddress() == null)
                    {
                        delivery = await _cafeService.GetCostOfDeliveryAsync(cart.Cafe.Id, totalPriceOfDishes, cart.IsCompanyEmployee, order.Key.ToString(StandardDateFormat));
                    }
                    else
                    {
                        var companyId = (await ProfileClient.GetUserCompanyId()).Value;
                        var shipAddress = _httpContextAccessor.HttpContext.Session.GetCurrentDeliveryAddress();

                        delivery = await _cafeService.GetShippingCostToSingleCompanyAddress(
                            shipAddress,
                            companyId,
                            cart.Cafe.Id,
                            totalPriceOfDishes,
                            order.Key.Date
                        );
                    }

                    // Добавляем стоимость доставки на дату в словарь по датам:
                    if (cart.DeliveryPriceInfoByDates.ContainsKey(order.Key))
                        cart.DeliveryPriceInfoByDates[order.Key] = delivery;
                    else
                        cart.DeliveryPriceInfoByDates.Add(order.Key, delivery);
                    // Добавляем стоимость доставки на дату к общей:
                    cart.DeliveryPriceInfo.PriceForOneOrder += delivery.PriceForOneOrder;
                    cart.DeliveryPriceInfo.TotalPrice += delivery.TotalPrice;
                    cart.DeliveryPriceInfo.CountOfOrders = delivery.CountOfOrders;
                }
            }
            else
            {
                // Нет ни отложенной доставки, ни меню на неделю.
                // Стоимость доставки вычисляется для всего заказа:
                var companyId = await ProfileClient.GetUserCompanyId();
                cart.DeliveryPriceInfo =
                    !cart.IsCompanyEmployee || companyId == null || _httpContextAccessor.HttpContext.Session.GetCurrentDeliveryAddress() == null
                        ? await _cafeService.GetCostOfDeliveryAsync(cart.Cafe.Id, cart.TotalPrice, cart.IsCompanyEmployee)
                        : await _cafeService.GetShippingCostToSingleCompanyAddress(_httpContextAccessor.HttpContext.Session.GetCurrentDeliveryAddress(),
                                                                                   companyId.Value,
                                                                                   cart.Cafe.Id,
                                                                                   cart.TotalPrice,
                                                                                   DateTime.Today);
            }
        }

        [AjaxOnly]
        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult> SelectNewShippingAddress(string address, EnumCartType cartType)
        {
            if (await ProfileClient.GetUserCompanyId() == null)
                return GetCart(cartType);

            var companyId = (await ProfileClient.GetUserCompanyId()).Value;
            var company = await CompanyService.GetCompanyById(companyId);

            if (company == null || company.Addresses == null || company.Addresses.Count == 0)
                return GetCart(cartType);

            if (!company.Addresses.Any(x => x.GetAddressString(true) == address))
                return GetCart(cartType);

            _httpContextAccessor.HttpContext.Session.SetCurrentDeliveryAddress(address);
            return GetCart(cartType);
        }

        private void SaveSessionCart()
        {
            CartMultiCafeViewModel.Save(_httpContextAccessor.HttpContext.Session, _cartMulti);
        }

        public double GetTotalPriceAndDeliveryCost()
        {
            double TotalCost = 0.0;
            foreach (long key in _cartMulti.CafeCarts.Keys)
            {
                if (_cartMulti.CafeCarts[key] != null)
                    TotalCost += _cartMulti.CafeCarts[key].TotalPrice + _cartMulti.CafeCarts[key].DeliveryPriceInfo.PriceForOneOrder;
            }
            return TotalCost;
        }

        // Можно ли оплачивать заказ баллами для указанного кафе
        public bool GetAllowPaymentByPoints(long CafeId)
        {
            if (_cartMulti.CafeCarts.ContainsKey(CafeId) && _cartMulti.CafeCarts[CafeId].Cafe != null)
                return _cartMulti.CafeCarts[CafeId].Cafe.AllowPaymentByPoints;
            return false;
        }

        private void ClearCartByUserCompany()
        {
            // Проверяем является ли пользователь сотрудником
            var userCompany = AsyncHelper.RunSync(ProfileClient.GetUserCompanyId);
            if (userCompany != null)
            {
                // Список идентификаторов кафе для удаления из корзины
                var forDelete = new List<long>();
                // Расписания компании пользователя
                var schedules = AsyncHelper.RunSync(() =>
                CompanyOrderServiceClient.GetListOfCompanyOrderScheduleByCompanyId(userCompany.Value));
                foreach (var item in _cartMulti.CafeCarts)
                {
                    // Если кафе только для физиков
                    if (item.Value.Cafe.CafeType == CafeType.PersonOnly
                        // Или если кафе для всех но нет расписания для компании пользователя
                        || (item.Value.Cafe.CafeType == CafeType.CompanyPerson
                        && (schedules == null || schedules.Count == 0 || !schedules.Any(s => s.CafeId == item.Value.Cafe.Id))))
                    {
                        // Добавляем кафе к удалению
                        forDelete.Add(item.Key);
                    }
                }
                // Удаляем карточки кафе
                foreach (var id in forDelete)
                {
                    _cartMulti.CafeCarts.Remove(id);
                }
            }
        }

        
    }
}
