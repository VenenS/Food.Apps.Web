using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Food.Services.Models;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using Microsoft.Extensions.Configuration;

namespace ITWebNet.Food.Site.Services
{
    public class CompanyOrderService : BaseClient<CompanyOrderService>, ICompanyOrderService
    {
        private static readonly string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static readonly string BaseUrl = ServiceUrl + "api/companyorder";

        public CompanyOrderService() : base(ServiceUrl)
        {
        }

        public CompanyOrderService(string baseAddress) : base(baseAddress)
        {
        }

        public CompanyOrderService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public CompanyOrderService(bool debug) : base(debug)
        {
        }

        public virtual async Task<long> AddNewCompanyOrder(CompanyOrderModel companyOrder)
        {
            var response = await PutAsync<long>($"{BaseUrl}", companyOrder);
            return response.Content;
        }

        public virtual async Task<long> AddNewCompanyOrderSchedule(CompanyOrderScheduleModel companyOrderSchedule)
        {
            var response = await PutAsync<long>($"{BaseUrl}/schedule", companyOrderSchedule);
            return response.Content;
        }

        public virtual async Task<long> EditCompanyOrder(CompanyOrderModel companyOrder)
        {
            var response = await PostAsync<long>($"{BaseUrl}", companyOrder);
            return response.Content;
        }

        public virtual async Task<long> EditCompanyOrderSchedule(CompanyOrderScheduleModel companyOrderSchedule)
        {
            var response = await PostAsync<long>($"{BaseUrl}/schedule", companyOrderSchedule);
            return response.Content;
        }

        public virtual async Task<List<CompanyOrderModel>> GetAvailableCompanyOrders()
        {
            var response = await GetAsync<List<CompanyOrderModel>>($"{BaseUrl}/availableorders");
            return response.Content;
        }

        public virtual async Task<List<CompanyOrderModel>> GetAvailableCompanyOrdersToUser(long? userId, DateTime? date)
        {
            var userPart = userId != null ? $"/user/{userId}" : "";
            var datePart = date != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(date.Value)}" : "";
            var response =
                await GetAsync<List<CompanyOrderModel>>($"{BaseUrl}/availableorderstouser{userPart}{datePart}");
            return response.Content;
        }

        public virtual async Task<bool> CheckUserIsEmployee(long? userId, long cafeId, DateTime? date)
        {
            var userPart = userId != null ? $"/user/{userId}" : "";
            var datePart = date != null ? $"/date/{DateTimeExtensions.ConvertToUnixTimestamp(date.Value)}" : "";
            var response =
                await GetAsync<bool>($"{BaseUrl}/CheckUserIsEmployee/cafe/{cafeId}{userPart}{datePart}");
            return response.Content;
        }

        public virtual async Task<List<OrderModel>> GetAvailableUserOrderFromCompanyOrder(long companyOrderId)
        {
            var response = await GetAsync<List<OrderModel>>($"{BaseUrl}/userorderfromcompanyorder/{companyOrderId}");
            return response.Content;
        }

        public virtual async Task<CompanyOrderModel> GetCompanyOrder(long companyOrderId)
        {
            var response = await GetAsync<CompanyOrderModel>($"{BaseUrl}/companyorder/{companyOrderId}");
            return response.Content;
        }

        /// <summary>
        /// Получить корп. заказ пользователя по выбранной компании
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<ResponseModel> GetCompanyOrderForUserByCompanyId(RequestCartCompanyOrderModel model)
        {
            var response = await PostAsync<ResponseModel>($"{BaseUrl}/GetCompanyOrderForUserByCompanyId", model);
            if (response.Succeeded)
            {
                return response.Content;
            }
            else
            {
                return new ResponseModel() { Status = 1, Message = "Не удалось получить корпоративные заказы для пользователя" };
            }
        }

        public virtual async Task<CompanyOrderScheduleModel> GetCompanyOrderScheduleById(long id)
        {
            var response = await GetAsync<CompanyOrderScheduleModel>($"{BaseUrl}/companyorderschedulebyid/{id}");
            return response.Content;
        }

        public virtual async Task<List<CompanyOrderModel>> GetListOfCompanyOrderByDate(long companyId, DateTime startDate,
            DateTime endDate, long? cafeId)
        {
            var cafePart = cafeId != null ? $"/cafe/{cafeId}" : "";
            var response =
                await GetAsync<List<CompanyOrderModel>>(
                    $"{BaseUrl}/companyordersbydate/{companyId}{cafePart}/from/{DateTimeExtensions.ConvertToUnixTimestamp(startDate)}/to/{DateTimeExtensions.ConvertToUnixTimestamp(endDate)}");
            return response.Content;
        }
        public virtual async Task<List<CompanyOrderModel>> GetCompanyOrderByDate(DateTime date)
        {
            var response =
                await PostAsync<List<CompanyOrderModel>>(
                    $"{BaseUrl}/companyordersbydate/bydate/{DateTimeExtensions.ConvertToUnixTimestamp(date)}",
                    new CompanyOrderModel
                    {
                    });
            return response.Content;
        }
        public virtual async Task<List<CompanyOrderScheduleModel>> GetListOfCompanyOrderSchedule(long companyId,
            DateTime startDate, DateTime endDate, long? cafeId)
        {
            var cafePart = cafeId != null ? $"/cafe/{cafeId}" : "";
            var response = await GetAsync<List<CompanyOrderScheduleModel>>(
                $"{BaseUrl}/companyordersschedule/{companyId}{cafePart}/from/{DateTimeExtensions.ConvertToUnixTimestamp(startDate)}/to/{DateTimeExtensions.ConvertToUnixTimestamp(endDate)}");
            return response.Content;
        }

        public virtual async Task<List<CompanyOrderScheduleModel>> GetListOfCompanyOrderScheduleByCafeId(long cafeId)
        {
            var response =
                await GetAsync<List<CompanyOrderScheduleModel>>($"{BaseUrl}/companyordersschedulebycafe/{cafeId}");
            return response.Content;
        }

        public virtual async Task<List<CompanyOrderScheduleModel>> GetListOfCompanyOrderScheduleByCompanyId(long cafeId)
        {
            var response =
                await GetAsync<List<CompanyOrderScheduleModel>>($"{BaseUrl}/companyordersschedulebycompany/{cafeId}");
            return response.Content;
        }

        public virtual async Task<bool> RemoveCompanyOrder(long companyOrderId)
        {
            var response = await DeleteAsync<bool>($"{BaseUrl}/companyorder/{companyOrderId}");
            return response.Content;
        }

        public virtual async Task<bool> RemoveCompanyOrderSchedule(long companyOrderScheduleId)
        {
            var response = await DeleteAsync<bool>($"{BaseUrl}/companyorderschedule/{companyOrderScheduleId}");
            return response.Content;
        }
    }
}