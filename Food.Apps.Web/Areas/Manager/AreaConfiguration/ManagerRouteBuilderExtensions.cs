
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace ITWebNet.Food.Site.Areas.Manager
{
    public static class ManagerRouteBuilderExtensions 
    {

        public static IRouteBuilder MapManagerRoutes(this IRouteBuilder routes)
        {
            routes.MapAreaRoute(
                name: "ManagerCafe",
                areaName: "Manager",
                template: "Manager/Cafe/{cafeId:long}/{controller=Cafe}/{action=Index}/{id?}");
            routes.MapAreaRoute(
                name: "Manager",
                areaName: "Manager",
                template: "Manager/{controller=Cafe}/{action=Index}/{id?}");
            return routes;
        }
    }
}
