using ITWebNet.Food.Site.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;

namespace ITWebNet.Food.Site
{
    public static class UrlHelperExtensions
    {
        /// <summary>
        ///   По аналогии с UrlHelper.Action возвращает ссылку на Action указанного контроллера.
        ///   Отличие в том что сформированная ссылка не содержит субдомен.
        /// </summary>
        public static string ActionWithoutSubdomain(this IUrlHelper helper, string action, string controller)
        {
            var url = helper.Action(
                action,
                controller,
                null,
                helper.ActionContext?.HttpContext.Request.Scheme ?? "http"  // В тестах RequestContext == null.
            );

            var uri = new UriBuilder(url);
            var result = uri.Host.Split(new char[] { '.' }, 2);

            // UriBuilder по-умолчанию включает номер порта в конечную ссылку за исключением
            // случая когда номер порта == -1.
            uri.Port = !uri.Uri.IsDefaultPort ? uri.Port : -1;

            if (result.Length > 1)
            {
                uri.Host = result[1];
                return uri.ToString();
            }
            else
            {
                return url;
            }
        }

        public static string RouteUrl(this IUrlHelper urlHelper, string routeName, RouteValueDictionary routeValues, bool subdomain)
        {
            if (subdomain)
            {
                var httpContext = urlHelper.ActionContext.HttpContext;
                var config = urlHelper.ActionContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var domainName = config.GetSection("AppSettings:Domain")?.Value.ToLowerInvariant();
                var routeData = urlHelper.ActionContext.RouteData;
                var hostBuilder = new StringBuilder();

                var controllerName = routeValues["controller"]?.ToString() ?? routeData.Values["controller"]?.ToString();
                var actionName = routeValues["action"]?.ToString() ?? routeData.Values["action"]?.ToString();
                var subdomainName = routeValues["name"]?.ToString() ?? routeData.Values["name"]?.ToString();

                if (!string.IsNullOrEmpty(subdomainName))
                    hostBuilder.Append(subdomainName).Append(".").Append(domainName);
                else
                    hostBuilder.Append(domainName);

                if (httpContext.Request.Host.Port != null)
                    hostBuilder.Append(":").Append(httpContext.Request.Host.Port.Value.ToString());

                var hostname = hostBuilder.ToString().ToLowerInvariant();

                if (routeData.Routers != null)
                {
                    var dict = new RouteValueDictionary();
                    foreach (var item in routeValues) {
                        if (item.Key != "name")
                            dict.Add(item.Key, item.Value);
                    }

                    var url = urlHelper.Action(actionName, controllerName, dict, httpContext.Request.Scheme, hostname);
                    return url;
                }
            }
            return urlHelper.RouteUrl(routeName, routeValues);
        }
    }
}
