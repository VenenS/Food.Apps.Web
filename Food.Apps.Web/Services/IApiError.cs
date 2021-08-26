using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface IApiError
    {
        Dictionary<string, string> GetErrorList();
    }
}
