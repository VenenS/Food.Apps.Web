using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;


namespace ITWebNet.Food.Site.Routing
{
    public static class MapRouteExtensions
    {
        public static IRouteBuilder MapCommonRoutes(this IRouteBuilder routes)
        {
            routes.MapRoute(
                name: "DefaultArea",
                template: "{area:exists}/{controller=Home}/{action=Index}/{id:long?}");

            routes.MapRoute(
                name: "Default",
                template: "{controller}/{action}/{id:long?}",
                defaults: new
                {
                    controller = "Cafe",
                    action = "Index",
                    namespaces = new[] { "ITWebNet.Food.Site.Controllers" }
                });

            routes.MapRoute(
                name: "HFU",
                template: "{action}/{id:long?}",
                defaults: new
                {
                    controller = "Cafe",
                    action = "Index",
                    namespaces = new[] { "ITWebNet.Food.Site.Controllers" }
                });

            routes.MapRoute(
                    name: "SubdomainRoute",
                    template: "{controller}/{action}",
                    defaults: new {
                        controller = "Cafe",
                        action = "Index",
                        namespaces = new[] { "ITWebNet.Food.Site.Controllers" }
                    });
            return routes;
        }
    }
}