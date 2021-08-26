using Food.Services.Contracts;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.Food.Site.Areas.Manager.Models;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Services;
using ITWebNet.FoodServiceManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FoodDishModel = ITWebNet.Food.Core.DataContracts.Common.FoodDishModel;

namespace ITWebNet.Food.Site.Areas.Manager.Controllers
{
    public class MenuController : BaseController
    {
        //private Food.Services.Proxies.FoodServiceManagerClient client;
        private IDishService dishServiceClient;
        private IMenuService menuServiceClient;

        public MenuController(IMemoryCache cache, IDishService dishService, IMenuService menuService, CafeService cafeService) : base(cache, cafeService)
        {
            dishServiceClient = dishService;
            menuServiceClient = menuService;
        }

        [ActivatorUtilitiesConstructor]
        public MenuController(IMemoryCache cache, ICafeService cafeService, IDishService dishService, IMenuService menuService) : 
            base(cache, cafeService)
        {
            dishServiceClient = dishService;
            menuServiceClient = menuService;
        }

        public async Task<ActionResult> Index(long cafeId, long? menuId)
        {
            MenuFilters filters = GetDishViewFilter(cafeId);

            MenuModel result = await GetMenu(cafeId, filters, menuId);

            return View(result);
        }

        [HttpPost]
        public async Task<ActionResult> Filter(long cafeId, MenuFilters filters)
        {
            MenuModel result = await GetMenu(cafeId, filters);

            return PartialView("Index", result);
        }

        private async Task<MenuModel> GetMenu(long cafeId, MenuFilters filters, long? menuId = null)
        {

            if (menuId.HasValue)
            {
                return await GetMenuById(cafeId, menuId, filters);
            }
            Dictionary<FoodCategoryModel, List<Core.DataContracts.Common.FoodDishModel>> allDishes;
            Dictionary<FoodCategoryModel, List<Core.DataContracts.Common.FoodDishModel>> schedule;

            DateTime? requestDate = filters.OneDate?.Date.Add(DateTime.Now.TimeOfDay) ?? DateTime.Now;

            allDishes = await menuServiceClient.GetMenuForManager(cafeId, requestDate);
            schedule = await menuServiceClient.GetMenuSchedules(cafeId, requestDate);
            var availableDishes = GetAvailableDishes(allDishes, schedule);

            var result = new MenuModel
            {
                AvailableDishes =
                    availableDishes
                        .Where(c => string.Compare(c.Key.Name, "Удаленные блюда", true) != 0 &&
                                    string.Compare(c.Key.Name, "Блюда удаленных категорий", true) != 0)
                        .ToDictionary(c => c.Key, c => c.Value),
                Schedule = schedule,
                Filter = filters
            };

            if (!string.IsNullOrEmpty(filters.KeyWords))
            {
                result.AvailableDishes =
                    result.AvailableDishes
                        .ToDictionary(k => k.Key,
                            k => k.Value.Where(c => c.Name.ToLower().Contains(filters.KeyWords.ToLower())).ToList())
                        .Where(c => c.Value.Count > 0)
                        .ToDictionary(k => k.Key, k => k.Value);

                result.Schedule = result.Schedule
                    .ToDictionary(k => k.Key,
                        k => k.Value.Where(c => c.Name.ToLower().Contains(filters.KeyWords.ToLower())).ToList())
                    .Where(c => c.Value.Count > 0)
                    .ToDictionary(k => k.Key, k => k.Value);
            }

            result.Patterns = await GetMenuPatterns(cafeId, null);

            SetDishViewFilter(filters, cafeId);
            return result;
        }

