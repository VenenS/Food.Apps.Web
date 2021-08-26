using FoodApps.Web.NetCore.Helpers.Ajax;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FoodApps.Web.NetCore.Helpers
{
    public static class HtmlHelperAjaxExtensions
    {
        public static IHtmlContent AjaxFileUpload(
            this IHtmlHelper htmlHelper, 
            string action, 
            string controller, 
            object routeValues, 
            AjaxOptions ajaxOptions, 
            object htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder("input");

            var attributesDict = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            var urlHelper = GetUrlHelper(htmlHelper);
            string targetUrl = urlHelper.Action(
                action,
                controller,
                routeValues);
            tagBuilder.MergeAttributes(attributesDict);
            tagBuilder.MergeAttribute("type", "file");
            tagBuilder.MergeAttribute("name", "Images", true);
            tagBuilder.MergeAttribute("data-ajax-url", targetUrl);
            tagBuilder.MergeAttributes(ajaxOptions.ToUnobtrusiveHtmlAttributes());
            return tagBuilder.RenderBody();
        }

        public static IHtmlContent AjaxActionLink(
            this IHtmlHelper htmlHelper,
            string actionName,
            string controllerName,
            object routeValues,
            AjaxOptions ajaxOptions)
        {
            return AjaxActionLink(htmlHelper, null, actionName, controllerName, routeValues, ajaxOptions, null);
        }

        public static IHtmlContent AjaxActionLink(
            this IHtmlHelper htmlHelper, 
            string text, 
            string actionName, 
            string controllerName, 
            object routeValues, 
            AjaxOptions ajaxOptions)
        {
            return AjaxActionLink(htmlHelper, text, actionName, controllerName, routeValues, ajaxOptions, null);
        }

        public static TagBuilder AjaxActionLink(
            this IHtmlHelper htmlHelper, 
            string text, 
            string actionName, 
            string controllerName, 
            object routeValues, 
            AjaxOptions ajaxOptions, 
            object htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder("a");

            var attributesDict = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            var urlHelper = GetUrlHelper(htmlHelper);
            string targetUrl = urlHelper.Action(
                actionName,
                controllerName,
                routeValues);
            tagBuilder.InnerHtml.Append(text);
            tagBuilder.MergeAttributes(attributesDict);
            tagBuilder.MergeAttribute("data-ajax-url", targetUrl);
            tagBuilder.MergeAttributes(ajaxOptions.ToUnobtrusiveHtmlAttributes());
            return tagBuilder;
        }

        public static IHtmlContent AjaxIconRouteLink(
            this IHtmlHelper helper,
            string icon,
            string text,
            string routeName,
            object routeValues,
            object htmlAttributes)
        {
            var builder = new TagBuilder("i");
            builder.MergeAttribute("class", icon);
            var link = helper.AjaxRouteLink("", routeName, routeValues, new AjaxOptions(), htmlAttributes);
            link.InnerHtml.AppendHtml(builder).Append(text);
            return link;
        }

        public static IHtmlContent AjaxIconRouteLink(
            this IHtmlHelper helper,
            string icon,
            string text,
            string routeName,
            object routeValues,
            AjaxOptions ajaxOptions,
            object htmlAttributes)
        {
            var builder = new TagBuilder("i");
            builder.MergeAttribute("class", icon);
            var link = helper.AjaxRouteLink("", routeName, routeValues, ajaxOptions, htmlAttributes);
            link.InnerHtml.AppendHtml(builder).Append(text);
            return link;
        }

        public static IHtmlContent AjaxIconActionLink(
            this IHtmlHelper helper,
            string icon,
            string text,
            string actionName,
            string controllerName,
            object routeValues,
            AjaxOptions ajaxOptions,
            object htmlAttributes)
        {
            var builder = new TagBuilder("i");
            builder.MergeAttribute("class", icon);
            var link = helper.AjaxActionLink("", actionName, controllerName, routeValues, ajaxOptions, htmlAttributes);
            link.InnerHtml.AppendHtml(builder).Append(text);
            return link;
        }

        public static AjaxForm AjaxBeginRouteForm(
            this IHtmlHelper htmlHelper,
            string routeName,
            object routeValues,
            AjaxOptions ajaxOptions)
        {
            return AjaxBeginRouteForm(htmlHelper, routeName, routeValues, ajaxOptions, null);
        }

        public static AjaxForm AjaxBeginRouteForm(
            this IHtmlHelper htmlHelper,
            string routeName,
            object routeValues,
            AjaxOptions ajaxOptions,
            object htmlAttributes)
        {
            var builder = new TagBuilder("form");
            var urlHelper = GetUrlHelper(htmlHelper);
            ajaxOptions.Url = urlHelper.RouteUrl(routeName, routeValues);
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            builder.MergeAttributes(attributes);
            builder.MergeAttributes(ajaxOptions.ToUnobtrusiveHtmlAttributes());
            return new AjaxForm(htmlHelper, builder);
        }

        public static TagBuilder AjaxRouteLink(
            this IHtmlHelper htmlHelper,
            string text,
            string routeName,
            object routeValues,
            AjaxOptions ajaxOptions)
        {
            return htmlHelper.AjaxRouteLink(text, routeName, routeName, ajaxOptions, null);
        }

        public static TagBuilder AjaxRouteLink(
            this IHtmlHelper htmlHelper,
            string text,
            string routeName,
            object routeValues,
            AjaxOptions ajaxOptions,
            object htmlAttributes)
        {
            var builder = new TagBuilder("a");
            var urlHelper = GetUrlHelper(htmlHelper);
            var url = urlHelper.RouteUrl(routeName, routeValues);
            builder.MergeAttribute("data-ajax-url", url);
            builder.MergeAttributes(ajaxOptions.ToUnobtrusiveHtmlAttributes());
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            builder.MergeAttributes(attributes);
            builder.InnerHtml.Append(text);
            return builder;
        }
        
        public static AjaxForm AjaxBeginForm(
            this IHtmlHelper htmlHelper,
            string actionName,
            string controllerName,
            AjaxOptions ajaxOptions)
        {
            var builder = new TagBuilder("form");
            var urlHelper = GetUrlHelper(htmlHelper);
            ajaxOptions.Url = urlHelper.Action(actionName, controllerName);
            builder.MergeAttributes(ajaxOptions.ToUnobtrusiveHtmlAttributes());
            return new AjaxForm(htmlHelper, builder);
        }

        public static AjaxForm AjaxBeginForm(
            this IHtmlHelper htmlHelper,
            string actionName,
            object routeValues,
            AjaxOptions ajaxOptions)
        {
            var builder = new TagBuilder("form");
            var urlHelper = GetUrlHelper(htmlHelper);
            ajaxOptions.Url = urlHelper.Action(actionName, routeValues);
            builder.MergeAttributes(ajaxOptions.ToUnobtrusiveHtmlAttributes());
            return new AjaxForm(htmlHelper, builder);
        }

        public static AjaxForm AjaxBeginForm(
            this IHtmlHelper htmlHelper,
            string actionName,
            string controllerName,
            object routeValues,
            AjaxOptions ajaxOptions,
            object htmlAttributes)
        {
            var builder = new TagBuilder("form");
            var urlHelper = GetUrlHelper(htmlHelper);
            ajaxOptions.Url = urlHelper.Action(actionName, controllerName, routeValues);
            builder.MergeAttributes(ajaxOptions.ToUnobtrusiveHtmlAttributes());
            builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return new AjaxForm(htmlHelper, builder);
        }

        #region private

        private static IUrlHelper GetUrlHelper(IHtmlHelper htmlHelper)
        {
            var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
            var urlHelperFactory = (IUrlHelperFactory)serviceProvider.GetService(typeof(IUrlHelperFactory));
            return urlHelperFactory.GetUrlHelper(htmlHelper.ViewContext);
        }

        #endregion
    }
}
