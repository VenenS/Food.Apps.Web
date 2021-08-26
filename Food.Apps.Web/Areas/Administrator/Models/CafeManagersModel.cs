using System.Collections.Generic;
using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Site.Areas.Administrator.Models
{
    public class CafeManagerViewModel
    {
        public CafeModel Cafe { get; set; }
        public IEnumerable<CafeManagerModel> Managers { get; set; }
    }
}