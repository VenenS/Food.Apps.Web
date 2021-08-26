using ITWebNet.Food.Core.DataContracts.Manager;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface IKitchenService
    {
        Task<bool> AddKitchenToCafe(long cafeId, long kitchenId);
        Task<KitchenModel[]> GetCurrentListOfKitchenToCafe(long cafeId);
        Task<KitchenModel[]> GetFullListOfKitchen();
        Task<bool> RemoveKitchenFromCafe(long cafeId, long kitchenId);
    }
}