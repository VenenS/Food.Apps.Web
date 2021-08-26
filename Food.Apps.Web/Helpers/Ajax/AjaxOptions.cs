using System;
using System.Collections.Generic;

namespace FoodApps.Web.NetCore.Helpers
{
    public class AjaxOptions
    {
        public string Confirm { get; set; }
        public InsertionMode? InsertionMode { get; set; }
        public string HttpMethod { get; set; }
        public string UpdateTargetId { get; set; }
        public string LoadingElementId { get; set; }
        public string Url { get; set; }
        public string OnSuccess { get; set; }
        public string OnBegin { get; set; }
        public string OnFailure { get; set; }
        public string OnComplete { get; set; }
        public int? LoadingElementDuration { get; set; }
        public bool AllowCache { get; set; }

        public Dictionary<string, string> ToUnobtrusiveHtmlAttributes()
        {
            var result = new Dictionary<string, string>();
            result.Add("data-ajax", "true");
            if (!string.IsNullOrEmpty(Confirm))
                result.Add("data-ajax-confirm", Confirm);
            if (InsertionMode.HasValue)
                result.Add("data-ajax-mode", InsertionMode.Value.ToString());
            if (!string.IsNullOrEmpty(HttpMethod))
                result.Add("data-ajax-method", HttpMethod);
            if (!string.IsNullOrEmpty(UpdateTargetId))
                result.Add("data-ajax-update", '#'+UpdateTargetId);
            if (!string.IsNullOrEmpty(LoadingElementId))
                result.Add("data-ajax-loading", '#'+LoadingElementId);
            if (!string.IsNullOrEmpty(Url))
                result.Add("data-ajax-url", Url);
            if (!string.IsNullOrEmpty(OnBegin))
                result.Add("data-ajax-begin", OnBegin);
            if (!string.IsNullOrEmpty(OnComplete))
                result.Add("data-ajax-complete", OnComplete);
            if (!string.IsNullOrEmpty(OnFailure))
                result.Add("data-ajax-failure", OnFailure);
            if (!string.IsNullOrEmpty(OnSuccess))
                result.Add("data-ajax-success", OnSuccess);
            if (LoadingElementDuration.HasValue)
                result.Add("data-ajax-loading-duration", LoadingElementDuration.ToString());
            if (AllowCache)
                result.Add("data-ajax-cache", "true");
            return result;
        }
    }
}
