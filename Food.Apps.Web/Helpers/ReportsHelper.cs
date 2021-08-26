using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompanyOrderModel = ITWebNet.Food.Site.Models.CompanyOrderModel;
using OrderModel = ITWebNet.Food.Core.DataContracts.Common.OrderModel;

namespace ITWebNet.Food.Site.Helpers
{
    public class ReportsHelper
    {
        private IBanketsService _banketsService;
        private IOrderService _orderServiceClient;
        public ReportsHelper(IOrderService orderServiceClient, IBanketsService banketsService)
        {
            _orderServiceClient = orderServiceClient;
            _banketsService = banketsService;
        }
        
        public async Task<ReportsModel> GetReportsData(ReportViewFilter filter, long cafeId)
        {
            ReportsModel model = new ReportsModel();

            var isCompaniesSelected = filter.CompanyOrders != null && filter.CompanyOrders.Count > 0;
            var isUsersSelected = filter.UserOrders != null && filter.UserOrders.Count > 0;
            var isBanketSelected = filter.BanketOrders != null && filter.BanketOrders.Count > 0;
            var isAnythingSelected = isCompaniesSelected || isUsersSelected || isBanketSelected;

            var loadCompanyOrders = filter.IsCompany && !isAnythingSelected
                                     || (isAnythingSelected && isCompaniesSelected);

            var loadUserOrders = filter.IsCustomer && !isAnythingSelected
                                  || (isAnythingSelected && isUsersSelected);

            var loadBanketOrders = filter.IsBanket && !isAnythingSelected || isAnythingSelected && isBanketSelected;

            var reportFilter = filter.AsReportFilter(cafeId, null);
            if (loadCompanyOrders)
            {
                var companyOrders = await _orderServiceClient.GetCompanyOrderByFilters(reportFilter);

                model.CompanyOrders =
                    CompanyOrderModel.AsModelList(companyOrders);
            }

            if (loadUserOrders)
            {
                var userOrders = await _orderServiceClient.GetOrderByFilters(reportFilter);

                model.UserOrders =
                    OrderViewModel.AsModelList(userOrders);
            }

            if (loadBanketOrders)
            {
                var banketFilter = new BanketsFilterModel()
                {
                    CafeId = cafeId,
                    EndDate = filter.End,
                    StartDate = filter.Start,
                    BanketIds = filter.BanketOrders,
                    LoadOrders = filter.LoadUserOrders,
                    Search = filter.SearchQuery.SearchString,
                    SearchType = filter.SearchQuery.Type,
                    SortType = (ReportSortType)(int)filter.SortType
                };
                model.BanketOrders = await _banketsService.GetBanketsByFilter(banketFilter);
            }

            // Фильтрация банкетных и корпоративных заказов. Персональные заказы фильтруются SQL-запросом  в момент выборки из БД.
            if (!string.IsNullOrWhiteSpace(filter.SearchQuery.SearchString))
            {
                var workingSearchString = filter.SearchQuery.SearchString.Trim().ToLower();
                switch (filter.SearchQuery.Type)
                {
                    case SearchType.SearchByName:
                        model.CompanyOrders =
                            model.CompanyOrders.Select(d => new
                            {
                                co = d,
                                uo = d.UserOrders.Where(
                                    c => c.Creator != null &&
                                         (
                                             (
                                                 !string.IsNullOrEmpty(c.Creator.UserFullName) &&
                                                 c.Creator.UserFullName.ToLower()
                                                     .Contains(workingSearchString)
                                             )
                                             ||
                                             (
                                                 string.IsNullOrEmpty(c.Creator.UserFullName)
                                                 && !string.IsNullOrEmpty(c.Creator.Email)
                                                 && c.Creator.Email.Trim().ToLower()
                                                     .Contains(workingSearchString)
                                             )
                                         )
                                )
                            }).Where(c => c.uo.Any()).Select(c => new CompanyOrderModel(c.co, c.uo)).ToList();
                        model.BanketOrders =
                            model.BanketOrders.Where(
                                    co =>
                                        co.Orders.Any(
                                            c => c.Creator != null &&
                                                 (
                                                     (
                                                         !string.IsNullOrEmpty(c.Creator.UserFullName) &&
                                                         c.Creator.UserFullName.ToLower()
                                                             .Contains(workingSearchString)
                                                     )
                                                     ||
                                                     (
                                                         string.IsNullOrEmpty(c.Creator.UserFullName)
                                                         && !string.IsNullOrEmpty(c.Creator.Email)
                                                         && c.Creator.Email.Trim().ToLower()
                                                             .Contains(workingSearchString)
                                                     )
                                                 )
                                        )
                                )
                                .ToList();
                        break;
                    case SearchType.SearchByDish:
                        model.CompanyOrders =
                            model.CompanyOrders.Select(d => new
                                {
                                    co = d,
                                    uo = d.UserOrders.Where(
                                        c =>
                                            c.OrderItems.Any(
                                                i =>
                                                    !string.IsNullOrEmpty(i.DishName) &&
                                                    i.DishName.ToLower()
                                                        .Contains(workingSearchString)))
                                }).Select(c => new CompanyOrderModel(c.co, c.uo.ToList()))
                                .Where(c => c.UserOrders.Any())
                                .ToList();
                        model.BanketOrders =
                            model.BanketOrders.Where(
                                co =>
                                    co.Orders.Any(
                                        c =>
                                            c.OrderItems.Any(
                                                i =>
                                                    !string.IsNullOrEmpty(i.DishName) &&
                                                    i.DishName.ToLower()
                                                        .Contains(workingSearchString)))).ToList();
                        break;
                    case SearchType.SearchByPhone:
                        model.CompanyOrders =
                            model.CompanyOrders.Select(d => new
                            {
                                co = d,
                                uo = d.UserOrders.Where(
                                    c =>
                                        (!string.IsNullOrEmpty(c.PhoneNumber) &&
                                         c.PhoneNumber.ToLower()
                                             .Contains(workingSearchString)) ||
                                        (!string.IsNullOrEmpty(c.Creator.PhoneNumber) &&
                                         c.Creator.PhoneNumber.ToLower()
                                             .Contains(workingSearchString)))
                            }).Select(c => new CompanyOrderModel(c.co, c.uo)).Where(c => c.UserOrders.Any()).ToList();
                        model.BanketOrders =
                            model.BanketOrders.Where(
                                    co =>
                                        co.Orders.Any(
                                            c =>
                                                (!string.IsNullOrEmpty(c.PhoneNumber) &&
                                                 c.PhoneNumber.ToLower()
                                                     .Contains(workingSearchString)) ||
                                                (!string.IsNullOrEmpty(c.Creator.PhoneNumber) &&
                                                 c.Creator.PhoneNumber.ToLower()
                                                     .Contains(workingSearchString))))
                                .ToList();
                        break;
                    case SearchType.SearchByOrderNumber:
                        long num;
                        if (long.TryParse(workingSearchString, out num))
                        {
                            var corpOrders = model.CompanyOrders.Where(o => o.Id == num).ToList();
                            List<OrderViewModel> privateOrders = new List<OrderViewModel>();
                            for (var i = 0; i < model.CompanyOrders.Count; i++)
                            {
                                privateOrders =
                                    model.CompanyOrders.ElementAt(i).UserOrders.Where(o => o.Id == num).ToList();
                                model.CompanyOrders.ElementAt(i).UserOrders = privateOrders;
                            }
                            
                            var banketOrders = model.BanketOrders.Where(o => o.Id == num).ToList();
                            var privateBanketOrders = new List<OrderModel>();
                            for (var i = 0; i < model.BanketOrders.Count; i++)
                            {
                                privateBanketOrders =
                                    model.BanketOrders.ElementAt(i).Orders.Where(o => o.Id == num).ToList();
                                model.BanketOrders.ElementAt(i).Orders = privateBanketOrders;
                            }

                            model.BanketOrders =
                                model.BanketOrders.Where(c => c.Orders.Count > 0)
                                    .ToList()
                                    .Union(banketOrders)
                                    .ToList();
                        }
                        else
                        {
                            model = new ReportsModel();
                        }
                        break;
                }
            }

            model.Filter = filter;
            return model;
        }

