
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace ITWebNet.Food.Site.Areas.Administrator
{
    public static class AdministratorRouteBuilderExtensions
    {
        public static IRouteBuilder MapAdministratorRoutes(this IRouteBuilder routes)
        {
            routes.MapAreaRoute(
                name: "AdministretorCompanyCurators",
                areaName: "Administrator",
                template: "companies/{companyId}/curators/{action}/{id?}",
                defaults: new { action = "Index", namespaces = new[] { "ITWebNet.Food.Site.Areas.Administrator.Controllers" } },
                constraints: new { area = new AdministratorRouteConstraint() });

            routes.MapAreaRoute(
                name: "AdministratorCafeManagers",
                areaName: "Administrator",
                template: "cafes/{cafeId}/managers/{action}/{id?}",
                defaults: new { action = "Index", namespaces = new[] { "ITWebNet.Food.Site.Areas.Administrator.Controllers" } },
                constraints: new { area = new AdministratorRouteConstraint() });

            routes.MapAreaRoute(
                name: "AdministratorReports",
                areaName: "Administrator",
                template: "cafes/{cafeId}/{controller=Reports}/reports/{action}/{id?}",
                defaults: new { action = "Index", namespaces = new[] { "ITWebNet.Food.Site.Areas.Administrator.Controllers" } },
                constraints: new { area = new AdministratorRouteConstraint() });

            routes.MapAreaRoute(
                name: "AdministratorDefault",
                areaName: "Administrator",
                template: "{controller}/{action}/{id?}",
                defaults: new { controller = "Info", action = "Index", namespaces = new[] { "ITWebNet.Food.Site.Areas.Administrator.Controllers" } },
                constraints: new { area = new AdministratorRouteConstraint() });
            return routes;
        }
    }
}