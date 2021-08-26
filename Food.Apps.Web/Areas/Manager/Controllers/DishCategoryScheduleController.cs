using ITWebNet.Food.Site.Areas.Manager.Models;
using ITWebNet.Food.Site.Areas.Manager.Models.DishCategorySchedule;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Models;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Areas.Manager.Controllers
{
    public class DishCategoryScheduleController : BaseController
    {
        private readonly ICategoriesService _categoryService;

        public DishCategoryScheduleController(
            IMemoryCache cache,
            ICategoriesService categoryService,
            ICafeService cafeService
            ) : base(cache, cafeService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index([FromRoute] long cafeId, long dishCategoryId)
        {
            var viewModel = new DishCategorySchedulePageViewModel()
            {
                CafeId = cafeId,
                CategoryId = dishCategoryId,
                BusinessHours = new CafeBusinessHoursModel(),
            };

            var categoryBusinesHours = await _categoryService.GetCategoryBusinessHours(cafeId, dishCategoryId);

            if (categoryBusinesHours != null)
            {
                viewModel.BusinessHours.Friday = categoryBusinesHours.Friday?.Select(i => new CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList();

                viewModel.BusinessHours.Monday = categoryBusinesHours.Monday?.Select(i => new CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList();

                viewModel.BusinessHours.Saturday = categoryBusinesHours.Saturday?.Select(i => new CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList();

                viewModel.BusinessHours.Sunday = categoryBusinesHours.Sunday?.Select(i => new CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList();

                viewModel.BusinessHours.Thursday = categoryBusinesHours.Thursday?.Select(i => new CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList();

                viewModel.BusinessHours.Tuesday = categoryBusinesHours.Tuesday?.Select(i => new CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList();

                viewModel.BusinessHours.Wednesday = categoryBusinesHours.Wednesday?.Select(i => new CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList();

                viewModel.BusinessHours.DatedItems = new List<CafeBusinessHoursDatedItemModel>();

                if (categoryBusinesHours.Departures != null)
                {
                    foreach (var departure in categoryBusinesHours.Departures)
                    {
                        viewModel.BusinessHours.DatedItems.AddRange(departure.Items.Select(i => new CafeBusinessHoursDatedItemModel
                        {
                            ClosingTime = i.ClosingTime.TimeOfDay,
                            Date = departure.Date,
                            IsDayOff = departure.IsDayOff,
                            OpeningTime = i.OpeningTime.TimeOfDay
                        }));
                    }
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Index(DishCategorySchedulePageViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.BusinessHours = model.BusinessHours ?? new CafeBusinessHoursModel();
            model.BusinessHours.DatedItems = model.BusinessHours.DatedItems ?? new List<CafeBusinessHoursDatedItemModel>();
            model.BusinessHours.Friday = model.BusinessHours.Friday ?? new List<CafeBusinessHoursItemModel>();
            model.BusinessHours.Monday = model.BusinessHours.Monday ?? new List<CafeBusinessHoursItemModel>();
            model.BusinessHours.Saturday = model.BusinessHours.Saturday ?? new List<CafeBusinessHoursItemModel>();
            model.BusinessHours.Sunday = model.BusinessHours.Sunday ?? new List<CafeBusinessHoursItemModel>();
            model.BusinessHours.Thursday = model.BusinessHours.Thursday ?? new List<CafeBusinessHoursItemModel>();
            model.BusinessHours.Tuesday = model.BusinessHours.Tuesday ?? new List<CafeBusinessHoursItemModel>();
            model.BusinessHours.Wednesday = model.BusinessHours.Wednesday ?? new List<CafeBusinessHoursItemModel>();

            var businessHours = new Core.DataContracts.Manager.CategoryBusinessHoursModel
            {
                CafeId = model.CafeId,
                CategoryId = model.CategoryId,
                Friday = model.BusinessHours.Friday.Select(i => new Core.DataContracts.Manager.CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList(),
                Monday = model.BusinessHours.Monday.Select(i => new Core.DataContracts.Manager.CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList(),
                Saturday = model.BusinessHours.Saturday.Select(i => new Core.DataContracts.Manager.CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList(),
                Sunday = model.BusinessHours.Sunday.Select(i => new Core.DataContracts.Manager.CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList(),
                Thursday = model.BusinessHours.Thursday.Select(i => new Core.DataContracts.Manager.CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList(),
                Tuesday = model.BusinessHours.Tuesday.Select(i => new Core.DataContracts.Manager.CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList(),
                Wednesday = model.BusinessHours.Wednesday.Select(i => new Core.DataContracts.Manager.CafeBusinessHoursItemModel
                {
                    ClosingTime = i.ClosingTime,
                    OpeningTime = i.OpeningTime
                }).ToList()
            };

            businessHours.Departures = new List<Core.DataContracts.Manager.CafeBusinessHoursDepartureModel>();

            foreach (var datedItem in model.BusinessHours.DatedItems)
            {
                var departure = businessHours.Departures.FirstOrDefault(d => d.Date == datedItem.Date);
                if (departure != null)
                {
                    if (datedItem.IsDayOff)
                    {
                        departure.IsDayOff = true;
                    }

                    departure.Items.Add(new Core.DataContracts.Manager.CafeBusinessHoursItemModel
                    {
                        ClosingTime = datedItem.Date.Date + datedItem.ClosingTime,
                        OpeningTime = datedItem.Date.Date + datedItem.OpeningTime
                    });
                }
                else
                {
                    departure = new Core.DataContracts.Manager.CafeBusinessHoursDepartureModel
                    {
                        Date = datedItem.Date.Date,
                        IsDayOff = datedItem.IsDayOff,
                        Items = new List<Core.DataContracts.Manager.CafeBusinessHoursItemModel>()
                    };

                    departure.Items.Add(new Core.DataContracts.Manager.CafeBusinessHoursItemModel
                    {
                        ClosingTime = datedItem.Date.Date + datedItem.ClosingTime,
                        OpeningTime = datedItem.Date.Date + datedItem.OpeningTime
                    });

                    businessHours.Departures.Add(departure);
                }
            }


            var httpResult = await _categoryService.SetCategoryBusinessHours(businessHours);
            if (httpResult.Succeeded)
                model.Message = MessageViewModel.Success("Расписание сохранено");
            else
                model.Message = MessageViewModel.Error("Произошла непредвиденная ошибка при сохранении расписания");

            return View(model);
        }

        public ActionResult AddBusinessHoursDatedItem(string prefix)
        {
            ViewData["Prefix"] = prefix;

            CafeBusinessHoursDatedItemModel model = new CafeBusinessHoursDatedItemModel
            {
                Date = DateTime.Today
            };

            return PartialView("EditorTemplates/BusinessHoursDatedItemModel", model);
        }

        public ActionResult AddBusinessHoursItem(string prefix)
        {
            ViewData["Prefix"] = prefix;

            CafeBusinessHoursItemModel model = new CafeBusinessHoursItemModel();

            return PartialView("EditorTemplates/BusinessHoursItemModel", model);
        }

        [AjaxOnly, HttpPost]
        public IActionResult GetLinkAddBusinessHoursItem(string day) => PartialView("_AddBusinessHoursItemLink", day);
    }
}