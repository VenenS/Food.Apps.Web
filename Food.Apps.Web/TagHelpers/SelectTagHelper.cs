using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodApps.Web.NetCore.TagHelpers
{
    [HtmlTargetElement("select", Attributes = "model-for")]
    public class SelectTagHelper : TagHelper
    {
        public ModelExpression ModelFor { get; set; }
        public string ModelId { get; set; }

        public string ModelName { get; set; }

        public string Selected { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Content.AppendHtml((await output.GetChildContentAsync(false)).GetContent());
            var selected = ModelFor.Model as string;
        }
    }
}
