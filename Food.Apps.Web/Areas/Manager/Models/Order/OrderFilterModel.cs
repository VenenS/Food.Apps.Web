using System;

namespace ITWebNet.Food.Site.Areas.Manager.Models
{
    public class OrderFilterModel
    {
        public long CafeId { get; set; }
        public DateTime SearchDate { get; set; }
        public string SearchString { get; set; }
        public bool ShowCollectiveOrders { get; set; }
        public bool ShowIndividualOrders { get; set; }
        public OrderSortingMode SortingMode { get; set; }

        public OrderFilterModel()
        {
            SearchDate = DateTime.Today;
            ShowCollectiveOrders = true;
            ShowIndividualOrders = true;
        }
    }
}
