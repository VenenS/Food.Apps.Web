using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Food.Services.Contracts.Companies;
using FoodApps.Web.NetCore.Extensions;
using ITWebNet.Food.Core.DataContracts.Account;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.AuthorizationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace ITWebNet.Food.Site.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private ICafeOrderNotificationService _cafeOrderNotificationService;
        private ICityService _cityService { get; set; }

        public ProfileController(ServiceManager serviceManager) : base(serviceManager)
        {
        }

        [ActivatorUtilitiesConstructor]
        public ProfileController(
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
            ICafeOrderNotificationService cafeOrderNotificationService) :
            base(accountSevice, profileClient, addressServiceClient,
                banketsService, cafeService, companyService, dishCategoryService,
                dishService, companyOrderServiceClient, orderItemServiceClient,
                orderServiceClient, ratingServiceClient, tagServiceClient,
                userServiceClient, menuServiceClient, cityService)
        {
            _cafeOrderNotificationService = cafeOrderNotificationService;
            _cityService = cityService;
        }
        public ProfileController()
        {
            _cafeOrderNotificationService = new CafeOrderNotificationService();
        }

        //public async Task<IActionResult> AddExternalLogin()
        //{
        //    var externalCookie = Request.Cookies.Get(".AspNet.ExternalCookie");
        //    if (externalCookie == null)
        //        return RedirectToAction("ExternalLogins");

        //    await ProfileClient.AddExternalLogin(new AddExternalLoginModel { ExternalAccessToken = externalCookie.Value });

        //    return RedirectToAction("ExternalLogins");
        //}

        [HttpGet]
        public async Task<ActionResult> Security()
        {
            var result = await ProfileClient.GetUserInfo();
            if (result.Succeeded)
                ViewBag.UserHasPassword = TempData.ContainsKey("ResetPassword")?  !(bool)TempData["ResetPassword"] : result.Content.HasPassword;

            return View("~/Views/Profile/Security.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordModel model)
        {
            ViewBag.UserHasPassword = true;
            if (!ModelState.IsValid)
                return View("Security", model);

            if (model.OldPassword != null && model.OldPassword.Equals(model.NewPassword))
                ModelState.AddModelError("", "Старый и новый пароль не должны совпадать");

            if (!ModelState.IsValid)
                return View("Security", model);

            var result = await ProfileClient.ChangePassword(model);
            if (result.Succeeded)
                ViewBag.Message = result.Content;

            SetModelErrors(result);

            if (ModelState.IsValid)
                return View("Security", model);

            return View("Security", model);
        }

        [AjaxOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_SetPassword", model);

            var result = await ProfileClient.SetPassword(model);
            if (result.Succeeded)
                ViewBag.Message = result.Content;

            SetModelErrors(result);

            if (ModelState.IsValid)
                return PartialView("_SetPassword");

            return PartialView("_SetPassword", model);
        }

        //public async Task<ActionResult> ExternalLogins()
        //{
        //    string redirectUrl = Url.Action("AddExternalLogin", null, null, Request.Url.Scheme);
        //
        //    var result = await AccountSevice.ExternalLogins(Server.UrlPathEncode(redirectUrl));
        //    if (result.Succeeded)
        //    {
        //        var loginButtons = result.Content.ToDictionary(b => b.Name.ToLowerInvariant());
        //        ViewBag.LoginButtons = loginButtons;
        //    }
        //
        //    return View();
        //}

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var result = await ProfileClient.GetUserInfo();
            var cities = await _cityService.GetCities();
            ViewBag.Cities = cities.Content;
            if (result.Succeeded)
            {
                var companies = await CompanyService.GetCompaniesByFilter(new CompaniesFilterModel()
                {
                    CompanyId = result.Content.UserInCompanies?.Select(c => c.CompanyId).ToArray() ?? new long[] { }
                });
                ViewBag.Companies = companies.Content;
                return View("~/Views/Profile/Index.cshtml", result.Content);
            }

            SetModelErrors(result);

            return View();
        }

        [HttpGet, AjaxOnly]
        public async Task<ActionResult> SendCode()
        {
            var result = await ProfileClient.GetUserInfo();

            var companies = await CompanyService.GetCompaniesByFilter(new CompaniesFilterModel()
            {
                CompanyId = result.Content.UserInCompanies?.Select(c => c.CompanyId).ToArray() ?? new long[] { }
            });
            ViewBag.Companies = companies.Content;

            if (result.Succeeded)
            {
                var response = await _cafeOrderNotificationService.SendSmsCode(result.Content.PhoneNumber, true);
                if (response.Status == 0)
                {
                    result.Content.IsSendCode = true;
                }
                else
                {
                    result.Content.IsSendCode = false;
                }

                return PartialView("_Details", result.Content);
            }

            SetModelErrors(result);

            return PartialView("_Details");
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmPhone(UserInfoModel model)
        {
            var result = await ProfileClient.GetUserInfo();

            if (result.Succeeded)
            {
                result.Content.SmsCode = model.SmsCode;
                await ProfileClient.ConfirmPhone(result.Content);
            }

            return RedirectToAction("Index");
        }

        [HttpGet, AjaxOnly]
        public async Task<ActionResult> SendEmailConfirmationCode()
        {
            var result = await ProfileClient.GetUserInfo();
            if (result.Succeeded)
            {
                var response = await ProfileClient.SendEmailConfirmationCode();
                if (response.Succeeded)
                    ViewBag.ResultMessage = "Письмо с подтверждением было отправлено на указанный email.";

                result.Content.IsSendEmailConfirmationCode = response.Content;

                return PartialView("_Details", result.Content);
            }

            return PartialView("_Details");
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmEmail(UserInfoModel model)
        {
            var result = await ProfileClient.GetUserInfo();

            if (result.Succeeded)
            {
                result.Content.EmailCode = model.EmailCode;
                await ProfileClient.ConfirmEmail(result.Content);
            }

            return RedirectToAction("Index");
        }

        [HttpGet, AjaxOnly] 
        public async Task<ActionResult> Details()
        {
            var result = await ProfileClient.GetUserInfo();
            if (result.Succeeded)
            {
                var companies = await CompanyService.GetCompaniesByFilter(new CompaniesFilterModel()
                {
                    CompanyId = result.Content.UserInCompanies?.Select(c => c.CompanyId).ToArray() ?? new long[] { }
                });
                ViewBag.Companies = companies.Content;
                var cities = await _cityService.GetCities();
                ViewBag.Cities = cities.Content;
                return PartialView("_Details", result.Content);
                
            }

            SetModelErrors(result);

            return PartialView("_Details");
        }

        [HttpGet, AjaxOnly]
        public Task<ActionResult> Edit()
        {
            return RenderEditView();
        }

        [HttpPost]
        public async Task<ActionResult> Save(UserInfoModel model)
        {
            var cities = await _cityService.GetCities();
            ViewBag.Cities = cities.Content;

            if (string.IsNullOrEmpty(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Требуется поле телефон");
            }

            if (!ModelState.IsValid)
            {
                return await RenderEditView(model);
            }

            if (model.UserFullName != null && string.IsNullOrEmpty(model.UserFullName.Trim())) 
                model.UserFullName = model.Email;
                long? addressId = 0;

                if (model.DefaultAddressId != null)
                {
                    DeliveryAddressModel deliveryAddress = await AddressServiceClient.GetAddressById(model.DefaultAddressId);
                    deliveryAddress.RawAddress = model.DefaultAddress;
                    deliveryAddress.CityId = model.City;
                    deliveryAddress.StreetName = model.Street;
                    deliveryAddress.HouseNumber = model.House;
                    deliveryAddress.BuildingNumber = model.Building;
                    deliveryAddress.FlatNumber = model.Flat;
                    deliveryAddress.OfficeNumber = model.Office;
                    deliveryAddress.EntranceNumber = model.Entrance;
                    deliveryAddress.StoreyNumber = model.Storey;
                    deliveryAddress.IntercomNumber = model.Intercom;
                    deliveryAddress.AddressComment = model.AddressComment;
                    addressId = (await AddressServiceClient.UpdateAddress(deliveryAddress)).Id;
                }
                else
                    addressId = await AddressServiceClient.AddAddress(
                        new DeliveryAddressModel
                        {
                            RawAddress = model.DefaultAddress,
                            CityId = model.City,
                            StreetName = model.Street,
                            HouseNumber = model.House,
                            BuildingNumber = model.Building,
                            FlatNumber = model.Flat,
                            OfficeNumber = model.Office,
                            EntranceNumber = model.Entrance,
                            StoreyNumber = model.Storey,
                            IntercomNumber = model.Intercom,
                            AddressComment = model.AddressComment
        });
                model.DefaultAddressId = addressId;

            var result = await ProfileClient.SaveUserInfo(model);
            if (result.Succeeded)
            {
                if (result.Content != null)
                {
                    // FIXME: при редактировании профиля, персистентность куки сбрасывается.
                    // Нужно чтобы оно дублировалось из текущей куки (которую перезаписываем).
                    await HttpContext.SignInAsync(result.Content.AccessToken);
                    ModelState.Clear();
                    return RedirectToAction(nameof(Details));
                }
            }
            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                foreach (var error in result.Errors)
                {
                    var splitError = error.Split('&');
                    ModelState.AddModelError(splitError[0], splitError[1]);
                }
            }

            return await RenderEditView(model);
        }

        private async Task<ActionResult> RenderEditView(UserInfoModel model = null)
        {
            if (model == null)
            {
                var result = await ProfileClient.GetUserInfo();

                if (result.Succeeded)
                {
                    if (result.Content.UserFullName == string.Empty || result.Content.UserFullName == null)
                        result.Content.UserFullName = result.Content.Email;
                    var companies = await CompanyService.GetCompaniesByFilter(new CompaniesFilterModel()
                    {
                        CompanyId = result.Content.UserInCompanies?.Select(c => c.CompanyId).ToArray() ?? new long[] { }
                    });
                    ViewBag.Companies = companies.Content;
                    var cities = await _cityService.GetCities();
                    ViewBag.Cities = cities.Content;

                    return PartialView("_Edit", result.Content);
                }

                SetModelErrors(result);
                return PartialView("_Edit");
            }
            else
            {
                return PartialView("_Edit", model);
            }
        }

        //// TODO как нить доделать 
        //public async Task<ActionResult> RemoveExternalLogin(RemoveLoginModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return RedirectToAction("ExternalLogins");

        //    await ProfileClient.RemoveLogin(model);

        //    return RedirectToAction("ExternalLogins");
        //}

        //private IAuthenticationManager Authentication
        //{
        //    get { return Request.GetOwinContext().Authentication; }
        //}

        //public async Task<ActionResult> Referrals()
        //{
        //    return Redirect("/");
        //    Dictionary<int, List<UserReferralModel>> result = new Dictionary<int, List<UserReferralModel>>();
        //    var userId = long.Parse(User.Identity.GetUserId());
        //    var referrals = await UserServiceClient.GetUserReferrals(userId, new int[] { 1, 2, 3 });
        //    result = referrals.GroupBy(c => c.Level).OrderBy(h => h.Key).ToDictionary(g => g.Key, g => g.ToList());
        //    return View(result);
        //}
    }
}
