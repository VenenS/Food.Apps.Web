using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.AuthorizationService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace ITWebNet.Food.Site.Controllers
{
    public class CafeController : BaseController
    {
        private readonly int _blockSize;

        /// <summary>
        /// Количество блюд, отображаемых в каждой категории на главной странице до нажатия кнопки "Показать ещё +N"
        /// </summary>
        private readonly int _CountOfDishesMain;
        /// <summary>
        /// Количество блюд, отображаемых в каждой категории при выборе фильтра по категории
        /// </summary>
        private readonly int _CountOfDishesFilter;

        public CafeController()
        {
            if (!Int32.TryParse(ConfigurationManager.AppSettings["blockSize"], out _blockSize))
                _blockSize = 10;
            // Считывание количества блюд, отображаемых в каждой категории:
            if (!int.TryParse(ConfigurationManager.AppSettings["CountOfDishesMain"], out _CountOfDishesMain))
                _CountOfDishesMain = 6;
            // Считывание количества блюд, отображаемых в категории при выборе фильтра по категории:
            if (!int.TryParse(ConfigurationManager.AppSettings["CountOfDishesFilter"], out _CountOfDishesFilter))
                _CountOfDishesFilter = 30;
        }

        [ActivatorUtilitiesConstructor]
        public CafeController(
            IAccountService accountSevice,
            IProfileService profileClient,
            IAddressesService addressServiceClient,
            IBanketsService banketsService,
            ICafeService cafeService,
            ICompanyService companyService,
            IDishCategoryService dishCategoryService,
            IDishService dishService,
            ICompanyOrderService companyOrderServiceClient,
            IOrderItemService orderItemServiceClient,
            IOrderService orderServiceClient,
            IRatingService ratingServiceClient,
            ITagService tagServiceClient,
            IUsersService userServiceClient,
            IMenuService menuServiceClient,
            ICityService cityService) :
            base(accountSevice, profileClient, addressServiceClient,
                banketsService, cafeService, companyService, dishCategoryService,
                dishService, companyOrderServiceClient, orderItemServiceClient,
                orderServiceClient, ratingServiceClient, tagServiceClient,
                userServiceClient, menuServiceClient, cityService)
        {
            if (!Int32.TryParse(ConfigurationManager.AppSettings["blockSize"], out _blockSize))
                _blockSize = 10;
            // Считывание количества блюд, отображаемых в каждой категории:
            if (!int.TryParse(ConfigurationManager.AppSettings["CountOfDishesMain"], out _CountOfDishesMain))
                _CountOfDishesMain = 6;
            // Считывание количества блюд, отображаемых в категории при выборе фильтра по категории:
            if (!int.TryParse(ConfigurationManager.AppSettings["CountOfDishesFilter"], out _CountOfDishesFilter))
                _CountOfDishesFilter = 30;
        }

        public IActionResult Advertisments(int count)
        {
            List<byte> cafes = new List<byte>();

            string banners = Path.Combine(Environment.CurrentDirectory, "/Image/TopCafe");
            int maxId = Directory.GetFiles(banners, "*.png").Length;

            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < count; i++)
                {
                    byte[] randomNumber = new byte[1];

                    do
                    {
                        rngCsp.GetNonZeroBytes(randomNumber);
                    } while (randomNumber[0] > maxId || cafes.Contains(randomNumber[0]));
                    cafes.Add(randomNumber[0]);
                }
            }

            return PartialView("_Advertisments", cafes);
        }

        [AjaxOnly]
        public async Task<ActionResult> Filter(long? cafeId, string filter, List<long> tags)
        {
            if (!string.IsNullOrEmpty(filter)) filter = filter.Trim().ToLower();
            if (cafeId.HasValue)
                return await FilterMenu(cafeId.Value, filter, tags);
            else
                return await FilterMenuMainPage(filter, tags);
        }

        [HttpGet]
        public async Task<ActionResult> CafeIndex(string name, DateTime? d)
        {
            var availableCafes = await _cafeService.GetCafesToCurrentUserAsync();

            if (!string.IsNullOrEmpty(name))
            {
                var cafe = (CafeModel)ViewBag.Cafe;

                if (!cafe.IsActive)
                    return NotFound();

                NavigatorViewModel.Step = EnumNavigatorStep.SelectDishes;
                NavigatorViewModel.CafeName = cafe.CleanUrlName;
                if (
                    (d.HasValue && (d.Value.Date < DateTime.Now.Date || d.Value.Date >= DateTime.Now.Date.AddDays(7))))
                    return RedirectToAction(nameof(CafeIndex), new { name });
                var menu = await GetMenuModel(d);

                menu.IsCompanyEmployee = await CompanyOrderServiceClient.CheckUserIsEmployee(null, cafe.Id, DateTime.Now);

                var cart = CartViewModel.Load(HttpContext.Session);
                cart.IsCompanyEmployee = menu.IsCompanyEmployee;
                if (cart.CartItems.Count == 0 && cart.Cafe != null && cart.Cafe.Id == cafe.Id && !cafe.IsClosed())
                {
                    cart.Cafe = cafe;
                    CartViewModel.Save(HttpContext.Session, cart);
                }

                return View("Menu", menu);
            }

            List<CafeModel> cafes = await _cafeService.GetCafesAsync();
            CafeListViewModel model = new CafeListViewModel(cafes, availableCafes);
            NavigatorViewModel.Step = EnumNavigatorStep.ChooseCafe;

            return View("Index", model);
        }

        [HttpGet]
        public async Task<ActionResult> Index(string name, DateTime? d)
        {
            var city = HttpContext.Session.GetCurrentCity();
            var isCafeSubdomain = !string.IsNullOrEmpty(name);
            if (isCafeSubdomain && d.HasValue && (d.Value.Date < DateTime.Now.Date || d.Value.Date >= DateTime.Now.Date.AddDays(7)))
                return RedirectToAction(nameof(Index), new { name });
            var date = d?.Date ?? DateTime.Now.Date;
            List<CompanyServersModel> availableServers;
            if (User.Identity.IsAuthenticated && !User.Identity.IsAnonymous())
            {
                availableServers = await _cafeService.GetCafesAvailableToEmployee(new List<DateTime> { date }, city?.Id);
            }
            else
            {
                availableServers = new List<CompanyServersModel>();
            }

            long? switchCompanyId = await ProfileClient.GetUserCompanyId();

            if (isCafeSubdomain)
            {
                var cafe = (CafeModel)ViewBag.Cafe;

                System.Diagnostics.Debug.Assert(cafe != null);

                if (cafe is null || !cafe.IsActive)
                    return NotFound();

                NavigatorViewModel.Step = EnumNavigatorStep.SelectDishes;
                NavigatorViewModel.CafeName = cafe.CleanUrlName;
                var menu = await GetMenuModel(d);

                menu.IsCompanyEmployee = await CompanyOrderServiceClient.CheckUserIsEmployee(null, cafe.Id, DateTime.Now);

                var cart = CartViewModel.Load(HttpContext.Session);
                cart.IsCompanyEmployee = menu.IsCompanyEmployee;
                if (cart.CartItems.Count == 0 && cart.Cafe != null && cart.Cafe.Id == cafe.Id && !cafe.IsClosed())
                {
                    cart.Cafe = cafe;
                    CartViewModel.Save(HttpContext.Session, cart);
                }
                return View("Menu", menu);
            }

            var viewModel = new Random6DishesViewModel()
            {
                Menu = new MenuViewModel()
            };

            List<FoodCategoryWithDishes> cats = await MenuServiceClient.GetDishesForDate(name, _CountOfDishesMain, companyId: switchCompanyId, cityId: city?.Id);
            string ErrorMessage = null;
            var newSortedCategories = new List<FoodCategoryModel>();
            var newDefaultCategories = new List<FoodCategoryWithDishes>();
            var idSortOfCategory = new List<long>();
            if (cats == null)
            {
                viewModel.Menu.StorageCategories = new List<FoodCategoryModel>();
                ErrorMessage = "Ошибка сервера, пожалуйста обратитесь позже";
            }
            else
            {
                // Хранит ссылки кафе
                var cafeUrlList = new List<string>();
                foreach (var item in cats)
                {
                    if (item.Category.Id > 0)
                    {
                        idSortOfCategory.Add(item.Category.Id);
                    }
                    foreach (var dish in item.Dishes)
                    {
                        // Если ссылка не находится в списке, добавляем.
                        if (!cafeUrlList.Contains(dish.CafeUrl) && !string.IsNullOrWhiteSpace(dish.CafeUrl))
                        {
                            cafeUrlList.Add(dish.CafeUrl);
                        }
                    }
                }
                // Если кафе одно и если город подтвержден
                if (cafeUrlList.Count == 1 && HttpContext.Session.IsCityConfirmed())
                {
                    // Создаем ссылку на кафе
                    var hostBuilder = new StringBuilder();
                    // Добавляем http или https
                    hostBuilder.Append($"{HttpContext.Request.Scheme}://");
                    var config = HttpContext.RequestServices?.GetRequiredService<IConfiguration>();
                    var domainName = config?.GetSection("AppSettings:Domain")?.Value.ToLowerInvariant();
                    // Добавляем субдомен и домен
                    hostBuilder.Append(cafeUrlList[0]).Append(".").Append(domainName);
                    // Добавляем порт при наличии
                    if (HttpContext.Request.Host.Port != null)
                        hostBuilder.Append(":").Append(HttpContext.Request.Host.Port.Value.ToString());
                    // Перенаправляем пользователя на страницу кафе
                    return Redirect(hostBuilder.ToString());
                }
                #region СортировкаПоId
                //idSortOfCategory.Sort();
                //foreach (var item in idSortOfCategory)
                //{
                //    for (int i = 0; i < cats.Count; i++)
                //    {
                //        if (item == cats[i].Category.Id)
                //        {
                //            newDefaultCategories.Add(cats[i]);
                //        }
                //    }
                //}
                //foreach(var item in newDefaultCategories)
                //{
                //    newSortedCategories.Add(item.Category);
                //}
                //viewModel.Menu.StorageCategories = newSortedCategories.ToList();
                #endregion

                var sortedCategories = new Dictionary<string, FoodCategoryWithDishes>
                {
                    { "салаты", null },
                    { "супы", null },
                    { "гарниры", null },
                    { "горячие блюда", null },
                    { "выпечка", null },
                    { "торты", null },
                };
                foreach (var item in cats)
                {
                    if (sortedCategories.ContainsKey(item.Category.Name.ToLower()))
                    {
                        sortedCategories[item.Category.Name.ToLower()] = item;
                    }
                }
                // Добавляет категории из sortedCategories
                newDefaultCategories = sortedCategories.Values.ToList();
                // Добавляет оставшиеся категории в конец списка
                newDefaultCategories.AddRange(cats.Except(newDefaultCategories));
                newDefaultCategories = newDefaultCategories.Where(c => c != null).ToList();
                newSortedCategories = newDefaultCategories.Select(c => c.Category).ToList();
                viewModel.Menu.StorageCategories = newSortedCategories.ToList();

            }
            cats = newDefaultCategories.Select(c => c).ToList();
            viewModel.FoodCategoryWithDishes = cats;
            TempData["Categories"] = JsonConvert.SerializeObject(viewModel.Menu.Categories);
            ViewBag.ErrorMessage = ErrorMessage;

            NavigatorViewModel.Step = EnumNavigatorStep.SelectDishes;
            if (!string.IsNullOrEmpty(name))
            {
                var cafe = (CafeModel)ViewBag.Cafe;
                NavigatorViewModel.CafeName = cafe.CleanUrlName;
            }
            viewModel.CountOfDishesMain = _CountOfDishesMain;
            viewModel.CountOfDishesFilter = _CountOfDishesFilter;
            return View("Random6Dishes", viewModel);
        }

        [AjaxOnly]
        [HttpPost]
        public async Task<ActionResult> ShowTheRest(long categoryId, ICollection<long> dishIds, string name)
        {
            RestOfCategory rest = new RestOfCategory()
            {
                CompanyId = await ProfileClient.GetUserCompanyId(),
                CategoryId = categoryId,
                DishIds = dishIds,
                RestName = name
            };
            List<FoodDishModel> dishes = await MenuServiceClient.GetRestOfCat(rest);
            if (dishes == null)
                dishes = new List<FoodDishModel>();
            return PartialView("_RestOfCategory", dishes);
        }

        /// <summary>
        /// Метод для получения первых скольких-то блюд из указанной категории
        /// Должен применяться при выборе фильтра по категории
        /// </summary>
        /// <param name="id">Идентфикатор категории, из которой нужно получить блюда</param>
        /// <returns></returns>
        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> TopDishesFromCat(long id)
        {
            // Получение всех категорий.
            List<FoodCategoryWithDishes> cats = await MenuServiceClient.GetDishesForDate(
                name: null, countDishes: _CountOfDishesFilter, filter: null, dishCatId: id);

            FoodCategoryWithDishes oneCat = cats.FirstOrDefault(c => c.Category.Id == id);
            if (oneCat == null)
                oneCat = new FoodCategoryWithDishes()
                {
                    CountOfDishes = 0,
                    Category = new FoodCategoryModel() { Id = id }
                };
            cats = new List<FoodCategoryWithDishes>();
            cats.Add(oneCat);
            var viewModel = new Random6DishesViewModel()
            {
                FoodCategoryWithDishes = cats,
                Menu = new MenuViewModel()
                {
                    StorageCategories = new List<FoodCategoryModel>()
                },
                CountOfDishesFilter = _CountOfDishesFilter,
                CountOfDishesMain = _CountOfDishesMain
            };
            return PartialView("_TopDishesFromCat", viewModel);
        }

        [AjaxOnly]
        public async Task<ActionResult> SetRating(long id, int rating, bool companyOnly = false)
        {
            ViewData["companyOnly"] = companyOnly;
            ModelState.Clear();
            if (!companyOnly && !User.Identity.IsAnonymous())
            {
                var result = await RatingServiceClient.InsertNewRating(id, ObjectTypesEnum.CAFE, rating);
            }

            var cafe = await _cafeService.GetCafeById(id);
            return PartialView("_Rating", cafe);
        }

        public async Task<ActionResult> Tags(long? id)
        {
            List<TagModel> allTags;
            if (id.HasValue)
                allTags = await TagServiceClient.GetListOfTagsConnectedWithObjectAndHisChild(id.Value, (long)ObjectTypesEnum.CAFE);
            else
                allTags = await TagServiceClient.GetAllTags();

            List<TagModel> treeTags = TagsTreeBuilder.BuildTagsTree(allTags);
            ViewBag.CafeId = id;

            return PartialView("_Tags", treeTags);
        }

        /// <summary>
        /// Получить блюда по фильтру с главной страницы
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        private async Task<ActionResult> FilterMenuMainPage(string filter, List<long> tags)
        {
            // Количество блюд, которые надо выводить при поиске. Если задана строка поиска или тэги - тогда будем выводить все блюда, которые нашлись. Если ни строка, ни тэги не заданы - считаем, что производится отмена поиска и надо показать всё так, как на главной странице - т.е. сколько-то первых блюд и остальные по кнопке "Показать ещё +N":
            int? currentDishCount;
            if (string.IsNullOrEmpty(filter) && (tags == null || tags.Count <= 0))
                currentDishCount = _CountOfDishesMain;
            else
                currentDishCount = null;

            var companyId = await ProfileClient.GetUserCompanyId();
            List<FoodCategoryWithDishes> cats = await MenuServiceClient.GetDishesForDate(countDishes: currentDishCount, filter: filter, companyId: companyId);
            string ErrorMessage = null;
            if (cats == null)
            {
                TempData["Categories"] = JsonConvert.SerializeObject(new List<FoodCategoryModel>());
                ErrorMessage = "Ошибка сервера, пожалуйста обратитесь позже";
            }
            else
                TempData["Categories"] = JsonConvert.SerializeObject(cats.Select(e => e.Category).ToList());

            ViewBag.ErrorMessage = ErrorMessage;
            // Если при отображении фильтра показываются сразу все блюда, кнопку "Показать ещё +N" нужно убрать:
            if (currentDishCount == null)
                ViewBag.RestButtonState = "none";
            var viewModel = new Random6DishesViewModel()
            {
                FoodCategoryWithDishes = cats,
                Menu = new MenuViewModel(),
                CountOfDishesMain = _CountOfDishesMain,
                CountOfDishesFilter = _CountOfDishesFilter
            };
            return PartialView("_MenuAllCafe", viewModel);
        }

        private async Task<ActionResult> FilterMenu(long cafeId, string filter, List<long> tags)
        {
            Dictionary<FoodCategoryModel, List<FoodDishModel>> menu;

            if (string.IsNullOrEmpty(filter) && (tags == null || tags.Count == 0))
                menu = await MenuServiceClient.GetDishesByScheduleByDate(cafeId, DateTime.Now);
            else
                menu = await MenuServiceClient.GetDishesByScheduleByDateByTagListAndSearchTerm(cafeId,
                    DateTime.Now, filter, tags);

            var clientDiscount = AsyncHelper.RunSync(() => _cafeService.GetDiscountAmount(cafeId, DateTime.Now, null));
            ViewBag.Cafe = await _cafeService.GetCafeById(cafeId);
            ViewBag.ClientDiscount = clientDiscount;
            TempData["Categories"] = JsonConvert.SerializeObject(menu.Keys.ToList());
            return PartialView("_Menu", menu);
        }

        public async Task<double> GetTotalPoints()
        {
            return Math.Round(await UserServiceClient.GetUserPointsByLogin(User.Identity.Name), 2);
        }

        public string GetCafeName(string cleanUrlName)
        {
            var cafe = AsyncHelper.RunSync(() => _cafeService.GetCafeByCleanUrl(cleanUrlName));
            return cafe.Name;
        }

        public ActionResult UpdateCategoriesByFilter()
        {
            var categories = TempData.ContainsKey("Categories") ? JsonConvert.DeserializeObject<List<FoodCategoryModel>>((string)TempData["Categories"]) : new List<FoodCategoryModel>();
            return PartialView("_Categories", categories);
        }
    }
}
