using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.AuthorizationService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ITWebNet.Food.Site.Controllers
{
    public class InfoController : BaseController
    {
        private readonly IConfiguration _configuration;

        public InfoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [ActivatorUtilitiesConstructor]
        public InfoController(
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
            _configuration = configuration;
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Feedback(string returnUrl)
        {
            FeedbackViewModel model = new FeedbackViewModel
            {
                UserName = User.Identity.GetUserFullName(),
                Email = User.Identity.GetUserEmail(),
                Url = HttpContext.Request.Headers["Referer"].FirstOrDefault()
            };

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Feedback(string returnUrl, FeedbackViewModel model, [FromServices] IHostingEnvironment env = null)
        {
            if (env != null && env.IsProduction())
            {
                var userResponse = Request.Query["g-recaptcha-response"];
                string privateKey = _configuration.GetSection("RecaptchaPrivateKey").Value;

                using (var wc = new WebClient())
                {
                    var parameters = new NameValueCollection();
                    parameters.Add("format", "json");
                    parameters.Add("secret", privateKey);
                    parameters.Add("response", userResponse);
                    string recaptchaResult = Encoding.Default.GetString(wc.UploadValues("https://www.google.com/recaptcha/api/siteverify", parameters));
                    if (string.IsNullOrEmpty(recaptchaResult) || recaptchaResult.ToLower().Contains("false"))
                        ModelState.AddModelError("RecaptchaId", "Не пройдена каптча");
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            model.Message = string.Format("{1}\nСтраница: {0}", model.Url, model.Message);

            await UserServiceClient.SendFeedback(new Core.DataContracts.Common.FeedbackModel()
            {
                UserName = model.UserName,
                Email = model.Email,
                Title = model.Title,
                Message = model.Message
            });

            ViewBag.ResultMessage = "Ваше сообщение успешно отправлено. "
                + "Мы свяжемся с вами в ближайшее время.";

            if (string.IsNullOrWhiteSpace(returnUrl))
                return View(model);

            return View(model);
        }

        public ActionResult Partner()
        {
            return View();
        }

        public ActionResult PizzaDelivery()
        {
            return View();
        }

        public ActionResult SushiDelivery()
        {
            return View();
        }

        public ActionResult FoodDelivery()
        {
            return View();
        }

        public ActionResult Payment()
        {
            return View();
        }

        public ActionResult Agreement()
        {
            return View();
        }

        public ActionResult ReferralProgramShortDescription()
        {
            return View();
        }

        public ActionResult ReferralProgramDescription()
        {
            return Redirect("/");
        }
    }
}