using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace ITWebNet.Food.Site.Routing
{
    public class SubdomainRouter : IRouter
    {
        private static string _subdomainRouteKey = "name";

        private readonly IRouter _target;
        private readonly IConfiguration _config;
        private readonly string _domain;
        private readonly string _suffix;

        public SubdomainRouter(IRouter target, IConfiguration config)
        {
            _target = target;
            _config = config;
            _domain = config.GetSection("AppSettings:Domain")?.Value?.ToLowerInvariant() ?? "edovoz.com";
            _suffix = "." + _domain;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return _target.GetVirtualPath(context);
        }

        public Task RouteAsync(RouteContext context)
        {
            var host = context.HttpContext.Request.Host.Host.ToLowerInvariant();
            if (host.EndsWith(_suffix)) {
                // XXX: для упрощения, делаем допущение что может быть только один
                // уровень субдоменов, поэтому кладем все что перед хостом в результат.
                var subdomain = host.Substring(0, host.Length - _suffix.Length);
                context.RouteData.Values.TryAdd(_subdomainRouteKey, subdomain);
            }
            return _target.RouteAsync(context);
        }
    }
}
