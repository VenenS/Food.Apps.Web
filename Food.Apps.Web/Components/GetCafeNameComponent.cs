using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FoodApps.Web.NetCore.Components
{
    public class GetCafeNameViewComponent : ViewComponent
    {
        public async Task<string> InvokeAsync(string cleanUrlName)
        {
            var cafe = await new CafeService().GetCafeByCleanUrl(cleanUrlName);
            return cafe.Name;
        }
    }
}
