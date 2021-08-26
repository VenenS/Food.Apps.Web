using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.AuthorizationService;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ITWebNet.Food.Site.Controllers
{
    public class ErrorController : BaseController
    {
        public ErrorController(
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
            ICityService cityService)
            : base(accountSevice, profileClient, addressServiceClient, 
                  banketsService, cafeService, companyService,
                  dishCategoryService, dishService, companyOrderServiceClient, 
                  orderItemServiceClient, orderServiceClient, ratingServiceClient, 
                  tagServiceClient, userServiceClient, menuServiceClient, cityService)
        {
        }

        // GET: Error
        public ActionResult Index()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            return View("Error", exception);
        }

        public async Task<ActionResult> NotFound()
        {
            // Если страница кафе
            if (RouteData.Values.Keys.Contains("name"))
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
                // Добавляем путь.
                hostBuilder.Append("/Error/NotFound");
                return Redirect(hostBuilder.ToString());
            }
            Response.StatusCode = 404;
            return View();
        }

        public ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View();
        }

        public ActionResult InternalServerError()
        {
            Response.StatusCode = 500;
            return View();
        }
    }
}