using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Food.Services.Contracts.Companies;
using ITWebNet.Food.Core.DataContracts.Admin;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.Extensions.Configuration;

namespace ITWebNet.Food.Site.Services
{
    public class CompanyService : BaseClient<CompanyService>, ICompanyService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru//";
        private const string BaseUrl = "api/companies";
        public static string filterUrl = ServiceUrl + BaseUrl + "/filter";

        public CompanyService() : base(ServiceUrl)
        {
        }

        public CompanyService(string token) : base(ServiceUrl, token)
        {
        }

        public CompanyService(bool debug) : base(debug)
        {
        }

        public virtual async Task<bool> AddUserCompanyLink(long companyId, long userId)
        {
            var httpResult = await PostAsync<bool>($"{BaseUrl}/AddUserCompanyLink/{companyId}/{userId}", null);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return false;
        }

        public virtual async Task<HttpResult<bool>> AddCompanyCurator(CompanyCuratorModel model)
        {
            return await PostAsync<bool>($"{BaseUrl}/curators/add", model);
        }

        public virtual async Task<bool> DeleteCompanyCurator(long companyId, long userId)
        {
            var response = await HttpClient.DeleteAsync($"{BaseUrl}/{companyId}/curators/delete/{userId}");
            return response.IsSuccessStatusCode;
        }

        public virtual async Task<HttpResult<List<CompanyModel>>> GetCompaniesByFilter(CompaniesFilterModel filter)
        {
            return await PostAsync<List<CompanyModel>>(filterUrl, filter);
        }

        public virtual async Task<List<CompanyModel>> GetCompanys()
        {
            var httpResult = await GetAsync<List<CompanyModel>>($"{BaseUrl}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        /// <summary>
        /// + неактивные и удаленные
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<CompanyModel>> GetAllCompanys()
        {
            var httpResult = await GetAsync<List<CompanyModel>>($"{BaseUrl}/all");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<CompanyModel> GetCompanyById(long id)
        {
            var httpResult = await GetAsync<CompanyModel>($"{BaseUrl}", value: id);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<CompanyModel>> GetMyCompanies()
        {
            var httpResult = await GetAsync<List<CompanyModel>>($"{BaseUrl}/GetMyCompanies");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<ResponseModel> GetMyCompanyForOrder(long? cafeId = null)
        {
            var httpResult = await GetAsync<ResponseModel>($"{BaseUrl}/GetMyCompanyForOrder?cafeId={cafeId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }
            else
            {
                return new ResponseModel() { Message = "Не удалось получить список компаний для корпоративного заказа", Status = (int)httpResult.StatusCode };
            }
        }

        public virtual async Task<HttpResult> CreateCompany(CompanyModel company)
        {
            return await PostAsync<bool>(BaseUrl, company);
        }

        public virtual async Task<HttpResult<CompanyModel>> UpdateCompany(CompanyModel model)
        {
            return await PutAsync<CompanyModel>($"{BaseUrl}", model);
        }

        public virtual async Task<bool> DeleteCompany(long id)
        {
            var response = await HttpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        public virtual async Task<bool> RestoreCompany(long id)
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/{id}/restore");
            return response.IsSuccessStatusCode;
        }

        public virtual async Task<bool> RemoveUserCompanyLink(long companyId, long userId)
        {
            var httpResult = await DeleteAsync<bool>($"{BaseUrl}/RemoveUserCompanyLink/{companyId}/{userId}");
            return httpResult.Succeeded;
        }

        public virtual async Task<HttpResult<HttpResponseMessage>> AddAddressToCompany(long id, DeliveryAddressModel address)
        {
            var response = await PostAsync<HttpResponseMessage>($"{BaseUrl}/{id}/addresses", address);
            return response;
        }

        public virtual async Task<bool> DeleteAddressFromCompany(long id)
        {
            var response = await HttpClient.DeleteAsync($"{BaseUrl}/addresses/{id}");
            return response.IsSuccessStatusCode;
        }

        public virtual async Task<HttpResult<HttpResponseMessage>> UpdateAddressCompany(DeliveryAddressModel address)
        {
            var response = await PutAsync<HttpResponseMessage>($"{BaseUrl}/addresses", address);
            return response;
        }

        public virtual async Task<bool> EditUserCompanyLink(long companyId, long oldCompanyId, long userId)
        {
            var httpResult = await PutAsync<bool>($"{BaseUrl}/EditUserCompanyLink/{companyId}/{oldCompanyId}/{userId}", null);
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return false;
        }

        public virtual async Task<HttpResult<DeliveryAddressModel>> GetAddressCompanyById(long id)
        {
            return await GetAsync<DeliveryAddressModel>($"{BaseUrl}/addresses/{id}");
        }

        public virtual async Task<List<CompanyCuratorModel>> GetCompanyCurators(long companyId)
        {
            var response = await GetAsync<List<CompanyCuratorModel>>($"{BaseUrl}/{companyId}/curators");
            return response.Succeeded ?
                response.Content
                : new List<CompanyCuratorModel>();
        }

        public virtual async Task<HttpResult<CompanyCuratorModel>> GetCompanyCurator(long companyId, long id)
        {
            return await GetAsync<CompanyCuratorModel>($"{BaseUrl}/{companyId}/curators/{id}");
        }

        public virtual async Task<CompanyModel> GetCuratedCompany()
        {
            var httpResult = await GetAsync<CompanyModel>($"{BaseUrl}/GetCuratedCompany");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<CompanyModel>> GetInactiveCompanies()
        {
            var httpResult = await GetAsync<List<CompanyModel>>($"{BaseUrl}/InactiveCompanies");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<CompanyModel>> GetListOfCompanyByUserId(long userId)
        {
            var httpResult = await GetAsync<List<CompanyModel>>($"{BaseUrl}/GetListOfCompanyByUserId?userId={userId}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<List<CompanyModel>> GetCompanyByOrdersInfo(long cafeId, DateTime startDate, DateTime endDate)
        {
            var httpResult = await GetAsync<List<CompanyModel>>($"{BaseUrl}/GetCompanyByOrdersInfo?cafeId={cafeId}&startDate={DateTimeExtensions.ConvertToUnixTimestamp(startDate)}&endDate={DateTimeExtensions.ConvertToUnixTimestamp(endDate)}");
            if (httpResult.Succeeded)
            {
                return httpResult.Content;
            }

            return null;
        }

        public virtual async Task<bool> SetSmsNotify(long id, bool EnableSms)
        {
            try
            {
                var response = await HttpClient.GetAsync($"{BaseUrl}/SetSmsNotify/{id.ToString()}?EnableSms={EnableSms.ToString()}");
                return response.IsSuccessStatusCode;
            }
            catch //(Exception ex)
            {
                return false;
            }

        }

        public async Task<bool> SetUserCompany(long userId, long companyId)
        {
            var response = await GetAsync<bool>($"{BaseUrl}/SetUserCompany?userId={userId}&companyId={companyId}");
            return response.Content;
        }

        public async Task<long?> GetUserCompanyId(long userId)
        {
            var response = await GetAsync<long?>($"{BaseUrl}/GetUserCompanyId?userId={userId}");
            return response.Content;
        }
    }
}