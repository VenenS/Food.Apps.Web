using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace ITWebNet.Food.Site.Areas.Administrator.Controllers
{
    public class CategoriesController : BaseController
    {
        private ICategoriesService _categoriesService;

        [ActivatorUtilitiesConstructor]
        public CategoriesController(ICategoriesService categoriesService)
        {
            _categoriesService = categoriesService;
        }

        public CategoriesController()
        {
            
        }
        
        // GET
        public async Task<ActionResult> Index()
        {
            var categories = await _categoriesService.GetCategories();
            ViewBag.ResultMessage = TempData["ResultMessage"];
            ViewBag.ResultMessageType = TempData["ResultMessageType"];
            return View(categories.Content);
        }
        
        [HttpGet]
        public ActionResult Add()
        {
            FoodCategoryModel category = new FoodCategoryModel();
            category.Id = -category.GetHashCode();

            return PartialView("_Category", category);
        }
        
        [HttpPost]
        public async Task<ActionResult> EditCategory(FoodCategoryModel category)
        {
            if (string.IsNullOrWhiteSpace(category.Name)) return PartialView("_Category", category);

            category.Name = category.Name.Trim();

            var categories = await _categoriesService.GetCategories();
            bool existsName = false;
            if (categories.Succeeded)
            {
                if (category.Id > 0)
                    existsName = categories.Content.Exists(o => o.Name == category.Name && o.Id != category.Id);
                else
                    existsName = categories.Content.Exists(o => o.Name == category.Name);

                if (existsName)
                    ModelState.AddModelError("Name", "Категория с таким названием уже существует");
            }

            if (!ModelState.IsValid)
            {
                return PartialView("_Category", category);
            }

            category.Description = category.Description?.Trim();
            category.FullName = category.FullName?.Trim();

            ModelState.Clear();

            FoodCategoryModel model;
            HttpResult<FoodCategoryModel> response;
            if (category.Id <= 0)
            {
                response = await _categoriesService.CreateCategory(category);
                TempData["ResultMessage"] = response.Succeeded ? "Категория создана." : "Произошла ошибка. Категория не создана.";
            }
            else
            {
                response = await _categoriesService.UpdateCategory(category);
                TempData["ResultMessage"] = response.Succeeded ? "Категория отредактирована." : "Произошла ошибка. Категория не отредактирована.";
            }

            TempData["ResultMessageType"] = response.Succeeded ? "success" : "danger";

            if (response.Succeeded && response.Content != null)
            {
                model = response.Content;
            }
            else
            {
                model = category;
            }

            TempData.Keep();
            return Json(new { result = "Redirect", url = Url.Action("Index", "Categories") });
        }

        public async Task<ActionResult> RemoveCategory(long categoryId)
        {
            bool response = false;
            if (categoryId > 0)
                response = await _categoriesService.DeleteCategory(categoryId);
            TempData["ResultMessage"] = response ? "Категория удалена." : "Произошла ошибка. Категория не удалена.";
            TempData["ResultMessageType"] = response ? "success" : "danger";

            return RedirectToAction(nameof(Index));
        }
    }
}