        [AjaxOnly]
        [HttpPost]
        public async Task<ActionResult> SaveMenu(
            long cafeId, 
            string patternName,
            bool isBanket,
            List<long> id, 
            List<string> name, 
            List<double> basePrice,
            bool approve = false)
        {
            var filter = GetDishViewFilter(cafeId);

            if (id == null || id.Count <= 0)
            {
                ModelState.AddModelError("", "В текущем меню нет блюд"); 
            }
            
            if (string.IsNullOrEmpty(patternName))
            {
                ModelState.AddModelError("PatternName", "Укажите название шаблона");
            }
            
            if (!string.IsNullOrEmpty(patternName) && patternName.Length > 128)
            {
                ModelState.AddModelError("PatternName", "Максимальное кол-во символов для названия шаблона - 128");
            }

            if (!ModelState.IsValid)
            {
                var menuPatterns = await GetMenuPatterns(cafeId, null);
                return PartialView("_Patterns", menuPatterns);
            }

            var patterns = await _cafeService.GetMenuPatternsByCafeIdAsync(cafeId);
            if (!approve && patterns.Select(c => c.Name.ToLower()).Contains(patternName.ToLower()))
            {
                //вызов предупреждения о замене шаблона с таким же именем
                return Forbid();
            }

            var dishes = new List<CafeMenuPatternDishModel>();
            for (var i = 0; i < id.Count; i++)
            {
                try
                {
                    dishes.Add(new CafeMenuPatternDishModel()
                    {
                        DishId = id[i],
                        Name = name[i],
                        Price = basePrice[i]
                    });
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            var pattern = new CafeMenuPatternModel()
            {
                CafeId = cafeId,
                PatternToDate = filter.OneDate ?? DateTime.Now.Date,
                Name = patternName,
                Dishes = dishes,
                IsBanket = isBanket
            };
            HttpResult result;
            if (!approve)
            {
                result = await _cafeService.AddCafeMenuPatternAsync(pattern);
            }
            else
            {
                result = await _cafeService.UpdateCafeMenuPatternAsync(pattern);
            }

            if (!result.Succeeded)
            {
                var error = string.Join(Environment.NewLine, result.Errors);
                ViewBag.Error = error;
            }
            else
            {
                ViewBag.Message = !approve
                    ? "Шаблон был успешно добавлен"
                    : "Шаблон был успешно обновлен";
            }

            ModelState.Clear();
            var model = await GetMenuPatterns(cafeId, null);

            return PartialView("_Patterns", model);
        }

        private async Task<MenuPatternModel> GetMenuPatterns(long cafeId, long? id)
        {
            var model = new MenuPatternModel
            {
                PatternId = id ?? 0,
                PatternName = string.Empty,
                Patterns = await _cafeService.GetMenuPatternsByCafeIdAsync(cafeId),
            };
            model.IsBanket = id.HasValue && model.Patterns.Any(c => c.IsBanket && c.Id == id);
            if (model.IsPatternSelected)
            {
                model.PatternName = model.Patterns.First(c => c.Id == model.PatternId).Name;
            }

            return model;
        }

        [AjaxOnly]
        public async Task<ActionResult> GetMenuByPatternId(long cafeId, long? id)
        {
            var filter = GetDishViewFilter(cafeId);
            var model = new MenuModel();
            if (!id.HasValue)
            {
                model = await GetMenu(cafeId, filter);
                return PartialView("_PatternsAndSchedule", Tuple.Create(model.Patterns, model.Schedule));
            }

            model = await GetMenuById(cafeId, id, filter);

            return PartialView("_PatternsAndSchedule", Tuple.Create(model.Patterns, model.Schedule));
        }

        private async Task<MenuModel> GetMenuById(long cafeId, long? id, MenuFilters filter)
        {
            var allDishes = await dishServiceClient.GetFoodCategoryAndFoodDishesByCafeIdAndDate(cafeId,
                        filter.OneDate ?? DateTime.Now);

            var schedule = await _cafeService.GetMenuByPatternIdAsync(cafeId, id.Value);
            var availableDishes = GetAvailableDishes(allDishes, schedule);
            var model = new MenuModel()
            {
                AvailableDishes = availableDishes,
                Filter = filter,
                Patterns = await GetMenuPatterns(cafeId, id),
                Schedule = schedule
            };
            return model;
        }

        [AjaxOnly]
        public async Task<ActionResult> DeleteMenuPattern(long cafeId, MenuPatternModel model)
        {
            var result = await _cafeService.RemoveCafeMenuPatternAsync(cafeId, model.PatternId);
            if (result.IsSuccessStatusCode)
                ViewBag.Message = "Шаблон был удален";
            else
            {
                var error = await result.Content.ReadAsAsync<ErrorMessage>();
                ViewBag.Error = error.Message;
            }

            var patterns = await GetMenuPatterns(cafeId, null);

            return PartialView("_Patterns", patterns);
        }
        
        [HttpPost]
        public async Task<ActionResult> UpdateSchedulesByPatterId(long cafeId, MenuPatternModel model)
        {
            var filter = GetDishViewFilter(cafeId);
            await menuServiceClient.UpdateSchedulesByPatternId(cafeId, model.PatternId, filter.OneDate);
            return RedirectToRoute("ManagerCafe", new { controller = "Menu", action ="Index", cafeId });
        }

        public async Task<ActionResult> FilterSchedules(long cafeId, string searchString)
        {
            var model = await GetScheduleDetails(cafeId, searchString);

            return PartialView("_ScheduleList", model);
        }

        public async Task<ActionResult> ScheduleDetails(long cafeId)
        {
            var model = await GetScheduleDetails(cafeId);

            return View(model);
        }

        public async Task<List<PageDishInMenuHistoryCategoryModel>> GetScheduleDetails(long cafeId,
            string searchString = "")
        {      
            var allSchedules =
                await
                    menuServiceClient.GetDishesInMenuHistoryForCafe(cafeId, searchString);
            return allSchedules;
        }

        [HttpPost, AjaxOnly]
        public async Task<ActionResult> Save(long cafeId, Models.FoodDishModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_Dish", model);
            ScheduleTypeEnum type;
            if (model.ActionType == ActionType.Delete)
            {
                type = ScheduleTypeEnum.Exclude;
                ViewData["ActionType"] = ActionType.Delete;
            }
            else
                type = ScheduleTypeEnum.Simply;
            if (!model.IsPreview)
            {
                var date = model.ToDate.Equals(DateTime.MinValue)
                    ? DateTime.Now
                    : model.ToDate;

                if (!ModelState.IsValid)
                {
                    return PartialView("_Dish", model);
                }

                await menuServiceClient.AddSchedule(
                    model.Id,
                    type,
                    null,
                    null,
                    date,
                    null,
                    null,
                    model.BasePrice);

                ModelState.Clear();
            }

            return PartialView("_Dish", model);
        }

        protected override void Dispose(bool disposing)
        {
            //if (disposing && client != null)
            //{
            //    client.Close();
            //    client = null;
            //}

            base.Dispose(disposing);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string token = User.Identity.GetJwtToken();

            ((DishService)dishServiceClient)?.AddAuthorization(token);
            ((MenuService)menuServiceClient)?.AddAuthorization(token);
        }

        private Dictionary<FoodCategoryModel, List<FoodDishModel>> GetAvailableDishes(
            Dictionary<FoodCategoryModel, List<FoodDishModel>> allDishes,
            Dictionary<FoodCategoryModel, List<FoodDishModel>> schedule)
        {
            var avaliableDishes = new Dictionary<FoodCategoryModel, List<FoodDishModel>>();

            foreach (var item in allDishes)
            {
                if (item.Key.Name.Trim().ToLower() != "удаленные блюда" && item.Key.Name.Trim().ToLower() != "блюда удаленных категорий")
                {
                    var scheduleItem = schedule.FirstOrDefault(s => s.Key.Id == item.Key.Id);

                    if (scheduleItem.Value != null)
                    {
                        var existedLookup = scheduleItem.Value.ToLookup(d => d.Id);
                        var notAddedDishes = item.Value.Where(dish => (!existedLookup.Contains(dish.Id))).ToList();
                        if (notAddedDishes.Count > 0)
                            avaliableDishes.Add(item.Key, notAddedDishes);
                    }
                    else
                    {
                        avaliableDishes.Add(item.Key, item.Value);
                    }
                }
            }

            return avaliableDishes;
        }

        private MenuFilters GetDishViewFilter(long cafeId)
        {
            MenuFilters filter = HttpContext.Session.Get<MenuFilters>("dish-filter-" + cafeId);
            if (filter == null)
                filter = new MenuFilters()
                {
                    OneDate = DateTime.Now
                };

            return filter;
        }

        private void SetDishViewFilter(MenuFilters filter, long cafeId)
        {
            HttpContext.Session.Set("dish-filter-" + cafeId, filter);
        }
    }
}