using Food.Services.Contracts;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public class ReportService : BaseClient<ReportService>, IReportService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static string BaseUrl = ServiceUrl + "api/reports";

        public ReportService() : base(ServiceUrl)
        {
        }

        public ReportService(string baseAddress) : base(baseAddress)
        {
        }

        public ReportService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public ReportService(bool debug) : base(debug)
        {
        }

        public virtual async Task<long> AddXslt(XSLTModel model)
        {
            var response = await PutAsync<long>($"{BaseUrl}/xslt", model);
            return response.Content;
        }

        public virtual async Task<bool> EditXslt(XSLTModel model)
        {
            var response = await PostAsync<bool>($"{BaseUrl}/xslt", model);
            return response.Content;
        }

        public virtual async Task<string> GetCompanyReportDataInJson(long companyId, DateTime startDate, DateTime endDate)
        {
            var response = await GetAsync<string>($"{BaseUrl}/companyreport/json/{companyId}/from/{DateTimeExtensions.ConvertToUnixTimestamp(startDate)}/to/{DateTimeExtensions.ConvertToUnixTimestamp(endDate)}");
            return response.Content;
        }

        public virtual async Task<string> GetCompanyReportDataInXml(long? cafeId, DateTime startDate, DateTime endDate)
        {
            string cafeIdPart = cafeId != null ? $"/{cafeId}" : "";
            var response = await GetAsync<string>($"{BaseUrl}/companyreport/xml{cafeIdPart}/from/{DateTimeExtensions.ConvertToUnixTimestamp(startDate)}/to/{DateTimeExtensions.ConvertToUnixTimestamp(endDate)}");
            return response.Content;
        }

        public virtual async Task<string> GetCustomerReportDataInJson(long customerId, DateTime startDate, DateTime endDate)
        {
            var response = await GetAsync<string>($"{BaseUrl}/customerreport/json/{customerId}/from/{DateTimeExtensions.ConvertToUnixTimestamp(startDate)}/to/{DateTimeExtensions.ConvertToUnixTimestamp(endDate)}");
            return response.Content;
        }

        public virtual async Task<string> GetCustomerReportDataInXml(long customerId, DateTime startDate, DateTime endDate)
        {
            var response = await GetAsync<string>($"{BaseUrl}/customerreport/xml/{customerId}/from/{DateTimeExtensions.ConvertToUnixTimestamp(startDate)}/to/{DateTimeExtensions.ConvertToUnixTimestamp(endDate)}");
            return response.Content;
        }

        public virtual async Task<ReportModel> GetReportFileByFilter(ReportFilter filter)
        {
            var response = await PostAsync<ReportModel>($"{BaseUrl}/getreportbyfilter", filter);
            return response.Content;
        }
        public async Task<ReportModel> GetUserDetailsReport(ReportFilter filter)
        {

            var response = await PostAsync<ReportModel>($"{BaseUrl}/getuserorderdetailsreport", filter);
            return response.Content;
        }

        public virtual async Task<List<XSLTModel>> GetXsltFromCafe(long cafeId)
        {
            var response = await GetAsync<List<XSLTModel>>($"{BaseUrl}/xlstfromcafe/{cafeId}");
            return response.Content;
        }

        public virtual async Task<bool> RemoveXslt(XSLTModel model)
        {
            var response = await PostAsync<bool>($"{BaseUrl}/removexlst", model);
            return response.Content;
        }

        /// <summary>
        /// Получение Html отчета всех заказов для компании по фильтру для Куратора
        /// </summary>
        /// <param name="filter">Фильтр</param>
        /// <param name="sort">Поле для сортировки</param>
        /// <returns></returns>
        public virtual async Task<ReportDataModel> GetCompanyReportInHtml(ReportFilter filter, string sort)
        {
            var response = await PostAsync<ReportDataModel>($"{BaseUrl}/companyreport/html?sort={sort}", filter);
            return response.Content;
        }

        // Получить xlsx файл всех заказов для компании
        public virtual async Task<ReportModel> GetCompanyReportInXLSX(ReportFilter filter, string sort)
        {
            var response = await PostAsync<ReportModel>($"{BaseUrl}/companyreport/xlsx?sort={sort}", filter);
            return response.Content;
        }

        //Отчет xlsx по заказам сотрудника компании
        public virtual async Task<ReportModel> GetReportOrdersEmployee(ReportFilter filter)
        {
            var response = await PostAsync<ReportModel>($"{BaseUrl}/orderreport/user/xlsx", filter);
            return response.Content;
        }

        //Отчет xlsx по заказам сотрудников компании
        public virtual async Task<ReportModel> GetReportOrdersAllEmployee(ReportFilter filter)
        {
            var response = await PostAsync<ReportModel>($"{BaseUrl}/orderreport/users/xlsx", filter);
            return response.Content;
        }

        //Отчет html по заказам сотрудника компании
        public virtual async Task<ReportModel> GetReportOrdersEmployeeInHtml(ReportFilter filter)
        {
            var response = await PostAsync<ReportModel>($"{BaseUrl}/getreportbyfilter/consolidator/user", filter);
            return response.Content;
        }

        //Отчет html по заказам сотрудников компании
        public virtual async Task<ReportModel> GetReportOrdersAllEmployeeInHtml(ReportFilter filter)
        {
            var response = await PostAsync<ReportModel>($"{BaseUrl}/getreportbyfilter/consolidator/users", filter);
            return response.Content;
        }

        public virtual async Task<List<ReportUserOrdersModel>> GetEmployeesReport(ReportFilter filter)
        {
            var response = await PostAsync<List<ReportUserOrdersModel>>($"{BaseUrl}/getEmployeesReport", filter);
            return response.Content;
        }
        public virtual async Task<List<XSLTModel>> GetXsltList()
        {
            var response = await GetAsync<List<XSLTModel>>($"{BaseUrl}/getxlstList");
            return response.Content;
        }
        public virtual async Task<long> AddXsltAdm(XSLTModel model)
        {
            var response = await PutAsync<long>($"{BaseUrl}/addxsltadm", model);
            return response.Content;
        }
        public virtual async Task<bool> СheckUniqueNameXslt(string name)
        {
            var response = await GetAsync<bool>($"{BaseUrl}/uniquexsltadm?name={name}");
            return response.Content;
        }

        /// <summary>
        /// Добавление комментария менеджера к заказу
        /// </summary>
        /// <param name="comment">коммент который надо в БД запихнуть</param>
        /// <param name="id">id заказа</param>
        /// <returns></returns>
        public virtual async Task<bool> SendManagerComment(string comment, long id)
        {
            var response = await GetAsync<bool>($"{BaseUrl}/sendcomment/manager?id={id}&comment={comment}");
            return response.Content;
        }

        public virtual async Task<bool> EditXsltadm(XSLTModel model)
        {
            var response = await PostAsync<bool>($"{BaseUrl}/editxsltadm", model);
            return response.Content;
        }
        public virtual async Task<bool> RemoveXsltAdm(XSLTModel model)
        {
            var response = await PostAsync<bool>($"{BaseUrl}/removexlstadm", model);
            return response.Content;
        }
        public virtual async Task<XSLTModel> GetXsltById(long modelId)
        {
            var response = await GetAsync<XSLTModel>($"{BaseUrl}/getxsltbyidadm?modelId={modelId}");
            return response.Content;
        }
        public virtual async Task<List<LayoutToCafeModel>> GetCafesToLayout(long modelId)
        {
            var response = await GetAsync<List<LayoutToCafeModel>>($"{BaseUrl}/getxslttocafeadm?layoutId={modelId}");
            return response.Content;
        }
        public virtual async Task<List<LayoutToCafeModel>> AddCafeToXslt(long id)
        {
            var response = await GetAsync<List<LayoutToCafeModel>>($"{BaseUrl}/addcafexlstadm?id={id}");
            return response.Content;
        }
        public virtual async Task<long> AddCafeToXslt(LayoutToCafeModel model)
        {
            var response = await PostAsync<long>($"{BaseUrl}/addxslttocafeadm", model);
            return response.Content;
        }
        public virtual async Task<LayoutToCafeModel> DeleteCafeToXslt(long id)
        {
            var response = await GetAsync<LayoutToCafeModel>($"{BaseUrl}/delxslttocafeadm?idLayToCafe={id}");
            return response.Content;
        }
        public virtual async Task<bool> DeleteCafeToXsltConfirm(LayoutToCafeModel idLayout)
        {
            var response = await PostAsync<bool>($"{BaseUrl}/delxslttocafeconfirmadm", idLayout);
            return response.Content;
        }
    }
}