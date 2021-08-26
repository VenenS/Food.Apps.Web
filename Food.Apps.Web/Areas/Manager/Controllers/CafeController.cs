using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Areas.Manager.Models;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using CafeInfoModel = ITWebNet.Food.Site.Areas.Manager.Models.CafeInfoModel;

namespace ITWebNet.Food.Site.Areas.Manager.Controllers
{
    public class CafeController : BaseController
    {
        private readonly IContentServiceClient _contentService;
        private readonly ICityService _cityService;
        private readonly IKitchenService _kitchenService;

        public CafeController(
            IMemoryCache cache,
            IContentServiceClient contentService,
            ICityService cityService,
            ICafeService cafeService,
            IKitchenService kitchenService) : base(cache, cafeService)
        {
            _contentService = contentService;
            _cityService = cityService;
            _kitchenService = kitchenService;
        }

        [ActivatorUtilitiesConstructor]
        public CafeController(IMemoryCache cache, ICafeService cafeService, IContentServiceClient contentService, IKitchenService kitchenService) : base(cache, cafeService)
        {
            _contentService = contentService;
            _kitchenService = kitchenService;
        }

        public ActionResult AddCafeBusinessHoursDatedItem(string prefix)
        {
            ViewData["Prefix"] = prefix;

            CafeBusinessHoursDatedItemModel model = new CafeBusinessHoursDatedItemModel
            {
                Date = DateTime.Today
            };

            return PartialView("EditorTemplates/CafeBusinessHoursDatedItemModel", model);
        }

        public ActionResult AddCafeBusinessHoursItem(string prefix)
        {
            ViewData["Prefix"] = prefix;

            CafeBusinessHoursItemModel model = new CafeBusinessHoursItemModel();

            return PartialView("EditorTemplates/CafeBusinessHoursItemModel", model);
        }

        public ActionResult AddDeliveryCostItem(long cafeId, bool forCompanyOrders)
        {
            CafeCostOfDeliveryModel model = new CafeCostOfDeliveryModel();
            model.ForCompanyOrders = forCompanyOrders;

            model.Id = -1;

            return PartialView("EditorTemplates/CafeCostOfDeliveryModel", model);
        }

