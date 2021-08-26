using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ITWebNet.Food.Site.Areas.Administrator.Models
{
    public class CompanyOrderShortModel
    {
        public long Id { get; set; }

        [Display(Name ="Компания")]
        public string CompanyName { get; set; }

        [Display(Name ="Кафе")]
        public string CafeName { get; set; }

        [Display(Name ="Время открытия заказа")]
        public TimeSpan? OrderOpenTime { get; set; }

        [Display(Name = "Время автозакрытия")]
        public TimeSpan? OrderAutoCloseTime { get; set; }

        [Display(Name = "Время доставки")]
        public TimeSpan? OrderDeliveryTime { get; set; }

        public TimeSpan? WorkingTimeFrom { get; set; }
        public TimeSpan? WorkingTimeTo { get; set; }
    }
}