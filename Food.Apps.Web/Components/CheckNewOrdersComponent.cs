using ITWebNet.Food.Site;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System;
using System.Threading.Tasks;

namespace FoodApps.Web.NetCore.Components
{
    public class CheckNewOrdersViewComponent : ViewComponent
    {
        public async Task<ViewViewComponentResult> InvokeAsync()
        {
            var userId = User.Identity.GetUserId();
            var response = AsyncHelper.RunSync(() =>
                new CafeOrderNotificationService().GetNotificationForUser(Convert.ToInt64(userId)));
            return await Task.Run(() => View("_NewCaffeOrdersDone", response));
        }
    }
}
