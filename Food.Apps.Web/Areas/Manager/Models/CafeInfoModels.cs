using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Food.Apps.Web.Attributes;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using Microsoft.AspNetCore.Http;

namespace ITWebNet.Food.Site.Areas.Manager.Models
{
    public class CafeBusinessHoursDatedItemModel : IValidatableObject
    {
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        [Display(Name = "Время закрытия")]
        public TimeSpan ClosingTime { get; set; }

        [Display(Name = "Время открытия")]
        public TimeSpan OpeningTime { get; set; }

        [Display(Name = "Выходной")]
        public bool IsDayOff { get; set; }

        public string Info { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsDayOff) {
                if (ClosingTime <= OpeningTime) {
                    Info = "Время закрытия должно быть больше времени открытия";
                    yield return new ValidationResult(Info);
                } else if (ClosingTime.TotalHours >= 24) {
                    Info = "Неверное время закрытия";
                    yield return new ValidationResult(Info);
                } else if (OpeningTime.TotalHours >= 24) {
                    Info = "Неверное время открытия";
                    yield return new ValidationResult(Info);
                }
            }
        }
    }

    public class CafeBusinessHoursModel : IValidatableObject
    {

        [Display(Name = "Время работы на дату")]
        public List<CafeBusinessHoursDatedItemModel> DatedItems { get; set; }
        [Display(Name = "Пятница")]
        public List<CafeBusinessHoursItemModel> Friday { get; set; }
        [Display(Name = "Понедельник")]
        public List<CafeBusinessHoursItemModel> Monday { get; set; }
        [Display(Name = "Суббота")]
        public List<CafeBusinessHoursItemModel> Saturday { get; set; }
        [Display(Name = "Воскресенье")]
        public List<CafeBusinessHoursItemModel> Sunday { get; set; }
        [Display(Name = "Четверг")]
        public List<CafeBusinessHoursItemModel> Thursday { get; set; }
        [Display(Name = "Вторник")]
        public List<CafeBusinessHoursItemModel> Tuesday { get; set; }
        [Display(Name = "Среда")]
        public List<CafeBusinessHoursItemModel> Wednesday { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var model = (CafeBusinessHoursModel)validationContext.ObjectInstance;
            var days = new List<List<CafeBusinessHoursItemModel>>();
            var dates = new List<List<CafeBusinessHoursDatedItemModel>>();
            days.Add(model.Monday);
            days.Add(model.Tuesday);
            days.Add(model.Wednesday);
            days.Add(model.Thursday);
            days.Add(model.Friday);
            days.Add(model.Saturday);
            days.Add(model.Sunday);
            dates.Add(model.DatedItems);

            foreach (var day in days)
            {
                if (day != null)
                {
                    var listBusinessHours = day.OrderBy(m => m.OpeningTime).ToList();
                    for (int i = 1; i < listBusinessHours.Count; i++)
                    {
                        if (listBusinessHours[i].OpeningTime <= listBusinessHours[i - 1].ClosingTime)
                           yield return new ValidationResult("Рабочее время не должно пересекаться", new List<string> { "Monday" });
                    }
                }
            }

            foreach (var date in dates)
            {
                if (date != null)
                {
                    var listBusinessHours = date.OrderBy(m => m.Date).ToList();
                    for (int i = 1; i < listBusinessHours.Count; i++)
                    {
                        if ((listBusinessHours[i].OpeningTime <= listBusinessHours[i - 1].ClosingTime) && (listBusinessHours[i].Date == listBusinessHours[i - 1].Date))
                           yield return new ValidationResult("Рабочее время не должно пересекаться", new List<string> { "DatedItems" });
                    }
                }
            }
        }
    }

    public class CafeBusinessHoursItemModel : IValidatableObject
    {
        [Display(Name = "Время закрытия")]
        public DateTime ClosingTime { get; set; }
        [Display(Name = "Время открытия")]
        public DateTime OpeningTime { get; set; }

        public string Info { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ClosingTime <= OpeningTime)
            {
                Info = "Дата закрытия должна быть больше даты открытия";
                yield return new ValidationResult("Дата закрытия должна быть больше даты открытия");
            }
        }
    }

    public class CafeCostOfDeliveryModel : IValidatableObject
    {
        public long Id { get; set; }
        [Display(Name = "Цена от")]
        public double OrderPriceFrom { get; set; }
        [Display(Name = "Цена до")]
        public double OrderPriceTo { get; set; }
        [Display(Name = "Стоимость доставки")]
        public double DeliveryPrice { get; set; }

        /// <summary>
        /// Тип доставки: true - для корпоративных заказов, false - для персональных заказов
        /// </summary>
        public bool ForCompanyOrders { get; set; }

        public bool IsEdit { get; set; }

        public string Info { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OrderPriceFrom < 0 || OrderPriceTo < 0.01)
            {
                Info = "Цена должна быть больше 0";
                yield return new ValidationResult("Цена должна быть больше 0");
            }
            if (DeliveryPrice < 0)
            {
                Info = "Неверно указана стоимость доставки";
                yield return new ValidationResult("Неверно указана стоимость доставки");
            }
            if (OrderPriceTo <= OrderPriceFrom)
            {
                Info = "Конечная цена должна быть больше начальной";
                yield return new ValidationResult("Конечная цена должна быть больше начальной");
            }
        }
    }

    public class CafeInfoModel
    {
        public const int kCafeIconMaxWidth = 48, kCafeIconMaxHeight = 48, kCafeIconMaxSize = 128 * 1024;
        public const int kLogotypeMaxWidth = 160, LogotypeMaxHeight = 120, kLogotypeMaxSize = 2 * 1024 * 1024;
        public const int kHeadPictureMaxWidth = 1920, kHeadPictureMaxHeight = 325, kHeadPictureMaxSize = 2 * 1024 * 1024;
        public const string kAllowedImageMimetypes = "image/jpeg, image/bmp, image/png";

        /// <summary>
        /// Тип обслуживания кафе (true - только корпоративное, false - все заказы)ю
        /// </summary>
        public bool CompanyOnly { get; set; }
        public CafeBusinessHoursModel BusinessHours { get; set; }
        public long CafeId { get; set; }
        [Display(Name = "Описание"), DataType(DataType.MultilineText)]
        [StringLength(1200, ErrorMessage = "Максимальное количество символов для описания - {1}")]
        public string Description { get; set; }
        [Display(Name = "Краткое описание")]
        [StringLength(256, ErrorMessage = "Максимальное количество символов для краткого описания - {1}")]
        public string ShortDescription { get; set; }
        public string Name { get; set; }
        [Display(Name = "Логотип")]
        public string Logotype { get; set; }
        [Display(Name = "Иконка")]
        public string CafeIcon { get; set; }
        [Display(Name = "Шапка кафе")]
        public string HeadPicture { get; set; }

        [ValidateImage(AllowedMimetypes = kAllowedImageMimetypes,
                       MaxWidth = kHeadPictureMaxWidth,
                       MaxHeight = kHeadPictureMaxHeight,
                       MaxSizeBytes = kHeadPictureMaxSize)]
        public IFormFile HeadPictureFile { get; set; }

        [ValidateImage(AllowedMimetypes = kAllowedImageMimetypes,
                       MaxWidth = kLogotypeMaxWidth,
                       MaxHeight = LogotypeMaxHeight,
                       MaxSizeBytes = kCafeIconMaxSize)]
        public IFormFile LogotypeFile { get; set; }

        [ValidateImage(AllowedMimetypes = kAllowedImageMimetypes,
                       MaxWidth = kCafeIconMaxWidth,
                       MaxHeight = kCafeIconMaxHeight,
                       MaxSizeBytes = kCafeIconMaxSize)]
        public IFormFile CafeIconFile { get; set; }

        [Display(Name = "Адрес кафе")]
        [StringLength(120, ErrorMessage = "Максимальное количество символов для адреса - {1}")]
        public string Address { get; set; }
        
        [Display(Name = "Телефон кафе")] 
        [StringLength(64, ErrorMessage = "Максимальное количество символов для телефона - {1}")]
        public string Phone { get; set; }

        [Display(Name = "Заказ еды на неделю")]
        public bool WeekMenuIsActive { get; set; }
        
        [Display(Name = "Отложенный заказ")]
        public bool DeferredOrder { get; set; }

        /// <summary>
        /// Стоимость доставки для персональных заказов
        /// </summary>
        [CustomValidation(typeof(CafeInfoModel),
            "ValidateCostOfDeliveryPersonal",
            ErrorMessage = "Стоимости не должны пересекаться")]
        public List<CafeCostOfDeliveryModel> CostOfDeliveryPersonal { get; set; }

        /// <summary>
        /// Стоимость доставки для корпоративных заказов
        /// </summary>
        [CustomValidation(typeof(CafeInfoModel),
            "ValidateCostOfDeliveryCompany",
            ErrorMessage = "Стоимости не должны пересекаться")]
        public List<CafeCostOfDeliveryModel> CostOfDeliveryCompany { get; set; }

        [Required]
        [Display(Name = "E-mail для уведомления")]
        [RegularExpression(@"^([a-zA-Z0-9_\.-]+)@([A-Za-z0-9_\.-]+)\.([A-Za-z\.]{2,6})$", 
            ErrorMessage = "Некорректный Email")]
        [StringLength(70, ErrorMessage = "Максимальное количество символов для электронного адреса - {1}")]
        public string NotificationContact { get; set; }

        [Display(Name = "Ежедневная минимальная общекорпоративная сумма заказа")]
        [CustomValidation(typeof(CafeInfoModel),
            "ValidateDailyCorpOrderSum",
            ErrorMessage = "Поле Ежедневная минимальная общекорпоративная сумма заказа должно иметь значение между 1 и 100000")]
        public Double? DailyCorpOrderSum { get; set; }

        [Display(Name = "Время отмены заказов по адресу если не набрана минимальная сумма")]
        public TimeSpan? OrderAbortTime { get; set; }

        /// <summary>
        /// Способ оплаты и его доступность для текущего кафе
        /// </summary>
        [Display(Name = "Способы оплаты")]
        public Dictionary<PaymentTypeEnum, bool> PaymentTypes { get; set; }

        /// <summary>
        /// Среднее время доставки
        /// </summary>
        [Display(Name = "Среднее время доставки в минутах")]
        [RegularExpression(@"\d*", ErrorMessage = "Недопустимое значение")]
        public int? AverageDeliveryTime { get; set; }

        /// <summary>
        /// Минимальная сумма заказа в рублях
        /// </summary>
        [Display(Name = "Минимальная сумма заказа в рублях")]
        [RegularExpression(@"^[0-9]*[.]?[0-9]+$", ErrorMessage = "Недопустимое значение")]
        public double? MinimumOrderSum { get; set; }

        /// <summary>
        /// Условия доставки
        /// </summary>
        [Display(Name = "Условия доставки")]
        [StringLength(1024, ErrorMessage = "Максимальная длина - {1}")]
        public string DeliveryComment { get; set; }

        /// <summary>
        /// Регионы доставки
        /// </summary>
        [Display(Name = "Регионы доставки")]
        [StringLength(256, ErrorMessage = "Максимальная длина - {1}")]
        public string Regions { get; set; }

        /// <summary>
        /// Кухня
        /// </summary>
        [Display(Name = "Кухни")]
        public List<KitchenModel> Kitchens { get; set; }

        [Display(Name = "Кухни")]
        public List<SelectableKitchenModel> SelectableKitchens { get; set; }

        public static ValidationResult ValidateCostOfDeliveryPersonal(object value, ValidationContext validationContext)
        {
            var model = (CafeInfoModel)validationContext.ObjectInstance;
            var listCostOfDelivery = model.CostOfDeliveryPersonal;
            if (listCostOfDelivery != null)
            {
                listCostOfDelivery = model.CostOfDeliveryPersonal.OrderBy(o => o.OrderPriceFrom).ToList();
                for (int i = 1; i < listCostOfDelivery.Count; ++i)
                {
                    if (listCostOfDelivery[i - 1].OrderPriceTo >= listCostOfDelivery[i].OrderPriceFrom)
                        return new ValidationResult("Стоимости не должны пересекаться");
                }
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateCostOfDeliveryCompany(object value, ValidationContext validationContext)
        {
            var model = (CafeInfoModel)validationContext.ObjectInstance;
            var listCostOfDelivery = model.CostOfDeliveryCompany;
            if (listCostOfDelivery != null)
            {
                listCostOfDelivery = model.CostOfDeliveryCompany.OrderBy(o => o.OrderPriceFrom).ToList();
                for (int i = 1; i < listCostOfDelivery.Count; ++i)
                {
                    if (listCostOfDelivery[i - 1].OrderPriceTo >= listCostOfDelivery[i].OrderPriceFrom)
                        return new ValidationResult("Стоимости не должны пересекаться");
                }
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateDailyCorpOrderSum(object value, ValidationContext validationContext)
        {
            var model = (CafeInfoModel)validationContext.ObjectInstance;
            var dailyCorpOrderSum = model.DailyCorpOrderSum;
            if (dailyCorpOrderSum == null ||
                dailyCorpOrderSum < 1.00 ||
                dailyCorpOrderSum > 100000.00)
            {
                return new ValidationResult("Поле Ежедневная минимальная общекорпоративная сумма заказа должно иметь значение между 1 и 100000");
            }
            return ValidationResult.Success;
        }
    }
}
