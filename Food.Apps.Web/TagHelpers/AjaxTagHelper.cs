using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodApps.Web.NetCore.TagHelpers
{
    [HtmlTargetElement("form", Attributes = "ajax-*")]
    [HtmlTargetElement("a", Attributes = "ajax-*")]
    [HtmlTargetElement("input", Attributes = "ajax-*")]
    public class AjaxTagHelper : TagHelper
    {
        public string AjaxMode { get; set; }
        public string AjaxLoading { get; set; }
        public string AjaxLoadingDuration { get; set; }
        public string AjaxUpdate { get; set; }
        public string AjaxUrl { get; set; }
        public string AjaxCache { get; set; }
        public string AjaxConfirm { get; set; }
        public string AjaxBegin { get; set; }
        public string AjaxSuccess { get; set; }
        public string AjaxFailure { get; set; }
        public string AjaxComplete { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            output.Attributes.SetAttribute("data-ajax", true);
            if (AjaxMode != null) output.Attributes.SetAttribute("data-ajax-mode", AjaxMode);
            if (AjaxUrl != null) output.Attributes.SetAttribute("data-ajax-url", AjaxUrl);
            if (AjaxLoading != null) output.Attributes.SetAttribute("data-ajax-loading", AjaxLoading);
            if (AjaxLoadingDuration != null) output.Attributes.SetAttribute("data-ajax-loading-duration", AjaxLoadingDuration);
            if (AjaxConfirm != null) output.Attributes.SetAttribute("data-ajax-confirm", AjaxConfirm);
            if (AjaxUpdate != null) output.Attributes.SetAttribute("data-ajax-update", AjaxUpdate);
            if (AjaxCache != null) output.Attributes.SetAttribute("data-ajax-cache", AjaxCache);
            if (AjaxBegin != null) output.Attributes.SetAttribute("data-ajax-begin", AjaxBegin);
            if (AjaxSuccess != null) output.Attributes.SetAttribute("data-ajax-success", AjaxSuccess);
            if (AjaxFailure != null) output.Attributes.SetAttribute("data-ajax-failure", AjaxFailure);
            if (AjaxComplete != null) output.Attributes.SetAttribute("data-ajax-complete", AjaxComplete);
        }
    }
}