        public async Task RemoveDeliveryCostItem(long itemId)
        {
            try
            {
                if (itemId != -1)
                    await _cafeService.RemoveCostOfDelivery(itemId);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ActionResult> Index(long cafeId)
        {
            CafeInfoModel model = await LoadCafeInfo(cafeId);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> EditCafeInfo(CafeInfoModel model)
        {
            var payments = new Dictionary<PaymentTypeEnum, bool>();
            foreach (PaymentTypeEnum item in Enum.GetValues(typeof(PaymentTypeEnum)))
            {
                if (item == 0 || payments.ContainsKey(item)) continue;
                payments.Add(item, model.PaymentTypes != null && model.PaymentTypes.ContainsKey(item));
            }
            model.PaymentTypes = payments;
            if (!ModelState.IsValid)
                return PartialView("_EditCafeInfo", model);

            if (!await SaveCafeInfo(model))
                return PartialView("_EditCafeInfo", model);

            model = await LoadCafeInfo(model.CafeId);

            return PartialView("_ViewCafeInfo", model);
        }

        [HttpPost]
        public async Task<ActionResult> ViewCafeInfo([FromForm] long cafeId)
        {
            var model = await LoadCafeInfo(cafeId);
            return PartialView("_EditCafeInfo", model);
        }

        [HttpPost]
        public async Task StopNotifyAboutOrders()
        {
            var userId = User.Identity.GetUserId();
            await new CafeOrderNotificationService().StopNotifyUser(Convert.ToInt64(userId));
        }

        protected async Task<CafeInfoModel> LoadCafeInfo(long cafeId)
        {
            CafeModel info = await _cafeService.GetCafeByIdIgnoreActivity(cafeId);

            var model = new CafeInfoModel
            {
                CompanyOnly = (info.CafeType == CafeType.CompanyOnly),
                BusinessHours = new CafeBusinessHoursModel(),
                CafeId = cafeId,
                Description = info.Description,
                ShortDescription = info.ShortDescription,
                Name = info.Name,
                CostOfDeliveryPersonal = new List<CafeCostOfDeliveryModel>(),
                CostOfDeliveryCompany = new List<CafeCostOfDeliveryModel>(),
                WeekMenuIsActive = info.WeekMenuIsActive,
                HeadPicture = info.SmallImage,
                Logotype = info.BigImage,
                Address = info.Address,
                CafeIcon = info.Logo,
                Phone = info.Phone,
                DeferredOrder = info.DeferredOrder,
                DailyCorpOrderSum = info.DaylyCorpOrderSum,
                OrderAbortTime = info.OrderAbortTime,
                AverageDeliveryTime = info.AverageDeliveryTime,
                Kitchens = (await _kitchenService.GetCurrentListOfKitchenToCafe(info.Id))?.ToList() ?? new List<Core.DataContracts.Manager.KitchenModel>(),
                DeliveryComment = info.DeliveryComment,
                Regions = info.Regions,
                MinimumOrderSum = info.MinimumOrderSum
            };
            model.PaymentTypes = new Dictionary<PaymentTypeEnum, bool>();
            foreach (PaymentTypeEnum item in Enum.GetValues(typeof(PaymentTypeEnum)))
            {
                if (item == 0 || model.PaymentTypes.ContainsKey(item)) continue;
                model.PaymentTypes.Add(item, (info.PaymentMethod | item) == info.PaymentMethod);
            }

            var kitchens = await _kitchenService.GetFullListOfKitchen();
            model.SelectableKitchens = kitchens.Select(k => new SelectableKitchenModel
            {
                Kitchen = k,
                IsSelected = info.Kitchens.Select(i => i.Id).Contains(k.Id)
            }).ToList();

            var businessHours = await _cafeService.GetCafeBusinessHours(cafeId);
            var notifications =
                await _cafeService.GetNotificationContactsToCafe(cafeId, NotificationChannelModel.Email);
            model.NotificationContact = notifications != null && notifications.Count > 0
                ? notifications.OrderBy(c => c.Id).First().NotificationContact
                : string.Empty;

            model.BusinessHours.Friday = businessHours.Friday.Select(i => new CafeBusinessHoursItemModel
            {
                ClosingTime = i.ClosingTime,
                OpeningTime = i.OpeningTime
            }).ToList();

            model.BusinessHours.Monday = businessHours.Monday.Select(i => new CafeBusinessHoursItemModel
            {
                ClosingTime = i.ClosingTime,
                OpeningTime = i.OpeningTime
            }).ToList();

            model.BusinessHours.Saturday = businessHours.Saturday.Select(i => new CafeBusinessHoursItemModel
            {
                ClosingTime = i.ClosingTime,
                OpeningTime = i.OpeningTime
            }).ToList();

            model.BusinessHours.Sunday = businessHours.Sunday.Select(i => new CafeBusinessHoursItemModel
            {
                ClosingTime = i.ClosingTime,
                OpeningTime = i.OpeningTime
            }).ToList();

            model.BusinessHours.Thursday = businessHours.Thursday.Select(i => new CafeBusinessHoursItemModel
            {
                ClosingTime = i.ClosingTime,
                OpeningTime = i.OpeningTime
            }).ToList();

            model.BusinessHours.Tuesday = businessHours.Tuesday.Select(i => new CafeBusinessHoursItemModel
            {
                ClosingTime = i.ClosingTime,
                OpeningTime = i.OpeningTime
            }).ToList();

            model.BusinessHours.Wednesday = businessHours.Wednesday.Select(i => new CafeBusinessHoursItemModel
            {
                ClosingTime = i.ClosingTime,
                OpeningTime = i.OpeningTime
            }).ToList();

            model.BusinessHours.DatedItems = new List<CafeBusinessHoursDatedItemModel>();

            foreach (var departure in businessHours.Departures)
            {
                model.BusinessHours.DatedItems.AddRange(departure.Items.Select(i => new CafeBusinessHoursDatedItemModel
                {
                    ClosingTime = i.ClosingTime.TimeOfDay,
                    Date = departure.Date,
                    IsDayOff = departure.IsDayOff,
                    OpeningTime = i.OpeningTime.TimeOfDay
                }));
            }

            List<CostOfDeliveryModel> costOfDelivery = await _cafeService.GetListOfCostOfDelivery(cafeId, null);

            model.CostOfDeliveryCompany.AddRange(costOfDelivery.Where(c => c.ForCompanyOrders).Select(i => new CafeCostOfDeliveryModel
            {
                Id = i.Id,
                DeliveryPrice = i.DeliveryPrice,
                OrderPriceFrom = i.OrderPriceFrom,
                OrderPriceTo = i.OrderPriceTo,
                ForCompanyOrders = true,
                IsEdit = false
            }));

            model.CostOfDeliveryPersonal.AddRange(costOfDelivery.Where(c => !c.ForCompanyOrders).Select(i => new CafeCostOfDeliveryModel
            {
                Id = i.Id,
                DeliveryPrice = i.DeliveryPrice,
                OrderPriceFrom = i.OrderPriceFrom,
                OrderPriceTo = i.OrderPriceTo,
                ForCompanyOrders = false,
                IsEdit = false
            }));

            return model;
        }

        protected async Task<bool> SaveCafeInfo(CafeInfoModel model)
        {
            async Task<(string, bool)> TryUploadImage(IFormFile file, string modelStateKey)
            {
                var (hash, err) = await UploadImage(file);
                if (err != null)
                {
                    err = "Контент сервис вернул ошибку: " + err;
                    ModelState.AddModelError(nameof(modelStateKey), err);
                    return (null, false);
                }
                return (hash, true);
            }

            if (model.PaymentTypes == null || !model.PaymentTypes.ContainsValue(true))
            {
                ModelState.AddModelError(nameof(model.PaymentTypes), "Выберите способы оплаты");
                return false;
            }

            var prev = await _cafeService.GetCafeByIdIgnoreActivity(model.CafeId);
            var info = new Core.DataContracts.Manager.CafeInfoModel
            {
                CafeId = model.CafeId,
                Description = model.Description,
                ShortDescription = model.ShortDescription,
                WeekMenuIsActive = model.WeekMenuIsActive,
                NotificationContact = model.NotificationContact,
                Address = model.Address,
                Phone = model.Phone,
                DeferredOrder = model.DeferredOrder,
                DailyCorpOrderSum = model.DailyCorpOrderSum ?? 0,
                HeadPicture = prev.SmallImage,
                Logotype = prev.BigImage,
                CafeIcon = prev.Logo,
                OrderAbortTime = model.OrderAbortTime,
                PaymentMethod = (PaymentTypeEnum)model.PaymentTypes.Where(p => p.Value).Sum(p => (short)p.Key),
                AverageDeliveryTime = model.AverageDeliveryTime,
                Kitchens = model.SelectableKitchens.Where(k => k.IsSelected).Select(k => k.Kitchen).ToList(),
                DeliveryComment = model.DeliveryComment,
                MinimumOrderSum = model.MinimumOrderSum,
                Regions = model.Regions
            };

            // Загрузка изображений в контент сервис.
            //
            // FIXME: если не удалось загрузить один из файлов, необходимо сделать
            // rollback ранее загруженных файлов. В контент сервисе в данный момент (11.08.2020)
            // нет такой возможности.
            var ok = false;
            if (model.LogotypeFile != null)
            {
                (info.Logotype, ok) = await TryUploadImage(model.LogotypeFile, nameof(model.LogotypeFile));
                if (!ok)
                    return false;
            }
            else if (string.IsNullOrEmpty(model.Logotype))
            {
                info.Logotype = null;
            }

            if (model.HeadPictureFile != null)
            {
                (info.HeadPicture, ok) = await TryUploadImage(model.HeadPictureFile, nameof(model.HeadPictureFile));
                if (!ok)
                    return false;
            }
            else if (string.IsNullOrEmpty(model.HeadPicture))
            {
                info.HeadPicture = null;
            }

            if (model.CafeIconFile != null)
            {
                (info.CafeIcon, ok) = await TryUploadImage(model.CafeIconFile, nameof(model.CafeIconFile));
                if (!ok)
                    return false;
            }
            else if (string.IsNullOrEmpty(model.CafeIcon))
            {
                info.CafeIcon = null;
            }

            await _cafeService.SetCafeInfo(info);
            model.BusinessHours = model.BusinessHours ?? new CafeBusinessHoursModel();
            model.BusinessHours.DatedItems = model.BusinessHours.DatedItems ?? new List<CafeBusinessHoursDatedItemModel>();
            model.BusinessHours.Friday = model.BusinessHours.Friday ?? new List<CafeBusinessHoursItemModel>();
            model.BusinessHours.Monday = model.BusinessHours.Monday ?? new List<CafeBusinessHoursItemModel>();
            model.BusinessHours.Saturday = model.BusinessHours.Saturday ?? new List<CafeBusinessHoursItemModel>();
            model.BusinessHours.Sunday = model.BusinessHours.Sunday ?? new List<CafeBusinessHoursItemModel>();
            model.BusinessHours.Thursday = model.BusinessHours.Thursday ?? new List<CafeBusinessHoursItemModel>();
            model.BusinessHours.Tuesday = model.BusinessHours.Tuesday ?? new List<CafeBusinessHoursItemModel>();
            model.BusinessHours.Wednesday = model.BusinessHours.Wednesday ?? new List<CafeBusinessHoursItemModel>();

            var businessHours = new Core.DataContracts.Manager.CafeBusinessHoursModel
            {
                CafeId = model.CafeId,
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

            // Редактирование стоимости доставки
            List<CafeCostOfDeliveryModel> changedCosts = new List<CafeCostOfDeliveryModel>();
            if (model.CostOfDeliveryPersonal != null)
                changedCosts.AddRange(model.CostOfDeliveryPersonal.Where(i => i.IsEdit).ToList());
            if (model.CostOfDeliveryCompany != null)
                changedCosts.AddRange(model.CostOfDeliveryCompany.Where(i => i.IsEdit).ToList());


            foreach (var item in changedCosts)
            {
                if (item.Id != -1)
                {
                    await
                        _cafeService.EditCostOfDelivery(new CostOfDeliveryModel
                        {
                            Id = item.Id,
                            OrderPriceFrom = item.OrderPriceFrom,
                            OrderPriceTo = item.OrderPriceTo,
                            DeliveryPrice = item.DeliveryPrice,
                            ForCompanyOrders = item.ForCompanyOrders
                        });
                }
                else
                {
                    await
                        _cafeService.AddNewCostOfDelivery(new CostOfDeliveryModel
                        {
                            CafeId = model.CafeId,
                            OrderPriceFrom = item.OrderPriceFrom,
                            OrderPriceTo = item.OrderPriceTo,
                            DeliveryPrice = item.DeliveryPrice,
                            ForCompanyOrders = item.ForCompanyOrders
                        });
                }

            }

            await _cafeService.SetCafeBusinessHours(businessHours);
            return true;
        }

        /// <summary>
        /// Загружает изображение в контент сервис и возвращает хэш изображения
        /// полученного от контент сервиса или ошибку.
        /// </summary>
        private async Task<(string url, string err)> UploadImage(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                var result = await _contentService.PostImage(file.FileName, stream);
                if (result.Succeeded)
                    return (result.Content, null);
                else
                    return (null, result.Message);
            }
        }

        [AjaxOnly, HttpPost]
        public IActionResult GetLinkAddBusinessHoursItem(string day) => PartialView("_AddBusinessHoursItemLink", day);
    }
}