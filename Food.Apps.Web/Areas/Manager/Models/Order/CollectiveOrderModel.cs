using System.Collections.Generic;
using ITWebNet.Food.Site.Models;

namespace ITWebNet.Food.Site.Areas.Manager.Models
{
    public class CollectiveOrderModel : OrderModelBase
    {
        public List<IndividualOrderModel> Orders { get; set; }
        public CollectiveOrderState State { get; set; }

        public CollectiveOrderModel()
        {
            Type = EnumOrderType.Collective;
        }
    }
}
