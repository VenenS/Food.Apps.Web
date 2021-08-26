using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Web;

namespace ITWebNet.Food.Site.Services
{
    public class StringHttpResult : HttpResult<string>
    {
        public StringHttpResult()
            : base()
        {
        }

        protected StringHttpResult(IList<string> errors)
            : base(errors)
        {

        }

        public string Message { get { return Content; } }

        public static new StringHttpResult Failure(string error)
        {
            return new StringHttpResult(new string[] { error });
        }

        public static new StringHttpResult Failure(IList<string> errors)
        {
            return new StringHttpResult(errors);
        }
    }
}