using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ITWebNet.Food.Site.Services
{
    public class TokenError : IApiError
    {
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        [JsonProperty(PropertyName = "error_description")]
        public string ErrorDescription { get; set; }

        public Dictionary<string, string> GetErrorList()
        {
            return new Dictionary<string, string>
            {
                { Error, ErrorDescription }
            };
        }
    }
}