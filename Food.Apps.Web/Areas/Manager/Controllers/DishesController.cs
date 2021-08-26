using FoodApps.Web.NetCore.Extensions;
using ITWebNet.Food.Site.Areas.Manager.Models;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.Client;
using ITWebNet.FoodServiceManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Areas.Manager.Controllers
{
    public class DishesController : BaseController
    {
        private readonly IDishCategoryService _dishCategoryService;
        private readonly IDishService _dishServiceClient;
        private readonly IMenuService _menuServiceClient;
        private readonly IContentServiceClient _contentService;

        public DishesController(
            IMenuService menuService,
            IDishService dishService,
            IDishCategoryService dishCategoryService,
            IContentServiceClient contentService,
            IMemoryCache cache,
            CafeService cafeService) : base(cache, cafeService)
        {
            _dishCategoryService = dishCategoryService;
            _dishServiceClient = dishService;
            _menuServiceClient = menuService;
            _contentService = contentService;
        }

        [ActivatorUtilitiesConstructor]
        public DishesController(
            IMenuService menuService,
            IDishService dishService,
            IDishCategoryService dishCategoryService,
            IContentServiceClient contentService,
            IMemoryCache cache,
            ICafeService cafeService) : base(cache, cafeService)
        {
            _dishCategoryService = dishCategoryService;
            _dishServiceClient = dishService;
            _menuServiceClient = menuService;
            _contentService = contentService;
        }

        [HttpGet]
        public async Task<ActionResult> Add(long? categoryId = null, long? cafeId = null)
        {
            Core.DataContracts.Common.FoodDishModel origin = new Core.DataContracts.Common.FoodDishModel()
            {
                FoodCategories = await _dishCategoryService.GetSelectableCategoriesByCafeId(cafeId.Value),
                Image = string.Empty
            };
            if (categoryId != null)
            {
                origin.FoodCategories.FirstOrDefault(c => c.Id == categoryId).IsSelected = true;
            }
            DishModel model = new DishModel(origin);
            ViewBag.Title = "Добавление блюда";
            return View("Edit", model);
        }

        public async Task<ActionResult> Dish(long id)
        {
            var dish = await _dishServiceClient.GetFoodDishById(id);

            return View(new DishModel(dish));
        }

        public async Task<ActionResult> Index(long cafeId)
        {
            var model = await GetDishes(cafeId);

            return View(model);
        }

        [AjaxOnly]
        public async Task<ActionResult> Filter(DishFilter filter, long cafeId)
        {
            var model = await GetDishes(cafeId);

            if (!string.IsNullOrEmpty(filter.SearchQuery))
                model = model.FilterDishes(filter);

            model.Filter = filter;

            return PartialView("Index", model);
        }

        public async Task<ActionResult> Delete(long cafeId, long id, long? categoryId = null)
        {
            if (id != 0)
                await _dishServiceClient.DeleteDish(id, categoryId);

            if (Request.IsAjaxRequest())
                return PartialView("Index", await GetDishes(cafeId));

            var message = "Блюдо было успешно удалено";
            return View("Alert", model: message);
        }

        [AjaxOnly]
        public ActionResult RemoveImage(long cafeId, long id)
        {
            return PartialView("_Image", string.Empty);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(long cafeId, long id)
        {
            DishModel model = await GetDishModelAsync(id);
            if (model == null)
                return NotFound("Блюдо не найдено");

            ViewBag.Title = "Редактирование блюда";
            return View("Edit", model);
        }

        [HttpPost]
        public async Task<ActionResult> Save(long cafeId, DishModel model)
        {
            // FIXME: присылается модель данные которой (редактируемые и не редактируемые)
            // скармливаются в методы апи (которые тоже ничего не проверяют).
            if (!string.IsNullOrEmpty(model.Name) && (model.Name.Contains("/") || model.Name.Contains(":")))
            {
                ModelState.AddModelError("Name",
                    "В названии блюда могу присутствовать только буквы, цифры, символы .,-;");
            }
            if (!string.IsNullOrEmpty(model.WeightDescription)
                && (model.WeightDescription.Contains("/")
                || model.WeightDescription.Contains(":")))
            {
                ModelState.AddModelError("WeightDescription",
                    "В описании веса могу присутствовать только буквы, цифры, символы .,;");
            }

            if (model.BasePrice < 0.01 || model.BasePrice > 999999)
            {
                ModelState.AddModelError("BasePrice",
                    "Цена не может быть отрицательным числом, либо нулем, либо меньше 0,01, либо больше 999999");
            }

            if (model.Id == 0)
                model.Id = -1;

            if (model.Id == 0 && model.Name != null)
            {
                var checkUnique = await _dishServiceClient.CheckUniqueNameWithinCafe(model.Name, cafeId, model.Id);
                if (!checkUnique)
                    ModelState.AddModelError("Name", "Блюдо с таким названием уже существует");
            }

            if (ModelState.IsValid && model.ImageFile != null) {
                if (model.ImageFile != null) {
                    using (var stream = model.ImageFile.OpenReadStream()) {
                        var httpResult = await _contentService.PostImage(model.ImageFile.FileName, stream);
                        if (!httpResult.Succeeded) {
                            ModelState.AddModelError(
                                nameof(model.ImageFile),
                                "Контент сервис вернул ошибку: " + httpResult.Message
                            );
                        }

                        model.Image = httpResult.Content;
                    }
                } else if (string.IsNullOrEmpty(model.Image)) {
                    model.Image = null;
                }
            }

            if (!ModelState.IsValid)
            {
                if (model.Id > 0)
                    return await Edit(cafeId, model.Id);
                return await Add(cafeId: cafeId);
            }

            long dishId = 0;

            try
            {
                // Добавление в описание и состав пробелов после знаков пунктуации:
                model.Description = StringUtilities.AddSpacesAfterPunctuations(model.Description);
                model.Composition = StringUtilities.AddSpacesAfterPunctuations(model.Composition);

                string message = string.Empty;
                if (model.Id <= 0)
                {
                    dishId = await _dishServiceClient.CreateDish(cafeId, model.GetOrigin());
                    message = "Блюдо создано";
                }
                else
                {
                    dishId = await _dishServiceClient.UpdateDish(model.GetOrigin());
                    message = "Блюдо было успешно отредактировано";
                }

                if (model != null)
                {
                    ScheduleTypeEnum scheduleType = ScheduleTypeEnum.Exclude;
                    DateTime? scheduleDate = null;

                    #region old
                    //var foodCategory = await _dishCategoryService.GetFoodCategoryById(model.CategoryId);
                    //if (
                    //    String.Compare(foodCategory.Name, "Удаленные блюда", true) == 0
                    //    || String.Compare(foodCategory.Name, "Блюда удаленных категорий", true) == 0
                    //)
                    //    model.IsDaily = false;
                    #endregion

                    // Должно работать как предыдущий код
                    if (!model.FoodCategories.Any(c => c.IsSelected && c.Name != "Удаленные блюда" && c.Name != "Блюда удаленных категорий"))
                    {
                        model.IsDaily = false;
                    }


                    if (model.IsDaily)
                    {
                        scheduleType = ScheduleTypeEnum.Daily;
                        scheduleDate = DateTime.Now.Date;
                    }
                    else if (model.IsSimply)
                    {
                        scheduleType = ScheduleTypeEnum.Simply;
                        scheduleDate = DateTime.Now.Date;
                    }

                    await _menuServiceClient.AddSchedule(
                        dishId,
                        scheduleType,
                        scheduleDate,
                        null,
                        null,
                        null,
                        null,
                        model.BasePrice);
                }
                ModelState.Clear();

                return View("Alert", model: message);
            }
            catch (Exception ex)
            {
                if (model.Id != 0)
                    return RedirectToRoute("ManagerCafe", new { controller = "Dishes", action = "Edit", id = model.Id });

                return RedirectToRoute("ManagerCafe",
                    new { controller = "Dishes", action = "Add", categoryId = model.CategoryId });
            }
        }

        [AjaxOnly, HttpGet]
        public async Task<ActionResult> LoadDishes(long cafeId, long categoryId)
        {
            var dishes = await GetDishModelCollectionAsync(cafeId, categoryId);

            return PartialView("_DishesList", dishes);
        }

        [AjaxOnly, HttpPost]
        public async Task<ActionResult> ChangeCatIndex(long cafeId, long categoryId, int offset)
        {
            await _dishCategoryService.ChangeFoodCategoryOrder(cafeId, categoryId, offset);
            return NoContent();
        }

        [AjaxOnly, HttpPost]
        public async Task<ActionResult> AddCategory(long cafeId, long categoryId, int offset)
        {
            await _dishCategoryService.AddCafeFoodCategory(cafeId, categoryId, offset);
            return PartialView("Index", await GetDishes(cafeId));
        }

        [AjaxOnly, HttpPost]
        public async Task<ActionResult> RemoveCategory(long cafeId, long categoryId)
        {
            await _dishCategoryService.RemoveCafeFoodCategory(cafeId, categoryId);
            return PartialView("Index", await GetDishes(cafeId));
        }

        private async Task<CategoryCollectionModel> GetDishes(long cafeId)
        {
            var dishes = await _menuServiceClient.GetDishesForManager(cafeId, DateTime.Now);
            CategoryCollectionModel model = new CategoryCollectionModel(cafeId);
            foreach (var item in dishes)
            {
                CategoryModel category = new CategoryModel()
                {
                    Id = item.Key.Id,
                    Index = item.Key.Index ?? -1,
                    Name = item.Key.Name,
                    IsActive = item.Value != null,
                    Dishes = DishModel.AsModelList(item.Value ?? new List<Core.DataContracts.Common.FoodDishModel>())
                };
                model.Add(category);
            }

            var result = model.OrderBy(c => c.IsActive).ThenBy(c => c.Index).ToList();
            model.Update(result);

            return model;
        }

        private async Task<DishModel> GetDishModelAsync(long id)
        {
            var dish = await _dishServiceClient.GetFoodDishById(id);
            if (dish == null)
                return null;

            var schedules = await _menuServiceClient.GetScheduleForDish(id, DateTime.Now.Date);
            bool isDaily = schedules.Any(item => item.Type[0] == ScheduleTypeEnum.Daily.ToString()[0]);
            bool isSimply = schedules.Any(item => item.Type[0] == ScheduleTypeEnum.Simply.ToString()[0]);

            DishModel model = new DishModel(dish)
            {
                IsSimply = isSimply,
                IsDaily = isDaily,
                FoodCategories = await _dishCategoryService.GetSelectableCategoriesByDishId(id)
            };

            return model;
        }

        private async Task<List<DishModel>> GetDishModelCollectionAsync(long cafeId, long categoryId)
        {
            var dishes =
                await _dishServiceClient.GetFoodDishesByCategoryIdAndCafeId(cafeId, categoryId, null);
            var model = DishModel.AsModelList(dishes);

            return model;
        }

        [AjaxOnly, HttpPost]
        public async Task<ActionResult> ChangeDishIndex(long cafeId, long oldCategoryId, int newIndex, int oldIndex, int? dishId = null)
        {
            await _dishServiceClient.ChangeDishIndex(cafeId, oldCategoryId, newIndex, oldIndex, dishId);
            return PartialView("Index", await GetDishes(cafeId));
        }

        [AjaxOnly, HttpPost]
        public async Task<ActionResult> UpdateDishIndexInSecondCategory(long cafeId, long categoryId, int newIndex,
            long dishId)
        {
            await _dishServiceClient.UpdateDishIndexInSecondCategory(cafeId, categoryId, newIndex, dishId);
            return PartialView("Index", await GetDishes(cafeId));
        }

        [AjaxOnly, HttpPost]
        public async Task<ActionResult> UpdateDishIndexInFirstCategory(long cafeId, long newCategoryId,
            long oldCategoryId, int newIndex, int oldIndex, long dishId)
        {
            await _dishServiceClient.UpdateDishIndexInFirstCategory(cafeId, newCategoryId, oldCategoryId, newIndex,
                oldIndex, dishId);
            return PartialView("Index", await GetDishes(cafeId));
        }
    }
}