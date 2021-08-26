using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.Food.Site.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace ITWebNet.Food.Site.Models
{
    public class ReportsModel
    {
        public List<CompanyOrderModel> CompanyOrders { get; set; }
        public List<OrderViewModel> UserOrders { get; set; }
        public ReportViewFilter Filter { get; set; }
        public List<BanketModel> BanketOrders { get; set; }

        public ReportsModel()
        {
            CompanyOrders = new List<CompanyOrderModel>();
            UserOrders = new List<OrderViewModel>();
            BanketOrders = new List<BanketModel>();
        }
    }

    public class ReportDetailsModel
    {
        public CompanyOrderModel CompanyOrder { get; set; }
        public OrderViewModel UserOrder { get; set; }
        public BanketModel BanketOrder { get; set; }
    }

    public class ReportViewFilter
    {
        [Display(Name = "Действие")]
        public ActionReportType ActionReportType { get; set; }

        private DateTime _start;

        [StartDateLessEndDate("Start", "End",
            ErrorMessage = "Дата начала периода не может быть больше даты окончания")]
        [DataType(DataType.Date)]
        public DateTime Start
        {
            get { return _start; }
            set { _start = value.Date; }
        }

        private DateTime _end;

        [DataType(DataType.Date)]
        public DateTime End
        {
            get { return _end; }
            set { _end = value.Date.AddDays(1).AddSeconds(-1); }
        }

        public ReportViewFilter()
        {
            AvailableStatuses = new List<OrderStatusEnum>()
            {
                OrderStatusEnum.Created,
                OrderStatusEnum.Delivered,
                OrderStatusEnum.Delivery,
                OrderStatusEnum.Accepted,
                OrderStatusEnum.Abort,
            };
        }

        public ReportViewFilter InitDefaults()
        {
            Statuses = new List<OrderStatusEnum>()
            {
                OrderStatusEnum.Delivered,
                OrderStatusEnum.Delivery,
                OrderStatusEnum.Accepted,
                OrderStatusEnum.Created,
                OrderStatusEnum.Abort
            };

            SortType = SortType.OrderByDeliveryDate;

            SearchQuery = new SearchFilter();

            var now = DateTime.Now.Date;

            End = now.AddDays(1).AddSeconds(-1);
            Start = now;

            IsCustomer = true;
            IsCompany = true;
            IsBanket = true;

            return this;
        }

        [Display(Name = "Физическое лицо")]
        public bool IsCustomer { get; set; }
        [Display(Name = "Юридическое лицо")]
        public bool IsCompany { get; set; }

        [Display(Name = "Банкетный заказ")]
        public bool IsBanket { get; set; }

        [Display(Name = "Статусы")]
        public List<OrderStatusEnum> Statuses { get; set; }

        [Display(Name = "Сортировка")]
        public SortType SortType { get; set; }

        [Display(Name = "Упорядочивание")]
        public ResultOrdering ResultOrder { get; set; }

        [Display(Name = "Поиск")]
        public SearchFilter SearchQuery { get; set; }

        public List<long> CompanyOrders { get; set; }
        public List<long> UserOrders { get; set; }
        public List<long> BanketOrders { get; set; }

        public List<OrderStatusEnum> AvailableStatuses { get; private set; }

        public long ReportTypeId { get; set; }

        public List<XSLTModel> ReportTypes { get; set; }

        public CompanyModel Company { get; set; }

        public ReportFilter AsReportFilter(long? cafeId, long? companyId)
        {
            var result = new ReportFilter
            {
                AvailableStatusList = this.Statuses,
                CafeId = cafeId,
                CompanyOrdersIdList = this.CompanyOrders,
                EndDate = this.End,
                OrdersIdList = this.UserOrders,
                BanketOrdersIdList = this.BanketOrders,
                ReportTypeId = this.ReportTypeId,
                StartDate = this.Start,
                CompanyId = companyId,
                LoadUserOrders = LoadUserOrders,
                LoadOrderItems = LoadOrderItems,
                ReportExtension = this.ReportExtension,
                UserId = this.UserId,
                SortType = (ReportSortType)(int)this.SortType,
                ResultOrder = (ReportResultOrder)this.ResultOrder
            };

            if (SearchQuery != null)
            {
                result.SearchType = this.SearchQuery.Type;
                result.Search = this.SearchQuery?.SearchString;
            }

            return result;
        }

        public bool LoadUserOrders { get; set; }
        public bool LoadOrderItems { get; set; }
        public ReportExtension ReportExtension { get; set; }

        public long UserId { get; set; }
        public SelectList Users { get; set; }
    }

    public enum ActionReportType
    {
        [Description("печать")]
        ActionPrint,
        [Description("получить xlsx файл")]
        ActionGetXLSX
    }

    public enum SortType
    {
        [Description("По времени поступления")]
        OrderByDate,
        [Description("По статусам")]
        OrderByStatus,
        [Description("По сумме заказа")]
        OrderByPrice,
        [Description("По номеру заказа")]
        OrderByOrderNumber,
        [Description("по названию кафе")]
        OrderByCafeName,
        [Description("По сотруднику")]
        OrderByEmployeeName,
        [Description("По сотрудникам")]
        OrderByAllEmployee,
        [Description("По времени доставки")]
        OrderByDeliveryDate
    }

    public enum ResultOrdering
    {
        [Description("По возрастанию")]
        Ascending = ReportResultOrder.Ascending,

        [Description("По убыванию")]
        Descending = ReportResultOrder.Descending,
    }

    public class SearchFilter
    {
        public string SearchString { get; set; }
        [Display(Name = "Тип поиска")]
        public SearchType Type { get; set; }

        public  SearchFilter()
        {
            SearchString = string.Empty;
            Type = SearchType.SearchByName;
        }
    }

    public class OrderViewModel
    {
        private readonly Core.DataContracts.Common.OrderModel _origin;

        public long Id
        {
            get { return _origin.Id; }
            set { _origin.Id = value; }
        }

        public long? BanketId
        {
            get { return _origin.BanketId; }
            set { _origin.BanketId = value; }
        }

        public string CreatorLogin
        {
            get { return _origin.CreatorLogin; }
            set { _origin.CreatorLogin = value; }
        }

        public DateTime Create
        {
            get { return _origin.Create; }
            set { _origin.Create = value; }
        }

        public List<OrderItemModel> OrderItems
        {
            get { return _origin.OrderItems; }
            set { _origin.OrderItems = value; }
        }

        public long? DeliveryAddress
        {
            get { return _origin.DeliveryAddressId; }
            set { _origin.DeliveryAddressId = value; }
        }

        public string PhoneNumber
        {
            get { return _origin.PhoneNumber; }
            set { _origin.PhoneNumber = value; }
        }

        public string OddMoneyComment
        {
            get { return _origin.OddMoneyComment; }
            set { _origin.OddMoneyComment = value; }
        }

        public string Comment
        {
            get { return _origin.Comment; }
            set { _origin.Comment = value; }
        }

        public long? CompanyOrderId
        {
            get { return _origin.CompanyOrderId; }
            set { _origin.CompanyOrderId = value; }
        }

        public double? TotalSum
        {
            get { return _origin.TotalSum; }
            set { _origin.TotalSum = value; }
        }

        public DateTime? DeliverDate
        {
            get { return _origin.DeliverDate; }
            set { _origin.DeliverDate = value; }
        }

        public OrderStatusEnum Status
        {
            get { return (OrderStatusEnum)_origin.Status; }
            set { _origin.Status = (long)value; }
        }

        public long CafeId
        {
            get { return _origin.CafeId; }
            set { _origin.CafeId = value; }
        }

        public bool IsDeleted
        {
            get { return _origin.IsDeleted; }
            set { _origin.IsDeleted = value; }
        }

        public UserModel Creator
        {
            get { return _origin.Creator; }
            set { _origin.Creator = value; }
        }

        public OrderInfoModel OrderInfo
        {
            get { return _origin.OrderInfo; }
            set { _origin.OrderInfo = value; }
        }

        public string CafeName
        {
            get { return _origin.Cafe?.Name; }
        }

        public string PayType
        {
            get { return DictionaryOfpaymentTypes.ReturningAValueByKey(_origin.PayType); }
            set { _origin.PayType = value; }
        }

        public string ManagerComment
        {
            get { return _origin.ManagerComment;}
            set { _origin.ManagerComment = value; }
        }

        public List<OrderStatusEnum> AvailableStatuses { get; set; }

        public OrderViewModel(Core.DataContracts.Common.OrderModel origin)
        {
            _origin = origin;
        }

        public OrderViewModel()
        {
                
        }

        //public static explicit operator OrderModel(Order origin)
        //{
        //    return new OrderModel(origin);
        //}

        //public static explicit operator Order(OrderModel model)
        //{
        //    return model._origin;
        //}

        public static List<OrderViewModel> AsModelList(List<Core.DataContracts.Common.OrderModel> source)
        {
            return source.Select(item => new OrderViewModel(item)).ToList();
        }

        public static List<Core.DataContracts.Common.OrderModel> AsSourceList(List<OrderViewModel> model)
        {
            return model.Select(item => item._origin).ToList();
        }


        public List<OrderStatusEnum> GetAvailableOrderStatuses(IIdentity currentUser, bool fromReport = true)
        {
            if (currentUser != null
                && currentUser.GetUserId<long>() == _origin.CreatorId
                && !fromReport
            )
            {
                return new List<OrderStatusEnum>
                {
                    OrderStatusEnum.Abort
                };
            }
            var orderStatuses =
                new List<OrderStatusEnum>();

            if (_origin.Status != null)
            {
                var status = (OrderStatusEnum)_origin.Status;

                switch (status)
                {
                    case OrderStatusEnum.Created:
                        {
                            orderStatuses.Add(OrderStatusEnum.Accepted);
                            orderStatuses.Add(OrderStatusEnum.Abort);
                        }
                        break;
                    case OrderStatusEnum.Accepted:
                        {
                            orderStatuses.Add(OrderStatusEnum.Delivery);
                            orderStatuses.Add(OrderStatusEnum.Abort);
                            orderStatuses.Add(OrderStatusEnum.Delivered);
                        }
                        break;
                    case OrderStatusEnum.Delivery:
                        {
                            orderStatuses.Add(OrderStatusEnum.Abort);
                            orderStatuses.Add(OrderStatusEnum.Delivered);
                        }
                        break;
                    default:
                        break;
                }
            }
            return orderStatuses;
        }
    }

    public class CompanyOrderModel
    {
        private readonly Core.DataContracts.Common.CompanyOrderModel _origin;

        public long Id
        {
            get { return _origin.Id; }
            set { _origin.Id = value; }
        }

        public DateTime? CreateDate
        {
            get { return _origin.CreateDate; }
            set { _origin.CreateDate = value; }
        }

        public long? DeliveryAddressId
        {
            get { return _origin.DeliveryAddressId; }
            set { _origin.DeliveryAddressId = value; }
        }

        public DateTime? DeliveryDate
        {
            get { return _origin.DeliveryDate; }
            set { _origin.DeliveryDate = value; }
        }

        public long CafeId
        {
            get { return _origin.CafeId; }
            set { _origin.CafeId = value; }
        }

        public long CompanyId
        {
            get { return _origin.CompanyId; }
            set { _origin.CompanyId = value; }
        }

        public DateTime? OrderOpenDate
        {
            get { return _origin.OrderOpenDate; }
            set { _origin.OrderOpenDate = value; }
        }

        public DateTime? OrderAutoCloseDate
        {
            get { return _origin.OrderAutoCloseDate; }
            set { _origin.OrderAutoCloseDate = value; }
        }

        public string ContactPhone
        {
            get { return _origin.ContactPhone; }
            set { _origin.ContactPhone = value; }
        }

        public string ContactEmail
        {
            get { return _origin.ContactEmail; }
            set { _origin.ContactEmail = value; }
        }

        public OrderStatusEnum OrderStatus
        {
            get { return (OrderStatusEnum)_origin.OrderStatus; }
            set { _origin.OrderStatus = (long)value; }
        }

        public double TotalPrice
        {
            get { return _origin.TotalPrice; }
            set { _origin.TotalPrice = value; }
        }

        public List<OrderStatusEnum> AvailableStatuses { get; set; }

        private List<OrderViewModel> _userOrders;

        public List<OrderViewModel> UserOrders
        {
            get
            {
                if (_userOrders == null)
                    _userOrders = OrderViewModel.AsModelList(_origin.UserOrders);
                return _userOrders;
            }
            set
            {
                _userOrders = value;
                _origin.UserOrders = OrderViewModel.AsSourceList(value);
            }
        }

        public bool isDeleted
        {
            get { return _origin.IsDeleted; }
            set { _origin.IsDeleted = value; }
        }

        public CompanyModel Company
        {
            get { return _origin.Company; }
            set { _origin.Company = value; }
        }

        public CafeModel Cafe
        {
            get { return _origin.Cafe; }
            set { _origin.Cafe = value; }
        }

        public Dictionary<string, double> DeliveryCostPerAddress
        {
            get { return _origin.DeliveryCostDetails.ToDictionary(x => x.Address, x => x.Cost); }
            set { }
        }

        public CompanyOrderModel(Core.DataContracts.Common.CompanyOrderModel origin)
        {
            _origin = origin;
        }

        //public static explicit operator CompanyOrderModel(CompanyOrder origin)
        //{
        //    return new CompanyOrderModel(origin);
        //}

        public CompanyOrderModel(CompanyOrderModel model, IEnumerable<OrderViewModel> orders)
        {
            _origin = model._origin;
            _userOrders = orders?.ToList() ?? new List<OrderViewModel>();
        }

        public static List<CompanyOrderModel> AsModelList(List<Core.DataContracts.Common.CompanyOrderModel> source)
        {
            return source.Select(item => new CompanyOrderModel(item)).ToList();
        }

        public static List<Core.DataContracts.Common.CompanyOrderModel> AsSourceList(List<CompanyOrderModel> model)
        {
            return model.Select(item => item._origin).ToList();
        }

        public double TotalDeliveryCost
        {
            get { return _origin.TotalDeliveryCost; }
            set { _origin.TotalDeliveryCost = value; }
        }
    }
}