using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace ITWebNet.Food.Site.Services
{
    public class HttpResult<T> : HttpResult
    {


        public T Content { get; set; }

        public HttpResult()
            : base()
        {
        }

        protected HttpResult(IList<string> errors)
            : base(errors)
        {

        }

        public HttpResult(T content)
            : base()
        {
            this.Content = content;
        }

        public string Message { get { return Content as string; } }

        public static HttpResult<T> Success(T content)
        {
            return new HttpResult<T>(content);
        }

        public static new HttpResult<T> Failure(string error)
        {
            return new HttpResult<T>(new string[] { error });
        }

        public static new HttpResult<T> Failure(IList<string> errors)
        {
            return new HttpResult<T>(errors);
        }

        public void Deconstruct(out T data, out string err)
        {
            data = Content;
            err = Errors.Count > 0 ? string.Join('\n', Errors) : null;
        }
    }
}