using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.AuthorizationService;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Controllers
{
    public class CityController : BaseController
    {
        public CityController(
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
            : base(accountService, profileClient, addressServiceClient,
                  banketsService, cafeService, companyService,
                  dishCategoryService, dishService, companyOrderServiceClient,
                  orderItemServiceClient, orderServiceClient, ratingServiceClient,
                  tagServiceClient, userServiceClient, menuServiceClient, cityService)
        {
        }

        [AjaxOnly, HttpPost]
        public async Task<IActionResult> CheckCityAvailable(string city, string region)
        {
            city = city.Trim().ToLower();
            region = region.Trim().ToLower().Replace("область", "обл").Replace("республика", "респ");
            var model = await CityService.GetCity(city, region) ?? await CityService.GetDefaultCity();
            var currentCity = HttpContext.Session.GetCurrentCity();
            if ((currentCity != null && currentCity.Id != model.Id) || currentCity is null)
            {
                HttpContext.Session.SetCurrentCity(model, false);
                return PartialView("_Geolocation", model);
            }
            else
            {
                return NoContent();
            }
        }

        [AjaxOnly]
        public async Task<IActionResult> ConfirmCurrentCity(long id)
        {
            var city = await CityService.GetCityById(id);
            HttpContext.Session.SetCurrentCity(city, true);
            return Ok();

        }

        [AjaxOnly]
        public async Task<IActionResult> Filter(string searchString)
        {
            var cities = await CityService.GetActiveCitiesForRegion(searchString);
            return PartialView("_CityList", cities);
        }

        [AjaxOnly]
        public IActionResult IsCityConfirmed() => Ok(HttpContext.Session.IsCityConfirmed());
    }
}
