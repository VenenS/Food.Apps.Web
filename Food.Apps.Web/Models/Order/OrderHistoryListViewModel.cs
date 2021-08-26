using System.Collections.Generic;

namespace ITWebNet.Food.Site.Models
{
    public class OrderHistoryListViewModel
    {
        public List<OrderHistoryViewModel> OrderHistoryList { get; set; }

        public MessageViewModel Message { get; set; }
    }
}
