using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.Food.Site.Areas.Administrator.Models;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Services;
using static System.String;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ITWebNet.Food.Site.Areas.Administrator.Controllers
{
    public class CafesController : BaseController
    {
        public CafesController(ICafeService cafeService, IUsersService usersService, ICityService cityService)
        {
            _cafeService = cafeService;
            _usersService = usersService;
            _cityService = cityService;
        }

        private ICafeService _cafeService { get; set; }
        private IUsersService _usersService { get; set; }
        private ICityService _cityService { get; set; }

        public async Task<ActionResult> Index()
        {
            var cafes = await _cafeService.GetAllCafesAsync();
            var cities = await _cityService.GetCities();
            ViewBag.Cities = cities.Content;
            return View(cafes);
        }

        [AjaxOnly]
        [HttpPost]
        public async Task<ActionResult> EditCafe(CafeModel cafe)
        {
            if (!IsNullOrWhiteSpace(cafe.Name) && !await _cafeService.СheckUniqueName(cafe.Name, cafe.Id))
                ModelState.AddModelError("Name", "Уже есть кафе с таким названием");

            if (!ModelState.IsValid)
                return PartialView("_Cafe", cafe);
            var cities = await _cityService.GetCities();
            ViewBag.Cities = cities.Content;
            var isCreating = cafe.Id <= 0;
            var (model, err) = isCreating
                ? await _cafeService.AddNewCafeAsync(cafe)
                : await _cafeService.EditCafeAsync(cafe);

            if (err != null)
            {
                model = await _cafeService.GetCafeById(cafe.Id);

                ModelState.AddModelError("", err);
                return PartialView("_Cafe", model);
            }

            ViewBag.Message = isCreating ? "Кафе успешно создано" : "Изменения успешно сохранены";
            return PartialView("_Cafe", model);
        }

        public async Task<ActionResult> RemoveCafe(long cafeId)
        {
            if (cafeId > 0)
            {
                await _cafeService.RemoveCafeAsync(cafeId);
            }

            return StatusCode(200);
        }

        [AjaxOnly, HttpGet]
        public async Task<ActionResult> Add()
        {
            CafeModel cafe = new CafeModel();
            cafe.Id = -cafe.GetHashCode();

            ViewBag.BackroundColor = "style=background-color:#ffa07a;";
            var cities = await _cityService.GetCities();
            ViewBag.Cities = cities.Content;
            return PartialView("_Cafe", cafe);
        }
        
        [HttpGet]
        public async Task<ActionResult> AddManager(long cafeId)
        {
            var model = new CafeManagerModel()
            {
                CafeId = cafeId
            };
            var users = await _usersService.GetAdminUsers();
            var cafeManagers = await _cafeService.GetCafeManagers(cafeId);
            ViewBag.Users = users.Where(c => !cafeManagers.Select(d => d.UserId).Contains(c.Id)).ToList();
            if (TempData.ContainsKey("Modelstate"))
            {
                ModelState.Merge(TempData["ModelState"] as ModelStateDictionary);
            }
            return View("AddCafeManager", model);
        }
        
        [HttpPost]
        public async Task<ActionResult> AddManager(CafeManagerModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ModelState"] = ModelState;
                return RedirectToRoute("AdministratorDefault",
                    new {action = "AddManager", cafeId = model.CafeId, controller = "Cafes"});
            }
            var response = await _cafeService.AddCafeManager(model);
            if (response.Succeeded)
            {
                return RedirectToRoute("AdministratorDefault",
                    new {action = "Managers", cafeId = model.CafeId, controller = "Cafes"});
            }

            TempData["Message"] = Join(Environment.NewLine, response.Errors);
            return RedirectToRoute("AdministratorDefault",
                new {action = "AddManager", cafeId = model.CafeId, controller = "Cafes"});
        }

        public async Task<ActionResult> Filter(string filter)
        {
            List<CafeModel> allCafes = await _cafeService.GetCafesAsync();
            List<CafeModel> cafes = null;

            if (!IsNullOrWhiteSpace(filter))
                cafes = allCafes.Where(c =>
                        c.Name.ToLower().Contains(filter.Trim().ToLower()) || c.FullName.ToLower().Contains(filter.Trim().ToLower()))
                    .ToList();
            else
                cafes = allCafes;

            return PartialView("_CafesList", cafes);
        }

        public async Task<ActionResult> Managers(long cafeId)
        {
            var cafe = await _cafeService.GetCafeById(cafeId);
            var managers = await _cafeService.GetCafeManagers(cafeId);
            var model = new CafeManagerViewModel()
            {
                Cafe = cafe,
                Managers = managers
            };
            return View(model);
        }
        
        [HttpGet]
        public async Task<ActionResult> Delete(long cafeId, long id)
        {
            var cafeManager = await _cafeService.GetCafeManager(cafeId, id);
            if (!cafeManager.Succeeded) 
                return NotFound();
            
            ViewBag.Cafe = await _cafeService.GetCafeById(cafeId);
            return View("DeleteCafeManager", cafeManager.Content);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(CafeManagerModel model)
        {
            await _cafeService.DeleteCafeManager(model.CafeId, model.UserId);
            return RedirectToRoute("AdministratorDefault",
                new {controller = "Cafes", action = "Managers", model.CafeId});
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var token = User.Identity.GetJwtToken();
            ((CafeService)_cafeService)?.AddAuthorization(token);
            ((UsersService)_usersService)?.AddAuthorization(token);
        }
    }
}