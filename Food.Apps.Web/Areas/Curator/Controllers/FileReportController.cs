using Food.Services.Contracts;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Areas.Curator.Controllers
{
    public class FileReportController : BaseController
    {
        private ICompanyService companyService;
        private IReportService reportServiceClient;
        private IUsersService usersService;

        public FileReportController(ICompanyService companyService, IReportService reportServiceClient, IUsersService usersService)
        {
            this.companyService = companyService;
            this.reportServiceClient = reportServiceClient;
            this.usersService = usersService;
        }

        public async Task<ActionResult> Index()
        {
            var company = await companyService.GetCuratedCompany();
            var companyUsers = await usersService.GetUsersByCafeOrCompany(OrganizationTypeEnum.Company, company.Id);
            var reportsModel = new ReportsModel();
            reportsModel.Filter = new ReportViewFilter().InitDefaults();
            reportsModel.Filter.Company = company;
            reportsModel.Filter.Users = new SelectList(companyUsers, "Id", "Name");

            if (TempData.ContainsKey("ModelState"))
            {
                var modelState = (ModelStateDictionary)TempData["ModelState"];
                ModelState.Merge(modelState);
            }

            return View("Index", reportsModel);
        }

        [HttpPost]
        public async Task<ActionResult> GetFilterFile(ReportViewFilter filter, long companyId)
        {
            if (!ModelState.IsValid)
            {
                TempData["ModelState"] = ModelState;
                return RedirectToAction("Index");
            }

            if (filter.SortType == SortType.OrderByEmployeeName)
            {
                if (filter.ActionReportType == ActionReportType.ActionGetXLSX)
                {
                    return await GetReportEmployeeXLSX(companyId, filter);
                }
                if (filter.ActionReportType == ActionReportType.ActionPrint)
                {
                    return await GetReportEmployeeHtml(companyId, filter);
                }
            }

            if (filter.SortType == SortType.OrderByAllEmployee)
            {
                if (filter.ActionReportType == ActionReportType.ActionGetXLSX)
                {
                    return await GetReportAllEmployeeXLSX(companyId, filter);
                }
                if (filter.ActionReportType == ActionReportType.ActionPrint)
                {
                    return await GetReportAllEmployeeHtml(companyId, filter);
                }
            }

            var reportFilter = filter.AsReportFilter(null, companyId);
            if (filter.ActionReportType == ActionReportType.ActionPrint)
            {
                var model = await reportServiceClient.GetCompanyReportInHtml(reportFilter, filter.SortType.ToString());
                return View("PrintGeneralReport", model);
            }
            else
            {
                reportFilter.CompanyId = companyId;
                return await GetCompanyReportInXLSX(filter.SortType.ToString(), reportFilter);
            }
        }

        

        /// <summary>
        /// Получить xlsx файл всех заказов для компании
        /// </summary>
        public async Task<FileResult> GetCompanyReportInXLSX(string sort, ReportFilter reportFilter = null)
        {
            try
            {
                var result = await reportServiceClient.GetCompanyReportInXLSX(reportFilter, sort);
                if (result != null && result.FileBody != null)
                {
                    return File(result.FileBody, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.FileName + ".xlsx");
                }
                else
                {
                    //string message = "Не удалось получить файл";
                    //TempData["message"] = message;
                    //return RedirectToAction("Index");
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Получить xlsx файл заказов для сотрудника
        /// </summary>
        public async Task<FileResult> GetReportEmployeeXLSX(long companyId, ReportViewFilter reportFilter = null)
        {
            try
            {
                var result = await reportServiceClient.GetReportOrdersEmployee(reportFilter.AsReportFilter(null, companyId));
                if (result != null && result.FileBody != null)
                {
                    return File(result.FileBody, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.FileName + ".xlsx");
                }
                else
                {
                    //string message = "Не удалось получить файл";
                    //TempData["message"] = message;
                    //return RedirectToAction("Index");
                    return null;
                }
            }
            catch(Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Получить xlsx файл заказов для всех сотрудников
        /// </summary>
        public async Task<FileResult> GetReportAllEmployeeXLSX(long companyId, ReportViewFilter reportFilter = null)
        {
            try
            {
                var result = await reportServiceClient.GetReportOrdersAllEmployee(reportFilter.AsReportFilter(null, companyId));
                if (result != null && result.FileBody != null)
                {
                    return File(result.FileBody, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.FileName + ".xlsx");
                }
                else
                {
                    //string message = "Не удалось получить файл";
                    //TempData["message"] = message;
                    //return RedirectToAction("Index");
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Получить html файл заказов для сотрудника
        /// </summary>
        public async Task<ActionResult> GetReportEmployeeHtml(long companyId, ReportViewFilter reportFilter = null)
        {
            var report = await reportServiceClient.GetReportOrdersEmployeeInHtml(reportFilter.AsReportFilter(null, companyId));

            if (report == null)
                return RedirectToAction("Index");

            return View("PrintReportForEmployee", (object)Encoding.UTF8.GetString(report.FileBody));
        }

        /// <summary>
        /// Получить html файл заказов для всех сотрудников
        /// </summary>
        public async Task<ActionResult> GetReportAllEmployeeHtml(long companyId, ReportViewFilter reportFilter = null)
        {
            var report = await reportServiceClient.GetReportOrdersAllEmployeeInHtml(reportFilter.AsReportFilter(null, companyId));

            if (report == null)
                return RedirectToAction("Index");

            return View("PrintReportForEmployee", (object)Encoding.UTF8.GetString(report.FileBody));
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var token = User.Identity.GetJwtToken();
            ((CompanyService)companyService)?.AddAuthorization(token);
            ((ReportService)reportServiceClient)?.AddAuthorization(token);
            ((UsersService)usersService)?.AddAuthorization(token);
        }
    }
}