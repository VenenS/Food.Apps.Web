using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITWebNet.Food.Site.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    [Authorize(Roles = "Admin")]
    public class BaseController : Controller
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //if (adminClient != null)
                //{
                //    adminClient.Close();
                //    adminClient = null;
                //}

            }

            base.Dispose(disposing);
        }

        protected void SetModelErrors(HttpResult result)
        {
            if (result.Succeeded)
                return;

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(string.Empty, item);
            }
        }
    }
}