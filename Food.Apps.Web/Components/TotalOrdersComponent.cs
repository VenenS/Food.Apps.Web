using ITWebNet.Food.Site;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FoodApps.Web.NetCore.Components
{
    public class TotalOrdersViewComponent : ViewComponent
    {
        public async Task<long> InvokeAsync()
        {
            var token = User.Identity.GetJwtToken();
            var orderService = new OrderService().AddAuthorization(token);
            var totalToday = await orderService.GetTotalOrdersPerDay();
            return totalToday;
        }

    }
}
