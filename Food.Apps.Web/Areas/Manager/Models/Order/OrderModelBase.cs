using System;
using ITWebNet.Food.Site.Models;

namespace ITWebNet.Food.Site.Areas.Manager.Models
{
    public abstract class OrderModelBase
    {
        public DateTime CreationDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string Comment { get; set; }
        public EnumOrderType Type { get; protected set; }
    }
}
