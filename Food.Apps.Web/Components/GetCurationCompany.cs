using ITWebNet.Food.Site;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Threading.Tasks;

namespace FoodApps.Web.NetCore.Components
{
    public class GetCurationCompanyViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // FIXME: получать CompanyService через DI.
            var token = User.Identity.GetJwtToken();
            var company = await new CompanyService(token).GetCuratedCompany();

            if (company != null)
                return View("LeftBar", company);
            return Content("");
        }
    }
}
