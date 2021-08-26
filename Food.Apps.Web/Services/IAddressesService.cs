using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface IAddressesService
    {
        Task<long?> AddAddress(DeliveryAddressModel model);
        Task<HttpResult<DeliveryAddressModel>> CreateAddress(DeliveryAddressModel model);
        Task<bool> DeleteAddress(long id);
        Task<DeliveryAddressModel> GetAddressById(long? id);
        Task<HttpResult<List<DeliveryAddressModel>>> GetAddresses();
        Task<List<DeliveryAddressModel>> GetCompanyAddresses(long companyId);
        Task<DeliveryAddressModel> UpdateAddress(DeliveryAddressModel model);
    }
}