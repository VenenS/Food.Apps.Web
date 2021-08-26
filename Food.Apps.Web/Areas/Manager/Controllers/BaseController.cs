using FoodApps.Web.NetCore.Extensions;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ITWebNet.Food.Site.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = "Manager")]
    public class BaseController : Controller
    {
        private readonly IMemoryCache cache;
        protected ICafeService _cafeService;

        public BaseController(IMemoryCache cache, ICafeService cafeService)
        {
            this.cache = cache;
            _cafeService = cafeService;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var idParam = RouteData.Values["cafeId"] ??
                              RouteData.Values["id"];
                if (idParam == null)
                    return;

                var id = long.Parse(idParam.ToString());
                if (!AsyncHelper.RunSync(() => _cafeService.IsUserOfManagerCafe(id)))
                {
                    filterContext.Result = NotFound();
                    return;
                }
                CafeModel cafe = null;
                if (cache.Get($"cafe-manager-{idParam}") == null)
                {
                    cafe = AsyncHelper.RunSync(() => _cafeService.GetCafeByIdIgnoreActivity(id));
                    cache.Set($"cafe-manager-{idParam}", cafe, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
                        Priority = CacheItemPriority.Normal
                    }); ;
                }
                else
                {
                    cafe = cache.Get<CafeModel>($"cafe-manager-{idParam}");
                }
                ViewBag.Cafe = cafe;
            }
        }
    }
}