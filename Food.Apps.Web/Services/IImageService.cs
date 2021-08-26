using ITWebNet.Food.Core.DataContracts.Common;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface IImageService
    {
        Task<bool> AddImage(string hash, long cafeId, long objectId, ObjectTypesEnum type);
        Task<bool> RemoveImage(string hash, long cafeId, long objectId, ObjectTypesEnum type);
    }
}