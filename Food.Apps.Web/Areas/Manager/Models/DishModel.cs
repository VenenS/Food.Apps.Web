using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Food.Apps.Web.Attributes;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.AspNetCore.Http;

namespace ITWebNet.Food.Site.Areas.Manager.Models
{
    public class DishModel
    {
        /// <summary>
        /// Макс. размер изображения в байтах.
        /// </summary>
        public const int kMaxImageSize = 100 * 1024;

        /// <summary>
        /// Макс. ширина изображения в пикселях.
        /// </summary>
        public const int kMaxImageWidth = 1024;

        /// <summary>
        /// Макс. высота изображения в пикселях.
        /// </summary>
        public const int kMaxImageHeight = 1024;

        private readonly Core.DataContracts.Common.FoodDishModel _origin;
        private bool _isDaily { get; set; }
        private bool _isimply { get; set; }

        public long Id
        {
            get { return _origin.Id; }
            set { _origin.Id = value; }
        }

        [Display(Name = "Название")]
        [StringLength(70, MinimumLength = 2, ErrorMessage = "Название блюда может содержать от {2} до {1} символов")]
        [Required(ErrorMessage = "Введите название блюда")]
        [RegularExpression("^((?!:\\/\\\\)(?!.*(\\.){4,})(?!.*(\"){2,})(?!.*(\'){2,})[а-яА-ЯёЁa-zA-Z0-9,-;\'\" ])+$", 
            ErrorMessage = "В названии блюда могу присутствовать только буквы, цифры, символы .,-;\"\'...")]
        public string Name
        {
            get { return _origin.Name; }
            set { _origin.Name = value; }
        }

        [Display(Name = "Калорийность")]
        [Range(1, 999999, ErrorMessage = "Калорийность не может быть отрицательным числом, либо нулем, либо больше {2}")]
        public double? Kcalories
        {
            get { return _origin.Kcalories; }
            set { _origin.Kcalories = value; }
        }

        [Display(Name = "Вес")]
        [Range(1, 9999999, ErrorMessage = "Вес не может быть отрицательным значением, либо больше {2}")]
        public double? Weight
        {
            get { return _origin.Weight; }
            set { _origin.Weight = value; }
        }

        [Display(Name = "Описание веса")]
        [StringLength(100, ErrorMessage = "Максимальное количество символов - {1}")]
        [RegularExpression(@"^((?<!:\/\\)[а-яА-ЯёЁa-zA-Z0-9.,-; ])+$", 
            ErrorMessage = "В описании веса могу присутствовать только буквы, цифры, символы .,;")]
        public string WeightDescription
        {
            get { return _origin.WeightDescription; }
            set { _origin.WeightDescription = value; }
        }

        [Display(Name = "Цена")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        [Required(ErrorMessage = "Введите цену блюда")]
        [Range(0.01, 999999, ErrorMessage = "Цена не может быть отрицательным числом, либо нулем, либо больше {2}")]
        [RegularExpression(@"[0-9]*[.,]?[0-9]{0,2}", ErrorMessage = "Цена указана некорректно")]
        public double BasePrice
        {
            get { return _origin.BasePrice; }
            set { _origin.BasePrice = value; }
        }

        [Display(Name = "Описание"), DataType(DataType.MultilineText)]
        [StringLength(500, ErrorMessage = "Максимальное количество символов - {1}")]
        public string Description
        {
            get { return _origin.Description; }
            set { _origin.Description = value; }
        }

        [Display(Name = "Состав")]
        [StringLength(300, ErrorMessage = "Максимальное количество символов - {1}")]
        public string Composition
        {
            get { return _origin.Composition; }
            set { _origin.Composition = value; }
        }

        public int? DishIndex
        {
            get { return _origin.DishIndex; }
            set { _origin.DishIndex = value; }
        }

        [Display(Name = "Начало")]
        public DateTime? VersionFrom
        {
            get { return _origin.VersionFrom; }
            set { _origin.VersionFrom = value; }
        }

        [Display(Name = "Окончание")]
        public DateTime? VersionTo
        {
            get { return _origin.VersionTo; }
            set { _origin.VersionTo = value; }
        }

        public Guid Uuid
        {
            get { return _origin.Uuid; }
            set { _origin.Uuid = value; }
        }

        public long CategoryId
        {
            get { return _origin.CategoryId; }
            set { _origin.CategoryId = value; }
        }

        public long DishRatingSumm
        {
            get { return _origin.DishRatingSumm; }
            set { _origin.DishRatingSumm = value; }
        }

        public long DishRatingCount
        {
            get { return _origin.DishRatingCount; }
            set { _origin.DishRatingCount = value; }
        }

        public string Image
        {
            get { return _origin.Image; }
            set { _origin.Image = value; }
        }

        public bool ImageCleared { get; set; }

        [ValidateImage(AllowedMimetypes = "image/jpeg, image/bmp, image/png",
                       MaxHeight = kMaxImageHeight,
                       MaxWidth = kMaxImageWidth,
                       MaxSizeBytes = kMaxImageSize)]
        public IFormFile ImageFile { get; set; }

        [Display(Name = "Включено в меню на каждый день")]
        public bool IsDaily
        {
            get { return _isDaily; }
            set { _isDaily = value; }
        }

        public bool IsSimply
        {
            get { return _isimply; }
            set { _isimply = value; }
        }

        public List<ScheduleModel> Schedules
        {
            get { return _origin.Schedules ?? new List<ScheduleModel>(); }
            set { _origin.Schedules = value; }
        }

        public List<SelectableFoodCategoryModel> FoodCategories
        {
            get => _origin.FoodCategories;
            set => _origin.FoodCategories = value;
        }

        public DishModel(Core.DataContracts.Common.FoodDishModel origin)
        {
            _origin = origin;
            _isDaily = _origin.Schedules != null && _origin.Schedules.Any(s => s.Type == "D");
            _isimply = _origin.Schedules != null && _origin.Schedules.Any(s => s.Type == "S");
        }

        public DishModel()
            : this(new Core.DataContracts.Common.FoodDishModel())
        {
        }

        public Core.DataContracts.Common.FoodDishModel GetOrigin()
        {
            return _origin;
        }

        public static List<DishModel> AsModelList(List<Core.DataContracts.Common.FoodDishModel> source)
        {
            return source.Select(item => new DishModel(item)).OrderBy(c => c.DishIndex).ThenBy(c => c.Id).ToList();
        }

        public static List<Core.DataContracts.Common.FoodDishModel> AsSourceList(List<DishModel> model)
        {
            return model.Select(item => item._origin).ToList();
        }
    }
}