        public async Task<ReportDetailsModel> GetDetailsForOrder(long cafeId, ReportViewFilter filter, long id,
            EnumOrderType orderType)
        {
            ReportsModel data;

            ReportDetailsModel model = new ReportDetailsModel();

            switch (orderType)
            {
                case EnumOrderType.Collective:
                    filter.CompanyOrders = new List<long> { id };
                    filter.LoadUserOrders = true;
                    filter.IsCompany = true;
                    data = await GetReportsData(filter, cafeId);
                    filter.LoadUserOrders = false;
                    filter.UserOrders = null;
                    filter.BanketOrders = null;
                    filter.CompanyOrders = null;

                    if (data.CompanyOrders.Count == 1)
                    {
                        var order = data.CompanyOrders.First();

                        if (order.OrderAutoCloseDate.HasValue && filter.Start.Equals(DateTime.MinValue))
                            filter.Start = order.OrderAutoCloseDate.Value;

                        model.CompanyOrder = order;
                    }
                    break;

                case EnumOrderType.Individual:
                    filter.UserOrders = new List<long> { id };
                    filter.IsCustomer = true;
                    filter.LoadOrderItems = true;
                    data = await GetReportsData(filter, cafeId);
                    filter.LoadOrderItems = false;
                    filter.CompanyOrders = null;
                    filter.BanketOrders = null;
                    filter.UserOrders = null;

                    if (data.UserOrders.Count == 1)
                    {
                        var order = data.UserOrders.First();

                        if (filter.Start.Equals(DateTime.MinValue))
                            filter.Start = order.Create;

                        model.UserOrder = order;
                        //Расчет суммы сдачи
                        if (model.UserOrder.PayType == PayTypeConst.CourierCash)
                        {
                            double doubChange;
                            var change = double.TryParse(model.UserOrder.OddMoneyComment, out doubChange);
                            if (change)
                            {
                                doubChange -= (double)model.UserOrder.TotalSum;
                                if (doubChange > 0)
                                    model.UserOrder.OddMoneyComment =
                                        model.UserOrder.OddMoneyComment + $" (сдача {doubChange} руб.)";
                                else
                                    model.UserOrder.OddMoneyComment = ($" Отрицательная сумма сдачи");
                            }
                        }
                        if (String.IsNullOrEmpty(model.UserOrder.Comment))
                            model.UserOrder.Comment = "нет";
                        if (model.UserOrder.OddMoneyComment == "terminal")
                            model.UserOrder.OddMoneyComment = "Требуется терминал оплаты";
                        
                    }
                    break;
                 case EnumOrderType.Banket:
                     filter.BanketOrders = new List<long> {id};
                     filter.IsBanket = true;
                     filter.LoadUserOrders = true;
                     data = await GetReportsData(filter, cafeId);
                     filter.LoadUserOrders = false;
                     filter.UserOrders = null;
                     filter.CompanyOrders = null;
                     filter.BanketOrders = null;
                     if (data.BanketOrders.Count == 1)
                     {
                         var order = data.BanketOrders.First();
                         model.BanketOrder = order;
                     }
                     break;
            }


            return model;
        }
    }
}