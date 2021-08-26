using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Food.Apps.Web.Attributes;
using Food.Services.Models;
using FoodApps.Web.NetCore.Extensions;
using ITWebNet.Food.Core.DataContracts.Account;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.AuthorizationService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace ITWebNet.Food.Site.Controllers
{
    public class AccountController : BaseController
    {

        ICafeOrderNotificationService _cafeOrderNotificationService;

        public AccountController(
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
        }

        //TODO: Авторизация через соцсети. Удалить
        //[HttpGet]
        //public ActionResult ExternalLoginCallback(string returnUrl, string externalAccessToken, string loginProvider)
        //{
        //    return new EmptyResult();

        //    /*
        //    if (string.IsNullOrWhiteSpace(externalAccessToken))
        //        return RedirectToAction("Index", "Cafe");

        //    // externalAccessToken возвращаемый сервисом зашифрован для избежания
        //    // утечки токена, поэтому перед использованием его необходимо расшифровать.
        //    var decryptedToken = externalAccessToken.Unprotect();
        //    var authTicket = Startup.OAuthOptions.AccessTokenFormat.Unprotect(decryptedToken);
        //    Authentication.SignIn(authTicket.Properties, authTicket.Identity);

        //    if (string.IsNullOrEmpty(loginProvider) || loginProvider.Equals(ClaimsIdentity.DefaultIssuer))
        //        return RedirectToLocal(returnUrl, false);

        //    return RedirectToAction("RegisterExternal", "Account", new { returnUrl = returnUrl, loginProvider = loginProvider });
        //    */
        //}


        [HttpGet]
        public async Task<IActionResult> Authorize(string token, string returnUrl, string referralLink)
        {
            await HttpContext.SignInAsync(token);
            if (!User.Identity.IsAuthenticated)
                return Forbid();
            if (!string.IsNullOrEmpty(referralLink))
            {
                var parent = UserServiceClient.GetUserByReferralLink(referralLink);
                if (parent != null)
                {
                    await UserServiceClient.AddUserReferralLink(parent.Id, User.Identity.GetUserId<long>());
                }
            }
            if (string.IsNullOrWhiteSpace(returnUrl))
                returnUrl = "/";
            return RedirectToLocal(returnUrl, false);
        }

        [HttpGet]
        public async Task<IActionResult> AuthorizeCallback(string token, long userId)
        {
            var model = await AccountService.ValidateResetPasswordToken(userId, token);
            if (model.Content == null)
                return RedirectToAction("ForgotPassword", "Account", new { authFailed = true });

            return RedirectToAction("ResetPassword", new { email = model.Content.Email, code = model.Content.Code });
        }

        [HttpGet]
        public ActionResult ForgotPassword(bool authFailed = false)
        {
            if (User.Identity.IsAuthenticated && !User.Identity.IsAnonymous())
                return Redirect("/");
            if (authFailed)
                ModelState.AddModelError("", "Не удалось авторизоваться, получите новую ссылку для получения доступа к сайту.");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            ModelState.Clear();
            model.ReturnUrl = Url.Action("AuthorizeCallback", "Account", null, Request.Scheme);

            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await AccountService.ForgotPassword(model);

            if (result.Succeeded)
                ViewBag.ResultMessage = "Письмо с инструкцией по восстановлению отправлено на указанный email. Если письмо не приходит слишком долго - попробуйте запросить его вновь.";

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                ViewBag.ResultMessage =
                    string.Format(
                        "E-mail адрес не подтвержден. Для повторной отправки сообщения, перейдите по <a href=\"{0}\">ссылке</a>",
                        Url.Action("SendEmailConfirmation", "Account"));
            }

            SetModelErrors(result);

            return View(model);
        }

        [HttpPost("account/login")]
        [MatchQueryParam("type", "password")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginWithPassword(LoginModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            //TODO: Авторизация через соцсети. Удалить
            //ViewBag.LoginButtons = await GetExternalLoginButtons(returnUrl);

            if (!ModelState.IsValid)
            {
                if (ModelState.Any(m =>
                    m.Key == "Password" && m.Value.Errors.Count > 0))
                {
                    TempData["ErrorLogin"] = "Неверное имя пользователя или пароль.";
                }
                return View("Login", new LoginViewModel() { Login = model, LoginSms = new LoginSmsViewModel() });
            }

            var result = await AccountService.Login(model);

            if (result.Succeeded)
            {
                if (result.Content != null)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var securityToken = tokenHandler.ReadToken(result.Content.AccessToken) as JwtSecurityToken;

                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    var extraClaims = securityToken.Claims.Where(c => !identity.Claims.Any(x => x.Type == c.Type)).ToList();
                    extraClaims.Add(new Claim("jwt", result.Content.AccessToken));
                    identity.AddClaims(extraClaims);
                    var authenticationProperties = new AuthenticationProperties()
                    {
                        IssuedUtc = DateTimeOffset.FromUnixTimeSeconds(long.Parse(identity.Claims.First(c => c.Type == JwtRegisteredClaimNames.Nbf)?.Value)),
                        ExpiresUtc = DateTimeOffset.FromUnixTimeSeconds(long.Parse(identity.Claims.First(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value)),
                        IsPersistent = false
                    };
                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticationProperties);

                    if (User.IsInRole("Admin"))
                    {
                        returnUrl = "/";
                    }

                    if (string.IsNullOrWhiteSpace(returnUrl))
                        returnUrl = "/";

                    // Если номер мобильного не подтвержден перебрасываем на страницу ЛК
                    var user = await UserServiceClient.GetUserByEmail(model.Email);
                    if (!user.PhoneNumberConfirmed || !user.EmailConfirmed)
                        return RedirectToAction("Index", "Profile");

                    return RedirectToLocal(returnUrl, false);
                }
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                ViewBag.ResultMessage =
                    string.Format(
                        "E-mail адрес не подтвержден. Для повторной отправки сообщения, перейдите по <a href=\"{0}\">ссылке</a>",
                        Url.Action("SendEmailConfirmation", "Account"));
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                ModelState.AddModelError(string.Empty,
                    "Ошибка сервера. Пожалуйста, попробуйте ещё раз позже.");
            }
            else if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                TempData["ErrorLogin"] = "Неверное имя пользователя или пароль.";
            }
            else
            {
                SetModelErrors(result);
            }

            return View("Login", new LoginViewModel() { Login = model, LoginSms = new LoginSmsViewModel() });
        }

        /// <summary>
        /// Отправить SMS код пользователю
        /// </summary>
        [HttpPost("account/login")]
        [MatchQueryParam("type", "sms")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginSendSmsCode([Required] string phone, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            //TODO: Авторизация через соцсети. Удалить
            //ViewBag.LoginButtons = await GetExternalLoginButtons(returnUrl);

            if (!ModelState.IsValid)
                return View("Login", new LoginViewModel());

            //Вызов метода отправки СМС кода пользователю
            //если успешно, то запускаем таймер 
            //иначе выдаем сообщение об ошибке

            var viewModel = new LoginSmsViewModel() { Phone = phone };
            var response = await _cafeOrderNotificationService.SendSmsCode(phone);
            if (response.Status == 0)
            {
                viewModel.IsSendCode = true;
                viewModel.Message = new MessageViewModel() { Text = "Сообщение с кодом было отправлено на ваш телефон", Type = "success" };
            }
            else
            {
                viewModel.IsSendCode = false;
                viewModel.Message = new MessageViewModel() { Text = "Ошибка: " + response.Message, Type = "danger" };
            }

            return View("Login", new LoginViewModel() { LoginSms = viewModel });
        }

        /// <summary>
        /// Отправить SMS код пользователю (Используется в кнопке "Отправить еще раз")
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> LoginSendSmsCodeAgain(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return BadRequest();

            MessageViewModel viewModel;
            var response = await _cafeOrderNotificationService.SendSmsCode(phone);
            if (response.Status == 0)
            {
                viewModel = new MessageViewModel() { Text = "Сообщение с кодом было отправлено на ваш телефон", Type = "success" };
            }
            else
            {
                viewModel = new MessageViewModel() { Text = "Ошибка: " + response.Message, Type = "danger" };
                Response.StatusCode = 500;
            }

            return PartialView("_Message", viewModel);
        }

        /// <summary>
        /// Авторизация пользователя через SMS код
        /// </summary>
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginSms(LoginSmsViewModel viewModel, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            //TODO: Авторизация через соцсети. Удалить
            //ViewBag.LoginButtons = await GetExternalLoginButtons(returnUrl);

            if (!ModelState.IsValid)
                return PartialView("_Message", new MessageViewModel() { Text = "Данные заполнены некорректно", Type = "danger" });

            //вызываем метод авторизации
            //если успешно, то редиректим на главную страницу
            var result = await AccountService.LoginSms(new LoginSmsModel()
            {
                Phone = viewModel.Phone,
                SmsCode = viewModel.SmsCode
            });

            if (result.Status == 0)
            {
                if (result.Result != null)
                {

                    var tokenModel = JsonConvert.DeserializeObject<TokenModel>(result.Result.ToString());
                    await HttpContext.SignInAsync(tokenModel.AccessToken);

                    if (User.IsInRole("Admin"))
                        returnUrl = "/";

                    if (string.IsNullOrWhiteSpace(returnUrl))
                        returnUrl = "/";
                    return RedirectToLocal(returnUrl, true);
                }
            }

            if (result.Status == 2)
                return PartialView("_Message", new MessageViewModel() { Text = result.Message, Type = "warning" });

            return PartialView("_Message", new MessageViewModel() { Text = result.Message, Type = "danger" });
        }

        [HttpGet("account/login")]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl = "/", string message = null)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl.ToLowerInvariant().StartsWith("/error"))
            {
                returnUrl = "/";
            }
            //TODO: Авторизация через соцсети. Удалить
            if (User.Identity.IsAuthenticated && !User.Identity.IsAnonymous()
                                              /*&& !User.Identity.IsExternal()*/)
                return Redirect("/");
            ViewBag.ResultMessage = TempData["ResultMessage"];
            ViewBag.ReturnUrl = returnUrl;
            //TODO: Авторизация через соцсети. Удалить
            //ViewBag.LoginButtons = await GetExternalLoginButtons(returnUrl);
            ViewBag.ErrorMessage = message;

            return View(new LoginViewModel()
            {
                Login = new LoginModel(),
                LoginSms = new LoginSmsViewModel()
            });
        }

        [HttpGet]
        public async Task<ActionResult> SendEmailConfirmation()
        {
            return View(new EmailConfirmationModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendEmailConfirmation(EmailConfirmationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.ReturnUrl = Url.Action("ConfirmEmail", "Account", null, Request.Scheme);
            var result = await AccountService.SendEmailConfirmation(model);

            if (result.Succeeded)
                ViewBag.ResultMessage = "Письмо с подтверждением было отправлено на указанный email.";

            SetModelErrors(result);

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> LoginAnonymous(string returnUrl)
        {
            var result = await AccountService.LoginAnonymous();

            if (result.Succeeded)
            {
                if (result.Content != null)
                {
                    await HttpContext.SignInAsync(result.Content.AccessToken);
                    if (string.IsNullOrWhiteSpace(returnUrl))
                        returnUrl = "/";
                }
                return RedirectToLocal(returnUrl, false);
            }
            else
                // Чтобы не было бесконечного перенаправления - перенаправляем на страницу обычного логина:
                return RedirectToAction(nameof(Login), new { returnUrl });
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            CartMultiCafeViewModel.ClearAllCarts(HttpContext.Session);
            if (RouteData.Values.ContainsKey("name"))
            {
                // Создаем ссылку на страницу ошибки без субдомена.
                var hostBuilder = new System.Text.StringBuilder();
                // Добавляем http или https.
                hostBuilder.Append($"{HttpContext.Request.Scheme}://");
                var config = HttpContext.RequestServices?.GetRequiredService<IConfiguration>();
                var domainName = config?.GetSection("AppSettings:Domain")?.Value.ToLowerInvariant();
                // Добавляем домен.
                hostBuilder.Append(domainName);
                // Добавляем порт при наличии.
                if (HttpContext.Request.Host.Port != null)
                    hostBuilder.Append(":").Append(HttpContext.Request.Host.Port.Value.ToString());
                return Redirect(hostBuilder.ToString());
            }
            return Redirect("/");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Register(string returnUrl, string referralLink)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl.ToLowerInvariant().StartsWith("/error"))
            {
                returnUrl = "/";
            }
            ViewBag.ReturnUrl = returnUrl;
            //TODO: Авторизация через соцсети. Удалить
            if (User.Identity.IsAuthenticated && !User.Identity.IsAnonymous()/* && !User.Identity.IsExternal()*/)
                return Redirect("/");
            //var refer = await FoodClient.GetUserByReferralLinkAsync(referralLink);
            //if (refer != null)
            //    ViewBag.Refer = refer;
            return View(new RegisterModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.ReturnUrl = Url.Action("ConfirmEmail", "Account", null, Request.Scheme);
            var result = await AccountService.Register(model);
            if (result.Succeeded && result.Content != null)
            {
                ViewBag.ResultMessage = "На ваш электронный ящик было выслано письмо с подтверждением регистрации";
            }

            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Json(new { result = true, content = result, redirectUrl = returnUrl });

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                ModelState.AddModelError(string.Empty, "Ошибка сервера. Пожалуйста, попробуйте ещё раз позже.");
            else
                SetModelErrors(result);
            return View(model);
        }

        //TODO: Авторизация через соцсети. Удалить
        //[HttpGet]
        //public ActionResult RegisterExternal(string returnUrl, string loginProvider)
        //{
        //    //Authentication.SignOut(OAuthDefaults.AuthenticationType);
        //    if (User.Identity.IsAuthenticated && !User.Identity.IsExternal())
        //        return Redirect("/");
        //    ViewBag.LoginProvider = loginProvider;
        //    ViewBag.ReturnUrl = returnUrl;
        //    return View(new RegisterExternalModel());
        //}

        //TODO: Авторизация через соцсети. Удалить
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        ////[OverrideAuthorization]
        ////[AuthorizeWithAnonymous(AllowAnonymous = false, AllowExternal = true)]
        //public async Task<ActionResult> RegisterExternal(RegisterExternalModel model, string returnUrl)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    var result = await AccountSevice.RegisterExternal(model);

        //    if (result.Succeeded && result.Content != null)
        //    {
        //        await HttpContext.SignInAsync(result.Content.AccessToken);

        //        return Redirect("/");
        //    }

        //    SetModelErrors(result);
        //    return View(model);
        //}

        [HttpGet]
        public ActionResult ResetPassword(string email, string code)
        {
            return View(new ResetPasswordModel { Email = email, Code = code });
        }

        //[AjaxOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await AccountService.ResetPassword(model);
            if (result.Succeeded)
            {
                TempData["ResultMessage"] = result.Content;
                return RedirectToAction("Login", "Account");
            }
            SetModelErrors(result);

            return View(model);
        }

        //TODO: Авторизация через соцсети. Удалить
        //private async Task<Dictionary<string, ExternalLoginModel>> GetExternalLoginButtons(string returnUrl)
        //{
        //    Dictionary<string, ExternalLoginModel> loginButtons = null;

        //    string redirectUrl = Url.Action(
        //        "ExternalLoginCallback",
        //        null,
        //        new { returnUrl = returnUrl },
        //        Request.Scheme);

        //    var result = await AccountSevice.ExternalLogins(WebUtility.UrlEncode(redirectUrl));
        //    if (result.Succeeded)
        //        loginButtons = result.Content.ToDictionary(b => b.Name.ToLowerInvariant());
        //    else
        //        SetModelErrors(result);

        //    return loginButtons;
        //}

        private ActionResult RedirectToLocal(string returnUrl, bool isAjax)
        {
            if (isAjax)
                return Json(new { redirectUrl = returnUrl });

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Cafe");
        }

        [HttpGet]
        public async Task<ActionResult> ConfirmEmail(long userId, string code)
        {
            var res = await AccountService.ConfirmEmail(userId, code);
            if (res?.Type == "success")
            {
                TempData["ResultMessage"] = res.Result;
                return RedirectToAction(nameof(Login));
            }

            if (res?.Type == "error")
                return RedirectToAction(nameof(Login), new { message = res.Result });
            return RedirectToAction(nameof(Login));
        }
    }
}
