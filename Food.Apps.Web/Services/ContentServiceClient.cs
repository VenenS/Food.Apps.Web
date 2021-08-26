using ITWebNet.Food.Site.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services.Client
{
    public class ContentServiceClient : BaseClient<ContentServiceClient>, IContentServiceClient
    {
        private readonly string _url;
        private const string _prefix = "images/";
        private string ImageUri => _url.EndsWith("/") ? _url + _prefix : _url + "/" + _prefix;

        public ContentServiceClient(IConfiguration config)
            : base(config.GetValue<string>("AppSettings:ContentService"))
        {
            _url = config.GetValue<string>("AppSettings:ContentService") ??
                throw new InvalidOperationException("Content service url is missing");
        }

        public ContentServiceClient(bool debug) : base(debug)
        {
        }

        public string GetImage(GetImageModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Hash))
                return null;
            if (model.Hash.Contains("data:image/"))
                return model.Hash;
            string path = ImageUri + model.ToString() + "/";

            return path;
        }

        public virtual async Task<HttpResult<string>> PostImage(PostImageModel image)
        {
            return await PostAsync<string>(_prefix, image);
        }

        public async Task<HttpResult<string>> PostImage(string filename, Stream stream)
        {
            byte[] bytes;

            using (var memStream = new MemoryStream()) {
                await stream.CopyToAsync(memStream).ConfigureAwait(false);
                bytes = memStream.ToArray();
            }

            return await PostImage(new PostImageModel { Content = bytes, Filename = filename }).ConfigureAwait(false);
        }
    }
}