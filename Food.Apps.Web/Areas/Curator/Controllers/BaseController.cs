

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITWebNet.Food.Site.Areas.Curator.Controllers
{
    [Area("Curator")]
    [Authorize(Roles = "Consolidator")]
    public class BaseController : Controller
    {
    }
}