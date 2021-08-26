using ITWebNet.Food.Site.Routing;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site
{
    public static class HtmlHelperExtensions
    {
        public static string Id(this IHtmlHelper htmlHelper)
        {

            object id;
            var routeData = htmlHelper.ViewContext.RouteData.Values;
            if (routeData.TryGetValue("id", out id))
                return id as string;
            else if ((id = htmlHelper.ViewContext.HttpContext.Request.Query["id"]) != null)
                return id as string;

            return string.Empty;
        }

        public static string Controller(this IHtmlHelper htmlHelper)
        {
            var routeValues = htmlHelper.ViewContext.RouteData.Values;

            object controller;
            if (routeValues.TryGetValue("controller", out controller))
                return controller as string;
            return string.Empty;
        }

        public static string Action(this IHtmlHelper htmlHelper)
        {
            var routeValues = htmlHelper.ViewContext.RouteData.Values;

            object action;
            if (routeValues.TryGetValue("action", out action))
                return action as string;
            return string.Empty;
        }

        public static string Active(this IHtmlHelper htmlHelper, string action)
        {
            string currentAction = htmlHelper.ViewContext.RouteData.Values["action"].ToString();

            bool isActive = action.ToLower().Equals(currentAction.ToLower());
            return isActive ? " active" : string.Empty;
        }

        public static string Active(this IHtmlHelper htmlHelper, string action, string controller)
        {
            var routeData = htmlHelper.ViewContext.RouteData.Values;
            string currentAction = routeData["action"].ToString().ToLower();
            string currentController = routeData["controller"].ToString().ToLower();

            bool isActive = action.ToLower().Equals(currentAction) && controller.ToLower().Equals(currentController);
            return isActive ? " active" : string.Empty;
        }

        public static string ActiveRoute(this IHtmlHelper htmlHelper, RouteValueDictionary routeValues)
        {
            var notActive = string.Empty;

            if (routeValues == null)
                return notActive;

            var routeData = htmlHelper.ViewContext.RouteData;
            foreach (var item in routeValues)
            {
                var routeItemValue = routeData.Values[item.Key];
                if (routeItemValue == null ||
                    !string.Equals(routeItemValue.ToString(), item.Value.ToString(), StringComparison.CurrentCultureIgnoreCase))
                {
                    return notActive;
                }
            }

            return "active";
        }

        public static IHtmlContent ActiveActionLink(
            this IHtmlHelper htmlHelper,
            string linkText,
            string actionName,
            string controllerName)
        {
            return ActiveActionLink(htmlHelper, linkText, actionName, controllerName, null);
        }

        public static IHtmlContent ActiveActionLink(
            this IHtmlHelper htmlHelper,
            string linkText,
            string actionName,
            string controllerName,
            object htmlAttributes)
        {
            var routeData = htmlHelper.ViewContext.RouteData.Values;
            string currentAction = routeData["action"].ToString();
            string currentController = routeData["controller"].ToString();

            IDictionary<string, object> htmlAttributesDict;

            if (htmlAttributes != null)
                htmlAttributesDict = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            else
                htmlAttributesDict = new RouteValueDictionary();

            bool isActive = actionName.Equals(currentAction) && controllerName.Equals(currentController);

            object classVal;
            if (htmlAttributesDict.TryGetValue("class", out classVal) && isActive)
            {
                if (!(classVal as string).Contains("active"))
                    htmlAttributesDict["class"] += " active";
            }
            else if (isActive)
                htmlAttributesDict.Add("class", "active");

            return htmlHelper.ActionLink(linkText, actionName, controllerName, null, htmlAttributesDict);
        }

        /*public static IHtmlContent IconActionLink(
            this IHtmlHelper helper,
            string icon,
            string text,
            string actionName,
            string controllerName,
            object routeValues,
            object htmlAttributes)
        {
            var builder = new TagBuilder("i");
            builder.MergeAttribute("class", icon);
            var link = helper.ActionLink("[replaceme] " + text, actionName, controllerName, routeValues, htmlAttributes);
            return new HtmlString(link.Replace("[replaceme]", builder.ToString()));
        }*/

        public static IHtmlContent Action(this IHtmlHelper helper, string action, object parameters = null)
        {
            var controller = (string)helper.ViewContext.RouteData.Values["controller"];

            return Action(helper, action, controller, parameters);
        }

        public static IHtmlContent Action(this IHtmlHelper helper, string action, string controller, object parameters = null)
        {
            var area = (string)helper.ViewContext.RouteData.Values["area"];

            return Action(helper, action, controller, area, parameters);
        }

        public static IHtmlContent Action(this IHtmlHelper helper, string action, string controller, string area, object parameters = null)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (controller == null)
                throw new ArgumentNullException("controller");


            var task = RenderActionAsync(helper, action, controller, area, parameters);

            return task.Result;
        }

        public static IHtmlContent Phone(this IHtmlHelper helper, string phone, object htmlAttributes = null)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;

            IDictionary<string, object> htmlAttributesDict;

            if (htmlAttributes != null)
                htmlAttributesDict = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            else
                htmlAttributesDict = new RouteValueDictionary();

            TagBuilder tagBuilder;
            string formattedPhone = new string(phone.Where(c => char.IsDigit(c)).ToArray());
            if (formattedPhone.Length == 10)
                formattedPhone.Insert(0, "7");
            if (formattedPhone.Length == 11)
            {
                if (formattedPhone.StartsWith('8'))
                {
                    formattedPhone.Remove(0, 1);
                    formattedPhone.Insert(0, "7");

                }
                tagBuilder = new TagBuilder("a");
                tagBuilder.Attributes.Add("href", $"tel:+{formattedPhone}");
                tagBuilder.InnerHtml.Append($"{long.Parse(formattedPhone):+# (###) ### ####}");
            }
            else
            {
                tagBuilder = new TagBuilder("span");
                tagBuilder.InnerHtml.Append(phone);
            }

            tagBuilder.MergeAttributes(htmlAttributesDict);
            return tagBuilder;
        }

        private static async Task<IHtmlContent> RenderActionAsync(this IHtmlHelper helper, string action, string controller, string area, object parameters = null)
        {
            // fetching required services for invocation
            var serviceProvider = helper.ViewContext.HttpContext.RequestServices;
            var actionContextAccessor = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IActionContextAccessor>();
            var httpContextAccessor = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IHttpContextAccessor>();
            var actionSelector = serviceProvider.GetRequiredService<IActionSelector>();

            // creating new action invocation context
            var routeData = new RouteData();
            foreach (var router in helper.ViewContext.RouteData.Routers)
            {
                routeData.PushState(router, null, null);
            }
            routeData.PushState(null, new RouteValueDictionary(new { controller = controller, action = action, area = area }), null);
            routeData.PushState(null, new RouteValueDictionary(parameters ?? new { }), null);

            //get the actiondescriptor
            RouteContext routeContext = new RouteContext(helper.ViewContext.HttpContext) { RouteData = routeData };
            var candidates = actionSelector.SelectCandidates(routeContext);
            var actionDescriptor = actionSelector.SelectBestCandidate(routeContext, candidates);

            var originalActionContext = actionContextAccessor.ActionContext;
            var originalhttpContext = httpContextAccessor.HttpContext;
            try
            {
                var newHttpContext = serviceProvider.GetRequiredService<IHttpContextFactory>().Create(helper.ViewContext.HttpContext.Features);
                if (newHttpContext.Items.ContainsKey(typeof(IUrlHelper)))
                {
                    newHttpContext.Items.Remove(typeof(IUrlHelper));
                }
                newHttpContext.Response.Body = new MemoryStream();
                var actionContext = new ActionContext(newHttpContext, routeData, actionDescriptor);
                actionContextAccessor.ActionContext = actionContext;
                var invoker = serviceProvider.GetRequiredService<IActionInvokerFactory>().CreateInvoker(actionContext);
                await invoker.InvokeAsync();
                newHttpContext.Response.Body.Position = 0;
                using (var reader = new StreamReader(newHttpContext.Response.Body))
                {
                    return new HtmlString(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                return new HtmlString(ex.Message);
            }
            finally
            {
                actionContextAccessor.ActionContext = originalActionContext;
                httpContextAccessor.HttpContext = originalhttpContext;
                if (helper.ViewContext.HttpContext.Items.ContainsKey(typeof(IUrlHelper)))
                {
                    helper.ViewContext.HttpContext.Items.Remove(typeof(IUrlHelper));
                }
            }
        }
    }
}