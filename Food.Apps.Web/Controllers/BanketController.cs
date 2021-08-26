using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.AuthorizationService;
using Microsoft.AspNetCore.Mvc;
using FoodDishModel = ITWebNet.Food.Site.Areas.Manager.Models.FoodDishModel;

namespace ITWebNet.Food.Site.Controllers
{
    public class BanketController : BaseController
    {
        public BanketController(
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
            ICityService cityService) : 
            base(accountSevice, profileClient, addressServiceClient, 
                banketsService, cafeService, companyService, dishCategoryService, 
                dishService, companyOrderServiceClient, orderItemServiceClient, 
                orderServiceClient, ratingServiceClient, tagServiceClient, 
                userServiceClient, menuServiceClient, cityService)
        {

        }

        public async Task<IActionResult> Index(string name)
        {
            
            ViewBag.CartType = EnumCartType.Banket;
            var model = await GetMenuModel(null);
            if (model.Banket != null)
            {
                var banketOrder =
                    await _banketsService.GetOrderByBanketId(model.Banket.Id, long.Parse(User.Identity.GetUserId()));
                var multicart = CartMultiCafeViewModel.Load(HttpContext.Session, EnumCartType.Banket);
                if (multicart != null)
                {
                    CartViewModel cart = multicart.CafeCarts.FirstOrDefault().Value;
                    if (cart == null)
                    {
                        var cafe = await _cafeService.GetCafeByCleanUrl(name);
                        cart = new CartViewModel();
                        cart.Type = EnumCartType.Banket;
                        multicart.CafeCarts.Add(cafe.Id, cart);
                    }
                    cart.Cafe = model.Cafe;
                    if (banketOrder.Succeeded && banketOrder.Content != null)
                    {
                        cart.CartItems.Clear();
                        cart.Type = EnumCartType.Banket;
                        banketOrder.Content.OrderItems.ForEach(c =>
                        {
                            cart.CartItems.Add(new CartItemViewModel()
                            {
                                BasePrice = c.DishBasePrice,
                                Count = c.DishCount,
                                Name = c.DishName,
                                OrderItemId = c.Id,
                                Id = c.FoodDishId,
                                CafeId = cart.Cafe.Id
                            });
                        });
                    }
                    CartMultiCafeViewModel.Save(HttpContext.Session, multicart);
                }
            }
            return View(model);
        }

        [AjaxOnly]
        public async Task<ActionResult> Filter(long? cafeId, string filter, long banketMenuId)
        {
            var model = new Dictionary<FoodCategoryModel, List<Core.DataContracts.Common.FoodDishModel>>();
            if (!cafeId.HasValue)
                return PartialView("~/Views/Cafe/_Menu.cshtml", model);

            if (!string.IsNullOrEmpty(filter))
            {
                model = await _cafeService.GetFilteredMenuByPatternId(cafeId.Value, banketMenuId, filter);
            }
            else
            {
                model = await _cafeService.GetMenuByPatternIdAsync(cafeId.Value, banketMenuId);
            }

            var clientDiscount = AsyncHelper.RunSync(() => _cafeService.GetDiscountAmount(cafeId.Value, DateTime.Now, null));
            ViewBag.Cafe = await _cafeService.GetCafeById(cafeId.Value);
            ViewBag.ClientDiscount = clientDiscount;
            TempData["Categories"] = model.Keys.ToList();
            return PartialView("~/Views/Cafe/_Menu.cshtml", model);
        }

        public async Task<ActionResult> Save(string name)
        {
            var cartMulti = CartMultiCafeViewModel.Load(HttpContext.Session, EnumCartType.Banket);

            if (cartMulti.CafeCarts.Count > 0)
            {
                CartViewModel cart;
                if (cartMulti.CurrentCafeId != null)
                    cart = cartMulti.CafeCarts[cartMulti.CurrentCafeId ?? -1];
                else
                    cart = cartMulti.CafeCarts.FirstOrDefault().Value;

                DeliveryInfoViewModel deliveryInfo = new DeliveryInfoViewModel();

                var orderInfo = new OrderInfoModel()
                {
                    PaymentType = (long)PaymentTypeEnum.Default,
                    OrderEmail = User.Identity.GetUserEmail(),
                    OrderPhone = User.Identity.GetUserPhone(),
                    DeliverySumm = 0
                };

                if (cart.Cafe == null)
                {
                    cart.Cafe = await _cafeService.GetCafeByCleanUrl(name);
                }
                if (cart.Cafe != null)
                {
                    var bankets = await ProfileClient.GetAvailableBankets(cart.Cafe.Id);
                    var banket = bankets.OrderBy(c => c.EventDate).FirstOrDefault();
                    if (banket != null)
                    {
                        var banketOrder =
                            await _banketsService.GetOrderByBanketId(banket.Id, long.Parse(User.Identity.GetUserId()));
                        var orderItems = (from item in cart.CartItems
                                          where item.Count > 0
                                          select new OrderItemModel()
                                          {
                                              Discount = cart.ClientDiscount,
                                              DishCount = item.Count,
                                              FoodDishId = item.Id,
                                              Comment = item.Comment,
                                              DishBasePrice = item.BasePrice,
                                              DishName = item.Name,
                                              TotalPrice = item.TotalPrice,
                                          }).ToList();

                        orderInfo.OrderAddress = banket.Company.DeliveryAddress.RawAddress;

                        var order = new Core.DataContracts.Common.OrderModel()
                        {
                            CafeId = cart.Cafe.Id,
                            DeliverDate = cart.DeliverDate,
                            OrderItems = orderItems,
                            Status = (long)cart.Status,
                            OrderInfo = orderInfo,
                            BanketId = banket.Id,
                            itemsCount = orderItems.Sum(c => c.DishCount),
                            TotalSum = orderItems.Sum(c => c.TotalPrice)
                        };
                        HttpResult response;
                        if (banketOrder.Succeeded && banketOrder.Content != null)
                        {
                            response = await _banketsService.UpdateOrder(order);
                        }
                        else
                        {
                            response = await _banketsService.PostOrders(order);
                        }
                        if (response.Succeeded)
                        {
                            cart = new CartViewModel().SetDefaults();
                            CartViewModel.Save(HttpContext.Session, cart);
                            TempData["Success"] = "Заказ сохранен";
                        }
                        else
                        {
                            TempData["Error"] = string.Join(Environment.NewLine, response.Errors);
                        }
                    }
                }

            }
            else if (cartMulti.CafeCarts.Count == 0)
            {
                var model = await GetMenuModel(null);
                var banketOrder =
                    await _banketsService.GetOrderByBanketId(model.Banket.Id, long.Parse(User.Identity.GetUserId()));
                banketOrder.Content.OrderItems = new List<OrderItemModel>();
                var response = await _banketsService.UpdateOrder(banketOrder.Content);
                if (response.Succeeded)
                {
                    TempData["Success"] = "Заказ сохранен";
                }
                else
                {
                    TempData["Error"] = string.Join(Environment.NewLine, response.Errors);
                }
            }
            return RedirectToAction(nameof(Index), new { name });
        }
    }
}