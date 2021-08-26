using ITWebNet.Food.Core.DataContracts.Manager;

namespace ITWebNet.Food.Site.Areas.Manager.Models
{
    public class SelectableKitchenModel
    {
        public KitchenModel Kitchen { get; set; }

        public bool IsSelected { get; set; }
    }
}
