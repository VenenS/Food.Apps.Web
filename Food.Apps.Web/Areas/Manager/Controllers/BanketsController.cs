using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Food.Services.Contracts.Companies;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NLog.LayoutRenderers.Wrappers;

namespace ITWebNet.Food.Site.Areas.Manager.Controllers
{
    public class BanketsController : BaseController
    {
        private IBanketsService _banketsService;
        private ICompanyService _companyService;

        public BanketsController(IMemoryCache cache, CafeService cafeService) : base(cache, cafeService)
        {
        }

        [ActivatorUtilitiesConstructor]
        public BanketsController(IMemoryCache cache, ICafeService cafeService, IBanketsService banketsService, ICompanyService companyService) : 
            base(cache, cafeService)
        {
            _banketsService = banketsService;
            _companyService = companyService;
        }

        public async Task<ActionResult> Index(long cafeId)
        {
            var bankets = await _cafeService.GetBanketsByCafeId(cafeId);
            var companies = await _companyService.GetCompaniesByFilter(new CompaniesFilterModel
            {
                CompanyId = bankets.Select(c => c.CompanyId).ToArray()
            });
            ViewBag.Companies = companies.Content.ToList();
            var menus = await _cafeService.GetMenuPatternsByCafeIdAsync(cafeId);
            ViewBag.Menus = menus.Where(c => c.IsBanket).ToList();
            return View(bankets.OrderBy(c => c.Id).ToList());
        }

        [HttpGet]
        public async Task<ActionResult> Create(long cafeId)
        {
            await FillMenuAndCompaniesInModel(cafeId);
            var model = new BanketModel
            {
                EventDate = DateTime.Now.AddDays(1),
                OrderStartDate = DateTime.Now,
                OrderEndDate = DateTime.Now.AddDays(1)
            };
            if (TempData.ContainsKey("ModelState"))
                ModelState.Merge(TempData["ModelState"] as ModelStateDictionary);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Create(long cafeId, BanketModel model)
        {

            ValidateDatesInModel(model);

            if (!ModelState.IsValid)
            {
                await FillMenuAndCompaniesInModel(cafeId);
                return View(model);
            }

            model.CafeId = cafeId;
            model.Name = "";
            model.Url = "";
            var result = await _banketsService.CreateBanket(model);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", string.Join(Environment.NewLine, result.Errors));
                await FillMenuAndCompaniesInModel(cafeId);
                return View(model);
            }

            TempData["Result"] = "Бакнкет был успешно добавлен";

            return RedirectToRoute("ManagerCafe", new {controller = "Bankets", action = "Index"});
        }


        [HttpGet]
        public async Task<ActionResult> Edit(long cafeId, long id)
        {
            var banket = await _banketsService.GetBanketById(id);
            if (!banket.Succeeded)
                return StatusCode((int)banket.StatusCode);
            await FillMenuAndCompaniesInModel(cafeId);
            if (TempData.ContainsKey("ModelState"))
                ModelState.Merge(TempData["ModelState"] as ModelStateDictionary);

            return View(banket.Content);
        }


        [HttpPost]
        public async Task<ActionResult> Edit(long cafeId, BanketModel model)
        {
            ValidateDatesInModel(model);

            model.CafeId = cafeId;
            if (!ModelState.IsValid)
            {
                await FillMenuAndCompaniesInModel(cafeId);
                return View("Edit", model);
            }
            var result = await _banketsService.UpdateBanket(model);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", string.Join(Environment.NewLine, result.Errors));
                await FillMenuAndCompaniesInModel(cafeId);
                return View("Edit", model);
            }

            TempData["Result"] = "Банкет был успешно обновлен";

            return RedirectToRoute("ManagerCafe", new {controller = "Bankets", action = "Index"});
        }

        [HttpGet]
        public async Task<ActionResult> Delete(long cafeId, long id)
        {
            var banket = await _banketsService.GetBanketById(id);
            if (!banket.Succeeded)
                return StatusCode((int)banket.StatusCode);
            return View(banket.Content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(long cafeId, BanketModel model)
        {
            var result = await _banketsService.DeleteBanket(model.Id);
            if (result.IsSuccessStatusCode)
                return RedirectToRoute("ManagerCafe", new {controller = "Bankets", action = "Index"});

            TempData["Result"] = result.ReasonPhrase;
            return View(model);
        }

        private async Task FillMenuAndCompaniesInModel(long cafeId)
        {
            var menus = await _cafeService.GetMenuPatternsByCafeIdAsync(cafeId);
            ViewBag.Menus = menus.Where(c => c.IsBanket);
            var companies = await _companyService.GetCompanys();
            ViewBag.Companies = companies;
        }

        private void ValidateDatesInModel(BanketModel model)
        {
            if (model.OrderStartDate.Date < DateTime.Now.Date || model.OrderEndDate < DateTime.Now.Date)
            {
                ModelState.AddModelError("", "Даты периода заказа должны быть больше текущей даты");
            }

            if (model.OrderStartDate > model.OrderEndDate)
                ModelState.AddModelError("", "Дата окончания заказа должна быть больше даты начала");
            if (model.OrderStartDate > model.EventDate || model.OrderEndDate > model.EventDate)
                ModelState.AddModelError("", "Даты заказа должны быть меньше даты проведения банкета");
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var token = User.Identity.GetJwtToken();
            ((BanketsService)_banketsService)?.AddAuthorization(token);
            ((CompanyService)_companyService)?.AddAuthorization(token);
        }
    }
}