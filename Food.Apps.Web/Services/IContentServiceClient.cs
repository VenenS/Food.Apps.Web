using System.IO;
using System.Threading.Tasks;
using ITWebNet.Food.Site.Models;

namespace ITWebNet.Food.Site.Services.Client
{
    public interface IContentServiceClient
    {
        string GetImage(GetImageModel model);
        Task<HttpResult<string>> PostImage(PostImageModel image);
        Task<HttpResult<string>> PostImage(string filename, Stream stream);
    }
}