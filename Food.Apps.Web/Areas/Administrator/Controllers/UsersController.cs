using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using ITWebNet.Food.Core.DataContracts.Admin;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.Food.Site.Areas.Administrator.Models;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace ITWebNet.Food.Site.Areas.Administrator.Controllers
{
    public class UsersController : BaseController
    {
        public UsersController(
            IUsersService userServiceClient, 
            ICompanyService companyService, 
            IDiscountService discountService, 
            ICafeService cafeServiceClient)
        {
            UserServiceClient = userServiceClient;
            CompanyService = companyService;
            DiscountService = discountService;
            CafeServiceClient = cafeServiceClient;
        }

        private IUsersService UserServiceClient { get; set; }
        private ICompanyService CompanyService { get; set; }
        private IDiscountService DiscountService { get; set; }
        private ICafeService CafeServiceClient { get; set; }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetUsers()
        {
            var data = await UserServiceClient.GetFullListOfUsers();
            return Json(data);
        }

        [AjaxOnly]
        [HttpPost]
        public async Task<ActionResult> Delete(UserWithLoginModel user)
        {
            user.IsDeleted = true;
            var data = await UserServiceClient.EditUser(user);
            return NoContent();
        }

        [AjaxOnly]
        [HttpPost]
        public async Task<ActionResult> Edit(UserWithLoginModel user)
        {
            EditUserViewModel model = new EditUserViewModel();
            model.User = user;
            var companies = await GetUserCompanies(user.Id);
            var discounts = await DiscountService.GetUserDiscounts(user.Id);
            var dvm = discounts.Select(d => new UserDiscountViewModel(d)).ToList();
            model.UserCompany = companies.FirstOrDefault() ?? new UserCompaniesViewModel
            {
                UserId = user.Id,
                AvailableCompanies = await CompanyService.GetCompanys()
            };
            model.UserDiscounts = dvm;
            return PartialView("_UserDetails", model);
        }

        [ValidateAntiForgeryToken]
        [AjaxOnly]
        [HttpPost]
        public async Task<ActionResult> EditUser(UserWithLoginModel user)
        {
            if (!await UserServiceClient.CheckUniqueEmail(user.EmailAdress, user.Id))
                return Content("$('#User_EmailAdress').siblings('span').html('Email используется другим пользователем');", "text/javascript");

            //ModelState.AddModelError("User_EmailAdress", "Email используется другим пользователем");

            if (!ModelState.IsValid)
            {
                return Forbid();
            }

            _ = await UserServiceClient.EditUser(user);
            return Json(user);
        }

        [AjaxOnly]
        [HttpPost]
        public async Task<ActionResult> Lockout(UserWithLoginModel user, bool isLock)
        {
            user.Lockout = isLock;
            var result = await UserServiceClient.EditUser(user);
            return NoContent();
        }

        [AjaxOnly]
        [HttpPost]
        public async Task<ActionResult> DeleteUserCompany(long userId, long companyId)
        {
            if (companyId > 0)
                await CompanyService.RemoveUserCompanyLink(companyId, userId);
            return PartialView("_AddCompany", userId);
        }

        [AjaxOnly]
        [HttpPost]
        public async Task<ActionResult> AddUserCompany(long userId)
        {

            var companies = await GetUserCompanies(userId);

            var model = new UserCompaniesViewModel
            {
                Company = new CompanyModel()
                {
                    Id = -1,
                    Name = "-----------",
                },
                UserId = userId,
                AvailableCompanies = await CompanyService.GetCompanys()
            };
            ViewBag.UserId = userId;
            return PartialView("_EditCompany", model);
        }

        [AjaxOnly]
        [HttpPost]
        public async Task<ActionResult> EditUserCompany(long userId)
        {
            var companyId = await CompanyService.GetUserCompanyId(userId);
            var model = new UserCompaniesViewModel();
            if (companyId.HasValue)
                model.Company = await CompanyService.GetCompanyById(companyId.Value);
            ViewBag.UserId = userId;
            model.AvailableCompanies = await CompanyService.GetCompanys();
            return PartialView("_EditCompany", model);
        }

        public async Task<ActionResult> SaveUserCompany(UserCompaniesViewModel model)
        {
            if (model.Company.Id == -1)
            {
                ModelState.AddModelError("Company.Id", "Выберите компанию");
                return PartialView("_EditCompany", model);
            }

            var response = await CompanyService.SetUserCompany(userId: model.UserId, companyId: model.Company.Id);
            if (response) 
                return PartialView("_UserCompany", model);
            return StatusCode(500);
        }

        public async Task<ActionResult> ChangeUserCompany(long userid, long companyid, long oldCompanyId)
        {
            bool result = false;
            var model = new UserCompaniesViewModel();
            var userCompanies = await GetUserCompanies(userid);
            CompanyModel oldCompany = null;
            var newCompany = new CompanyModel();

            var companyExist = userCompanies.FirstOrDefault(c => c.Company.Id == companyid);

            if (companyExist != null)
            {
                companyExist.Company = new CompanyModel()
                {
                    Id = -1,
                    Name = "-----------"
                };
                ModelState.AddModelError("", "Компания уже добавлена");
                return PartialView("_UserCompany", companyExist);
            }

            oldCompany = oldCompanyId > 0
                ? userCompanies.FirstOrDefault(c => c.Company.Id == oldCompanyId).Company
                : null;

            var availableCompanies = userCompanies.FirstOrDefault();
            newCompany = availableCompanies != null
                ? availableCompanies.AvailableCompanies.FirstOrDefault(c => c.Id == companyid)
                : await CompanyService.GetCompanyById(companyid);


            if (oldCompanyId > 0)
            {
                result = await CompanyService.EditUserCompanyLink(companyid, oldCompanyId, userid);
            }
            else
            {
                result = await CompanyService.AddUserCompanyLink(companyid, userid);
            }

            if (result)
            {
                var companiesFromModel = userCompanies.FirstOrDefault();
                var newAvailableCompanies = companiesFromModel != null
                    ? availableCompanies.AvailableCompanies
                    : await CompanyService.GetCompanys();
                newAvailableCompanies.Remove(newCompany);
                if (oldCompanyId > 0)
                    newAvailableCompanies.Add(oldCompany);
                model = new UserCompaniesViewModel
                {
                    Company = newCompany,
                    AvailableCompanies = newAvailableCompanies
                };
            }
            else
            {
                return Forbid();
            }

            return PartialView("_UserCompany", model);
        }

        async Task<List<UserCompaniesViewModel>> GetUserCompanies(long userId)
        {
            List<UserCompaniesViewModel> model = new List<UserCompaniesViewModel>();
            var companies = await CompanyService.GetCompanys();
            var userCompanies = await CompanyService.GetListOfCompanyByUserId(userId);
            var existCompanies = companies.Select(c => c.Id).Except(userCompanies.Select(c => c.Id)).ToList();
            companies = companies.Where(c => existCompanies.Contains(c.Id)).ToList();
            ViewBag.UserId = userId;

            foreach (var item in userCompanies)
            {
                model.Add(new UserCompaniesViewModel()
                {
                    Company = item,
                    AvailableCompanies = companies,
                    UserId = userId
                });
            }
            return model;
        }

        public async Task<ActionResult> AddUserDiscount(long userId)
        {
            var cafes = await CafeServiceClient.GetCafesToUser(userId);

            var model = new UserDiscountViewModel
            {
                DiscountId = -1,
                CafeName = "-----------",
                UserId = userId,
                AvailableCafes = cafes
            };
            return PartialView("_EditDiscount", model);
        }

        public async Task<ActionResult> EditDiscount(long id, long userId)
        {
            var cafes = await CafeServiceClient.GetCafesToUser(userId);
            var discount = await DiscountService.GetDiscounts(new long[] { id });
            var model = new UserDiscountViewModel(discount.FirstOrDefault());
            model.AvailableCafes = cafes;
            return PartialView("_EditDiscount", model);
        }

        public async Task<ActionResult> SaveDiscount(UserDiscountViewModel model)
        {
            if (!ModelState.IsValid || model.CafeId == 0)
            {
                if (model.CafeId == 0)
                {
                    ModelState.AddModelError("", "Укажите кафе");
                }
                var cafes = await CafeServiceClient.GetCafesToUser(model.UserId);
                model.AvailableCafes = cafes;
                return PartialView("_EditDiscount", model);
            }

            var oldDiscount = await DiscountService.GetDiscounts(new long[] { model.DiscountId });

            var result = false;
            if (model.DiscountId > 0 && oldDiscount.Count > 0)
            {
                var newDiscount = oldDiscount.FirstOrDefault();
                newDiscount.Value = model.DiscountValue;
                newDiscount.CafeId = model.CafeId;
                result = await DiscountService.EditDiscount(newDiscount);
            }
            else
            {
                var newDiscount = new DiscountModel()
                {
                    Value = model.DiscountValue,
                    CafeId = model.CafeId,
                    UserId = model.UserId,
                    BeginDate = DateTime.Now
                };

                var newDiscountId = await DiscountService.AddDiscount(newDiscount);
                if (newDiscountId != -1)
                {
                    result = true;
                    model.DiscountId = newDiscountId;
                }
            }

            if (!result)
            {
                var cafes = await CafeServiceClient.GetCafesToUser(model.UserId);
                model.AvailableCafes = cafes;
                ModelState.AddModelError("", "Скидка для данного кафе уже существует");
                return PartialView("_EditDiscount", model);
            }
            
            return PartialView("_UserDiscount", model);
        }

        [AjaxOnly]
        [HttpPost]
        public async Task<ActionResult> RemoveDiscount(long discountId)
        {
            if (discountId > 0)
                await DiscountService.RemoveDiscount(discountId);

            return NoContent();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            string token = User.Identity.GetJwtToken();

            ((UsersService)UserServiceClient)?.AddAuthorization(token);
            ((CompanyService)CompanyService)?.AddAuthorization(token);
            ((DiscountService)DiscountService)?.AddAuthorization(token);
            ((CafeService)CafeServiceClient)?.AddAuthorization(token);
        }
    }
}