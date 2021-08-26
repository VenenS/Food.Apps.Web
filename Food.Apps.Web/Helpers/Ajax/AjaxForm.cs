using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodApps.Web.NetCore.Helpers.Ajax
{
    public class AjaxForm : IDisposable
    {
        private bool isDisposed = false;
        private readonly IHtmlHelper helper;

        public AjaxForm(IHtmlHelper helper, TagBuilder builder = null)
        {
            this.helper = helper;
            if (builder != null)
                this.helper.ViewContext.Writer.Write(builder.RenderStartTag());
                
        }

        ~AjaxForm()
        {
            Dispose(false);
        }



        public void Dispose(bool fromDisposedMethod)
        {
            if (!isDisposed)
            {
                if (fromDisposedMethod)
                {
                    helper.EndForm();
                }
                isDisposed = true;
            }
                        
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
