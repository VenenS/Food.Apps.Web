using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITWebNet.Food.Site
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    sealed class LogErrorAttribute : Attribute, IExceptionFilter
    {
        private ILogger logger;

        public LogErrorAttribute(ILogger logger)
        {
            this.logger = logger;
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            string controllerName = (string)filterContext.RouteData.Values["controller"];
            string actionName = (string)filterContext.RouteData.Values["action"];
            string id = (string)filterContext.RouteData.Values["id"];
            string area = (string)filterContext.RouteData.DataTokens["area"];

            string queryParams = filterContext.HttpContext.Request.QueryString.ToString();
            string bodyParams = null;
            if (filterContext.HttpContext.Request.ContentType != null)
                bodyParams = filterContext.HttpContext.Request.Form.ToString();

            string message = string.Format("Exception thrown in {1}{0}Area: {2},\tController: {3},\tAction: {4},\tId: {5}{0}Query:{0}{6}{0}Body:{0}{7}",
                Environment.NewLine,
                filterContext.HttpContext.Request.Path,
                area,
                controllerName,
                actionName,
                id,
                queryParams,
                bodyParams);

            logger.LogError(filterContext.Exception, message);
        }
    }
}