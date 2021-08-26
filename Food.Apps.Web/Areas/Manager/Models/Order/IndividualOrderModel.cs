using System.Collections.Generic;
using ITWebNet.Food.Site.Models;

namespace ITWebNet.Food.Site.Areas.Manager.Models
{
    public class IndividualOrderModel : OrderModelBase
    {
        public List<OrderDish> Dishes { get; set; }
        public IndividualOrderState State { get; set; }

        public IndividualOrderModel()
        {
            Type = EnumOrderType.Individual;
        }
    }
}
