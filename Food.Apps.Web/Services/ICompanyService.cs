using Food.Services.Contracts.Companies;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface ICompanyService
    {
        Task<HttpResult<HttpResponseMessage>> AddAddressToCompany(long id, DeliveryAddressModel address);
        Task<HttpResult<bool>> AddCompanyCurator(CompanyCuratorModel model);
        Task<bool> AddUserCompanyLink(long companyId, long userId);
        Task<HttpResult> CreateCompany(CompanyModel company);
        Task<bool> DeleteAddressFromCompany(long id);
        Task<bool> DeleteCompany(long id);
        Task<bool> DeleteCompanyCurator(long companyId, long userId);
        Task<bool> EditUserCompanyLink(long companyId, long oldCompanyId, long userId);
        Task<HttpResult<DeliveryAddressModel>> GetAddressCompanyById(long id);
        Task<List<CompanyModel>> GetAllCompanys();
        Task<HttpResult<List<CompanyModel>>> GetCompaniesByFilter(CompaniesFilterModel filter);
        Task<CompanyModel> GetCompanyById(long id);
        Task<List<CompanyModel>> GetCompanyByOrdersInfo(long cafeId, DateTime startDate, DateTime endDate);
        Task<HttpResult<CompanyCuratorModel>> GetCompanyCurator(long companyId, long id);
        Task<List<CompanyCuratorModel>> GetCompanyCurators(long companyId);
        Task<List<CompanyModel>> GetCompanys();
        Task<CompanyModel> GetCuratedCompany();
        Task<List<CompanyModel>> GetInactiveCompanies();
        Task<List<CompanyModel>> GetListOfCompanyByUserId(long userId);
        Task<List<CompanyModel>> GetMyCompanies();
        Task<ResponseModel> GetMyCompanyForOrder(long? cafeId = null);
        Task<bool> RemoveUserCompanyLink(long companyId, long userId);
        Task<bool> RestoreCompany(long id);
        Task<bool> SetSmsNotify(long id, bool EnableSms);
        Task<HttpResult<HttpResponseMessage>> UpdateAddressCompany(DeliveryAddressModel address);
        Task<HttpResult<CompanyModel>> UpdateCompany(CompanyModel model);
        Task<long?> GetUserCompanyId(long userId);
        Task<bool> SetUserCompany(long userId, long companyId);
    }
}