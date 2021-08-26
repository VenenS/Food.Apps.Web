using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Areas.Administrator.Models;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ITWebNet.Food.Site.Areas.Administrator.Controllers
{
    public class CompaniesController : BaseController
    {
        public CompaniesController(ICompanyService companyService, IAddressesService addressesService, IUsersService usersService, ICityService cityService)
        {
            _companyService = companyService;
            _addressesService = addressesService;
            _usersService = usersService;
            _cityService = cityService;
        }

        private ICompanyService _companyService { get; set; }
        private IAddressesService _addressesService { get; set; }
        private IUsersService _usersService { get; set; }
        private ICityService _cityService { get; set; }

        #region Работа с компанией

        public async Task<ActionResult> Index()
        {
            CompanysLists model = new CompanysLists();
            var allCompanies = await _companyService.GetAllCompanys();
            model.InactiveCompanys = allCompanies.Where(c => c.IsActive == false && c.IsDeleted).ToList();
            model.ActiveCompanys = allCompanies.Where(c => c.IsActive && !c.IsDeleted).ToList();
            return View(model);
        }

        public async Task<ActionResult> Details(long id)
        {
            var company = await _companyService.GetCompanyById(id);

            return View(company);
        }

        public async Task<ActionResult> Create()
        {
            var response = await _addressesService.GetAddresses();
            var model = new CompanyModel();
            var cities = await _cityService.GetCities();
            ViewBag.Cities = cities.Content;
            ViewBag.Addresses = response.Succeeded 
                ? response.Content 
                : new List<DeliveryAddressModel>();
            return View(model);
        }

        // POST: Administrator/Companies/Create
        [HttpPost]
        public async Task<ActionResult> Create(CompanyModel company, List<long> address)
        {
            company.Name = company.Name.Trim();
            if (company.Name == string.Empty)
                ModelState.AddModelError("Name", "Введите название компании");

            if (!ModelState.IsValid)
                return View(company);

            var httpResult = await _companyService.CreateCompany(company);
            if (!httpResult.Succeeded)
            {
                SetModelErrors(httpResult);
                return View(company);
            }


            return RedirectToAction("Index");
        }
        
        public async Task<ActionResult> Curators(long companyId)
        {
            var company = await _companyService.GetCompanyById(companyId);
            var curators = await _companyService.GetCompanyCurators(companyId);
            var model = new CompanyCuratorViewModel()
            {
                Company = company,
                Curators = curators
            };
            return View(model);
        }

        // GET: Administrator/Companies/Edit/5
        public async Task<ActionResult> Edit(long id)
        {
            var company = await _companyService.GetCompanyById(id);
            if (company == null)
                return NotFound();
            var response = await _addressesService.GetAddresses();
            ViewBag.Addresses = response.Succeeded 
                ? response.Content 
                : new List<DeliveryAddressModel>();
            var cities = await _cityService.GetCities();
            ViewBag.Cities = cities.Content;
            return View(company);
        }

        // POST: Administrator/Companies/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(CompanyModel company)
        {
            if (ModelState.IsValid)
            {
                await _companyService.UpdateCompany(company);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Delete(long id)
        {
            var company = await _companyService.GetCompanyById(id);

            return View(company);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(CompanyModel company)
        {
            try
            {
                await _companyService.DeleteCompany(company.Id);

                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> RestoreCompany(long id)
        {
            try
            {
                await _companyService.RestoreCompany(id);

                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Адреса компании

        [HttpGet]
        public async Task<ActionResult> CreateAddress(long id)
        {            
            var address = new DeliveryAddressModel();
            ViewBag.CompanyId = id;
            return View(address);
        }
        
        [HttpPost]
        public async Task<ActionResult> CreateAddress(long id, DeliveryAddressModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var result = await _companyService.AddAddressToCompany(id, model);
            if (result.Succeeded)
            {
                TempData["Result"] = "Адрес создан";
                return RedirectToAction(nameof(Edit), new RouteValueDictionary(new {id = id}));
            }
            ModelState.AddModelError("", string.Join(Environment.NewLine, result.Errors));
            return View(model);
        }
        
        [HttpGet]
        public async Task<ActionResult> EditAddress(long id, long companyId)
        {
            var response = await _companyService.GetAddressCompanyById(id);
            ViewBag.CompanyId = companyId;
            if (!response.Succeeded)
                return NotFound();

            return View("EditAddress", response.Content);
        }
        
        [HttpPost]
        public async Task<ActionResult> EditAddress(DeliveryAddressModel model, long companyId)
        {
            if (!ModelState.IsValid)
                return View(model);
            
            var response = await _companyService.UpdateAddressCompany(model);
            if (response.Succeeded)
            {
                TempData["Result"] = "Адрес отредактирован";
            }
            else
            {
                ModelState.AddModelError("", string.Join(Environment.NewLine, response.Errors));
                TempData["ModelState"] = ModelState;
            }
            return RedirectToAction(nameof(EditAddress),
                new RouteValueDictionary(new {companyId, id = model.CompanyAddressId, Area = "AdministratorDefault" }));
        }
        
        [HttpGet]
        public async Task<ActionResult> DeleteAddress(long id, long companyId)
        {
            var response = await _companyService.GetAddressCompanyById(id);
            ViewBag.CompanyId = companyId;
            if (!response.Succeeded)
                return StatusCode((int)response.StatusCode);
            return View(response.Content);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAddress(DeliveryAddressModel model, long companyId)
        {
            await _companyService.DeleteAddressFromCompany(model.CompanyAddressId);
            return RedirectToAction(nameof(Edit), new { id = companyId });
        }

        #endregion
        
        public async Task<ActionResult> AddCurator(long companyId)
        {
            var model = new CompanyCuratorModel()
            {
                CompanyId = companyId
            };

            var users = await _usersService.GetUsersWithoutCurators();
            ViewBag.Users = users;
            if (TempData.ContainsKey("Modelstate"))
            {
                ModelState.Merge(TempData["ModelState"] as ModelStateDictionary);
            }
            return View("AddCompanyCurator", model);
        }
        
        [HttpPost]
        public async Task<ActionResult> AddCurator(CompanyCuratorModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ModelState"] = ModelState;
                return RedirectToAction(nameof(AddCurator),
                    new RouteValueDictionary(new { companyId = model.CompanyId, Area = "AdministratorDefault" }));
            }
            var response = await _companyService.AddCompanyCurator(model);
            if (response.Succeeded)
            {
                TempData["Result"] = $"Пользователь успешно добавлен";
                return RedirectToAction(nameof(AddCurator),
                    new RouteValueDictionary(new { companyId = model.CompanyId, Area = "AdministratorDefault" }));
            }

            TempData["Message"] = string.Join(Environment.NewLine, response.Errors);
            return RedirectToAction(nameof(AddCurator),
                    new RouteValueDictionary(new { companyId = model.CompanyId, Area = "AdministratorDefault" }));
        }
        
        [HttpGet]
        public async Task<ActionResult> DeleteCurator(long companyId, long id)
        {
            var curator = await _companyService.GetCompanyCurator(companyId, id);
            if (!curator.Succeeded) 
                return NotFound();
            var company = await _companyService.GetCompanyById(companyId);
            ViewBag.Company = company;
            return View("DeleteCompanyCurator", curator.Content);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteCurator(CompanyCuratorModel model)
        {
            await _companyService.DeleteCompanyCurator(model.CompanyId, model.UserId);
            return RedirectToRoute("AdministratorDefault",
                new {controller = "Companies", action = "Curators", model.CompanyId});
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            string token = User.Identity.GetJwtToken();

            ((CompanyService)_companyService)?.AddAuthorization(token);
            ((AddressesService)_addressesService)?.AddAuthorization(token);
            ((UsersService)_usersService)?.AddAuthorization(token);
        }
    }
}
