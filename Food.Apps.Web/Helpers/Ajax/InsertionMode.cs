using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace FoodApps.Web.NetCore.Helpers
{
    public enum InsertionMode
    {
        [DisplayName("replace")]
        Replace = 0,
        
        [DisplayName("before")]
        InsertBefore = 1,

        [DisplayName("after")]
        InsertAfter = 2,

        [DisplayName("replace-with")]
        ReplaceWith = 3
    }
}
