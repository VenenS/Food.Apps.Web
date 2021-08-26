using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Apps.Web.Areas.Curator.Models.CompanyOrders
{
    public class EditCompanyOrderInputModel
    {
        public TimeSpan OrderStartTime { get; set; }
        public TimeSpan OrderEndTime { get; set; }
        public TimeSpan OrderDeliveryTime { get; set; }
    }

    public class EditCompanyOrderViewModel : EditCompanyOrderInputModel
    {
        public long Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string CafeName { get; set; }
        public bool AfterPost { get; set; }
    }
}
