using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using Newtonsoft.Json;

namespace ITWebNet.Food.Site.Areas.Manager.Models
{
    public class MenuModel
    {
        public MenuFilters Filter { get; set; }
        public Dictionary<FoodCategoryModel, List<Core.DataContracts.Common.FoodDishModel>> AvailableDishes { get; set; }
        public Dictionary<FoodCategoryModel, List<Core.DataContracts.Common.FoodDishModel>> Schedule { get; set; }
        public MenuPatternModel Patterns { get; set; }
    }

    public class FoodDishModel : Core.DataContracts.Common.FoodDishModel
    {
        public ActionType ActionType { get; set; }
        [JsonProperty]
        public bool IsPreview { get; set; }
        [JsonProperty]
        public DateTime ToDate { get; set; }
    }

    public class MenuFilters
    {
        [DataType(DataType.Date)]
        public DateTime? OneDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Start { get; set; }

        [DataType(DataType.Date)]
        public DateTime? End { get; set; }

        public string KeyWords { get; set; }
    }

    public class MenuPatternModel
    {
        [Required]
        [Display(Name = "Шаблон")]
        public long PatternId { get; set; }
        public List<CafeMenuPatternModel> Patterns { get; set; }

        public MenuPatternModel()
        {
            Patterns = new List<CafeMenuPatternModel>();
        }

        [Display(Name = "Название")]
        [StringLength(128, ErrorMessage = "Максимальное кол-во символов для названия шаблона - {1}")]
        public string PatternName { get; set; }

        [Display(Name = "Банкетное меню")]
        public bool IsBanket { get; set; }

        public bool IsPatternSelected
        {
            get { return Patterns != null && PatternId != 0 && Patterns.Select(c => c.Id).Contains(PatternId); } 
        }
    }

    public class SaveScheduleModel
    {
        public DateTime? Date { get; set; }
        public List<FoodDishModel> Dishes { get; set; }
    }

    public class ScheduleHistoryItem
    {
        public Core.DataContracts.Common.FoodDishModel Dish { get; set; }
        public List<DishInMenuHistoryModel> Schedules { get; set; }
    }

    public enum ActionType
    {
        Add = 0,
        Delete = 1,
        Edit = 2
    }
}