using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ITWebNet.Food.Site.Areas.Administrator
{
    public class AdministratorRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
		{
			//TODO: Авторизация через соцсети. Удалить
			// httpContext.User.Identity.IsExternal() всегда возвращает false, см. реализацию.

			return httpContext.User.Identity.IsAuthenticated
				   && httpContext.User.IsInRole("Admin")
				   && !httpContext.User.Identity.IsAnonymous();
			//TODO: Авторизация через соцсети. Удалить
			//&& !httpContext.User.Identity.IsExternal();
		}
    }
}