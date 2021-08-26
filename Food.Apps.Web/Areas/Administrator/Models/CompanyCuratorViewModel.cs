using System.Collections.Generic;
using ITWebNet.Food.Core.DataContracts.Admin;
using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Site.Areas.Administrator.Models
{
    public class CompanyCuratorViewModel
    {
        public CompanyModel Company { get; set; }
        public List<CompanyCuratorModel> Curators { get; set; }
    }
}