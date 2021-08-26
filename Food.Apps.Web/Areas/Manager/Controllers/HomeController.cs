using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITWebNet.Food.Site.Areas.Manager.Controllers;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace ITWebNet.Food.Site.Areas.Manager.Controllers
{
    public class HomeController : BaseController
    {
        [ActivatorUtilitiesConstructor]
        public HomeController(IMemoryCache cache, ICafeService cafeService) : base(cache, cafeService)
        {
        }

        public async Task<IActionResult> Index()
        {
            var cafes = await _cafeService.GetManagedCafes();
            if (cafes != null && cafes.Count == 1)
                return RedirectToAction("Index", "Cafe", new { cafeId = cafes[0].Id });

            return View(cafes);
        }
    }
}