using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;


namespace ITWebNet.Food.Site.Areas.Administrator.Models
{
    public class CompanysLists
    {
        public List<CompanyModel> ActiveCompanys { get; set; }
        public List<CompanyModel> InactiveCompanys { get; set; }
    }
}