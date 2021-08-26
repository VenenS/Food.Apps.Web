using Food.Services.Models;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface ICompanyOrderService
    {
        Task<long> AddNewCompanyOrder(CompanyOrderModel companyOrder);
        Task<long> AddNewCompanyOrderSchedule(CompanyOrderScheduleModel companyOrderSchedule);
        Task<bool> CheckUserIsEmployee(long? userId, long cafeId, DateTime? date);
        Task<long> EditCompanyOrder(CompanyOrderModel companyOrder);
        Task<long> EditCompanyOrderSchedule(CompanyOrderScheduleModel companyOrderSchedule);
        Task<List<CompanyOrderModel>> GetAvailableCompanyOrders();
        Task<List<CompanyOrderModel>> GetAvailableCompanyOrdersToUser(long? userId, DateTime? date);
        Task<List<OrderModel>> GetAvailableUserOrderFromCompanyOrder(long companyOrderId);
        Task<CompanyOrderModel> GetCompanyOrder(long companyOrderId);
        Task<List<CompanyOrderModel>> GetCompanyOrderByDate(DateTime date);
        Task<ResponseModel> GetCompanyOrderForUserByCompanyId(RequestCartCompanyOrderModel model);
        Task<CompanyOrderScheduleModel> GetCompanyOrderScheduleById(long id);
        Task<List<CompanyOrderModel>> GetListOfCompanyOrderByDate(long companyId, DateTime startDate, DateTime endDate, long? cafeId);
        Task<List<CompanyOrderScheduleModel>> GetListOfCompanyOrderSchedule(long companyId, DateTime startDate, DateTime endDate, long? cafeId);
        Task<List<CompanyOrderScheduleModel>> GetListOfCompanyOrderScheduleByCafeId(long cafeId);
        Task<List<CompanyOrderScheduleModel>> GetListOfCompanyOrderScheduleByCompanyId(long cafeId);
        Task<bool> RemoveCompanyOrder(long companyOrderId);
        Task<bool> RemoveCompanyOrderSchedule(long companyOrderScheduleId);
    }
}