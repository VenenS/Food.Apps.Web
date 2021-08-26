using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITWebNet.Food.Site.Services
{
    public class ModelError
    {
        public string Message { get; set; }

        public string MessageDetail { get; set; }

        public Dictionary<string, string[]> ModelState { get; set; }
    }
}