using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using FoodApps.Web.NetCore.Extensions;

namespace ITWebNet.Food.Site.Attributes
{
    public class AjaxOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
                filterContext.HttpContext.Response.StatusCode = 403;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}