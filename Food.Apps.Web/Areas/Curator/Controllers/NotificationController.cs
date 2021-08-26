using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ITWebNet.Food.Site.Areas.Curator.Controllers
{
    public class NotificationController : BaseController
    {

        private ICompanyService companyService;

        public NotificationController(ICompanyService companyService)
        {
            this.companyService = companyService;
        }

        // GET: Curator/Notification
        public async Task<ActionResult> Index()
        {
            return View(await companyService.GetCuratedCompany());
        }

        [HttpPost]
        public async Task<ActionResult> Index(CompanyModel model)
        {
            if (!await companyService.SetSmsNotify(model.Id, model.SmsNotify))
            {
                TempData["ErrorSave"] = "Ошибка сервера при сохранении настроек.";
                TempData["ErrorType"] = "danger";
            }                
            else
            {
                TempData["ErrorSave"] = "Сохранение прошло успешно!";
                TempData["ErrorType"] = "success";
                return Redirect(Url.Action(nameof(Index)));
            }

            return View("Index", model);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            companyService = new CompanyService();

            var token = User.Identity.GetJwtToken();
            ((CompanyService)companyService)?.AddAuthorization(token);
        }
    }
}