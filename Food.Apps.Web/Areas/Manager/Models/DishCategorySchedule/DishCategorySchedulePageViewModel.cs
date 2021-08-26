using ITWebNet.Food.Site.Models;

namespace ITWebNet.Food.Site.Areas.Manager.Models.DishCategorySchedule
{
    public class DishCategorySchedulePageViewModel
    {
        public long CafeId { get; set; }
        public long CategoryId { get; set; }
        public CafeBusinessHoursModel BusinessHours { get; set; }
        public MessageViewModel Message { get; set; }
    }
}