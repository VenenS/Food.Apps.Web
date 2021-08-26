using Food.Services.Contracts;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface IReportService
    {
        Task<long> AddCafeToXslt(LayoutToCafeModel model);
        Task<List<LayoutToCafeModel>> AddCafeToXslt(long id);
        Task<long> AddXslt(XSLTModel model);
        Task<long> AddXsltAdm(XSLTModel model);
        Task<LayoutToCafeModel> DeleteCafeToXslt(long id);
        Task<bool> DeleteCafeToXsltConfirm(LayoutToCafeModel idLayout);
        Task<bool> EditXslt(XSLTModel model);
        Task<bool> EditXsltadm(XSLTModel model);
        Task<List<LayoutToCafeModel>> GetCafesToLayout(long modelId);
        Task<string> GetCompanyReportDataInJson(long companyId, DateTime startDate, DateTime endDate);
        Task<string> GetCompanyReportDataInXml(long? cafeId, DateTime startDate, DateTime endDate);
        Task<ReportDataModel> GetCompanyReportInHtml(ReportFilter filter, string sort);
        Task<ReportModel> GetCompanyReportInXLSX(ReportFilter filter, string sort);
        Task<string> GetCustomerReportDataInJson(long customerId, DateTime startDate, DateTime endDate);
        Task<string> GetCustomerReportDataInXml(long customerId, DateTime startDate, DateTime endDate);
        Task<List<ReportUserOrdersModel>> GetEmployeesReport(ReportFilter filter);
        Task<ReportModel> GetReportFileByFilter(ReportFilter filter);
        Task<ReportModel> GetReportOrdersAllEmployee(ReportFilter filter);
        Task<ReportModel> GetReportOrdersAllEmployeeInHtml(ReportFilter filter);
        Task<ReportModel> GetReportOrdersEmployee(ReportFilter filter);
        Task<ReportModel> GetReportOrdersEmployeeInHtml(ReportFilter filter);
        Task<ReportModel> GetUserDetailsReport(ReportFilter filter);
        Task<XSLTModel> GetXsltById(long modelId);
        Task<List<XSLTModel>> GetXsltFromCafe(long cafeId);
        Task<List<XSLTModel>> GetXsltList();
        Task<bool> RemoveXslt(XSLTModel model);
        Task<bool> RemoveXsltAdm(XSLTModel model);
        Task<bool> СheckUniqueNameXslt(string name);
        Task<bool> SendManagerComment(string comment, long id);
    }
}