using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ITWebNet.Food.Site.Areas.Administrator.Controllers
{
    public class ReportsLayoutsController : BaseController
    {
        private IReportService reportServiceClient;

        public ReportsLayoutsController(IReportService reportServiceClient)
        {
            this.reportServiceClient = reportServiceClient;
        }

        // GET: Administrator/ReportsLayouts
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var token = User.Identity.GetJwtToken();
            ((ReportService)reportServiceClient)?.AddAuthorization(token);
        }
        public async Task<ActionResult> Index()
        {
            List<XSLTModel> model = await reportServiceClient.GetXsltList();
            ViewBag.ResultMessage = TempData["ResultMessage"];
            ViewBag.ResultMessageType = TempData["ResultMessageType"];
            return View(model);
        }

        public ActionResult AddLayout()
        {
            XSLTModel model = new XSLTModel();
            model.Id = -model.GetHashCode();
            return View("AddLayout", model);
        }

        public async Task<ActionResult> RemoveLayout(long layoutId)
        {
            XSLTModel model = await reportServiceClient.GetXsltById(layoutId);
            var response = await reportServiceClient.RemoveXsltAdm(model);
            TempData["ResultMessage"] = response ? "Шаблон удален." : "Произошла ошибка. Шаблон не удален.";
            TempData["ResultMessageType"] = response ? "success" : "danger";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<ActionResult> AddLayout(XSLTModel layout)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_Layout", layout);
            }
            layout.Id = 0;
            var isUnique = await reportServiceClient.СheckUniqueNameXslt(layout.Name);
            if (isUnique)
            {
                var responce = await reportServiceClient.AddXsltAdm(layout);
                if (responce > 0)
                {
                    TempData["ResultMessage"] = "Шаблон создан.";
                    TempData["ResultMessageType"] = "success";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                ModelState.AddModelError("Name", "Имя уже  занято");
            }
            return View("AddLayout", layout);
        }

        
        public async Task<ActionResult> EditLayout(XSLTModel layout)
        {

            XSLTModel tmpLayout = new XSLTModel();
            if (!ModelState.IsValid)
            {
                return PartialView("_Layout", layout);
            }
            
             var response = await reportServiceClient.EditXsltadm(layout);
            TempData["ResultMessage"] = response ? "Шаблон отредактирован." : "Произошла ошибка. Шаблон не отредактирован.";
            TempData["ResultMessageType"] = response ? "success" : "danger";
            TempData.Keep();
            return Json(new { result = "Redirect", url = Url.Action("Index", "ReportsLayouts") });
        }
        
        public async Task<ActionResult> LayoutToCafes(long layoutId)
        {
            List<LayoutToCafeModel> model = await reportServiceClient.GetCafesToLayout(layoutId);
            return View("LayoutToCafes", model);
        }
        public async Task<ActionResult> DeleteCafe(long id)
        {
            LayoutToCafeModel model = await reportServiceClient.DeleteCafeToXslt(id);
            return View("DeleteCafeToLayout", model);
        }
        public async Task<ActionResult> AddCafe(long id)
        {
            List<LayoutToCafeModel> model = await reportServiceClient.AddCafeToXslt(id);
            return View("AddCafeToLayout", model);
        }
        [HttpPost,AjaxOnly]
        public async Task<ActionResult> AddCafeConfirm(LayoutToCafeModel model)
        {
            var id = await reportServiceClient.AddCafeToXslt(model);
            if(id > 0)
            {
                List<LayoutToCafeModel> okModel = await reportServiceClient.GetCafesToLayout(model.Xslt.Id);
                return View("LayoutToCafes", okModel);
            }
            List<LayoutToCafeModel> returnModel = await reportServiceClient.AddCafeToXslt(id);
            return View("AddCafeToLayout", returnModel);
        }
        [HttpPost]
        public async Task<ActionResult> Delete(LayoutToCafeModel modelId)
        {
            await reportServiceClient.DeleteCafeToXsltConfirm(modelId);
            List<LayoutToCafeModel> okModel = await reportServiceClient.GetCafesToLayout(modelId.Xslt.Id);
            return View("LayoutToCafes", okModel);

        }
    }
}