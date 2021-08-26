using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Areas.Administrator.Controllers
{
    public class SitesController : BaseController
    {
        private static string Host = ConfigurationManager.AppSettings["Domain"] ?? "edovoz.com";
        private static string Application = ConfigurationManager.AppSettings["ClientApplication"];
        private static string Code = "SujsCYg4";

        private ICafeService _cafeService;

        public SitesController(ICafeService cafeService)
        {
            _cafeService = cafeService;
        }

        public ActionResult Index()
        {
            using (var iis = new ServerManager())
            {
                return View("InfoIIS", iis.Sites.Select(s => s.Name).ToList());
            }
        }

        [HttpGet]
        public async Task<ActionResult> Bindings()
        {
            using (var iis = new ServerManager())
            {
                var site = iis.Sites.FirstOrDefault(s =>
                    string.Equals(s.Name, Application, StringComparison.CurrentCultureIgnoreCase));

                if (site == null)
                {
                    return View("InfoIIS", new List<string>());
                }

                return View("InfoIIS", site.Bindings.Select(c => c.BindingInformation).ToList());
            }
        }

        [HttpPost]
        public async Task<ActionResult> Bindings(string code)
        {
            if (string.IsNullOrEmpty(code) || code != Code)
                return BadRequest();
            
            using (var iis = new ServerManager())
            {
                var site = iis.Sites.FirstOrDefault(s =>
                    string.Equals(s.Name, Application, StringComparison.CurrentCultureIgnoreCase));
                if (site == null)
                {
                    return StatusCode(500);
                }
                var token = User.Identity.GetJwtToken();
                ((CafeService)_cafeService).AddAuthorization(token);
                var cafes = await _cafeService.GetCafes();
                foreach (var cafe in cafes)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(cafe.CleanUrlName) ||
                            Regex.IsMatch(cafe.CleanUrlName, @"\p{IsCyrillic}"))
                        {
                            continue;
                        }
                        var cleanUrl = cafe.CleanUrlName.Replace(" ", "-");
                        var binding = $"*:80:{cleanUrl}.{Host}";
                        if (site.Bindings.FirstOrDefault(c =>
                                string.Equals(c.BindingInformation, binding,
                                    StringComparison.CurrentCultureIgnoreCase)) == null)
                        {
                            site.Bindings.Add(binding, "http");
                        }
                        var wwwwBinding = $"*:80:www.{cleanUrl}.{Host}";
                        if (site.Bindings.FirstOrDefault(c =>
                                string.Equals(c.BindingInformation, wwwwBinding,
                                    StringComparison.CurrentCultureIgnoreCase)) == null)
                        {
                            site.Bindings.Add(wwwwBinding, "https");
                        }
                        var sslBinding = $"*:443:{cleanUrl}.{Host}";
                        if (site.Bindings.FirstOrDefault(c =>
                                string.Equals(c.BindingInformation, sslBinding,
                                    StringComparison.CurrentCultureIgnoreCase)) == null)
                        {
                            site.Bindings.Add(sslBinding, "http");
                        }
                        var wwwSslBinding = $"*:443:www.{cleanUrl}.{Host}";
                        if (site.Bindings.FirstOrDefault(c =>
                                string.Equals(c.BindingInformation, wwwSslBinding,
                                    StringComparison.CurrentCultureIgnoreCase)) == null)
                        {
                            site.Bindings.Add(wwwSslBinding, "https");
                        }
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }
                iis.CommitChanges();
                return Ok();
            }
        }
    }
}