using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ITWebNet.Food.Core.DataContracts.Admin;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;

namespace ITWebNet.Food.Site.Areas.Administrator.Models
{
    public class EditUserViewModel
    {
        public UserWithLoginModel User { get; set; }
        [Display(Name = "Компания")]
        public UserCompaniesViewModel UserCompany { get; set; }

        public List<UserDiscountViewModel> UserDiscounts { get; set; }

        public EditUserViewModel()
        {
            User = new UserWithLoginModel();
            UserCompany = new UserCompaniesViewModel();
            UserDiscounts = new List<UserDiscountViewModel>();
        }
    }

    public class UserCompaniesViewModel
    {
        public CompanyModel Company { get; set; }
        public List<CompanyModel> AvailableCompanies { get; set; }
        public long UserId { get; set; }

        public UserCompaniesViewModel()
        {
            AvailableCompanies = new List<CompanyModel>();
        }
    }
    
    public class UserDiscountViewModel
    {
        public long DiscountId { get; set; }
        [Range(1, 99, ErrorMessage = "Значение скидки некорректно")]
        public int DiscountValue { get; set; }
        public string CafeName { get; set; }
        public long CafeId { get; set; }
        public long UserId { get; set; }
        public List<CafeModel> AvailableCafes { get; set; }

        public UserDiscountViewModel()
        {
            AvailableCafes = new List<CafeModel>();
        }

        public UserDiscountViewModel(DiscountModel discount)
        {
            DiscountId = discount.Id;
            DiscountValue = discount.Value;
            CafeName = discount.Cafe != null ? discount.Cafe.FullName : "-----------";
            CafeId = discount.CafeId;
            UserId = discount.UserId != null ? discount.UserId.Value : -1;
            AvailableCafes = new List<CafeModel>();
        }
    }
}