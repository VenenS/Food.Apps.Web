using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITWebNet.Food.Site.Areas.Administrator.Controllers
{
    public class InfoController : BaseController
    {
        // GET: Administrator/Info
        public ActionResult Index()
        {
            return View();
        }
    }
}