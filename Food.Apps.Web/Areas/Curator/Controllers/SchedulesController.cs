using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.Food.Site.Areas.Curator.Models;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Areas.Curator.Controllers
{
    public class SchedulesController : BaseController
    {
        private ICompanyOrderService companyOrderServiceClient;
        private ICompanyService companyService;
        private ICafeService cafeServiceClient;

        public SchedulesController(ICompanyOrderService companyOrderServiceClient, ICompanyService companyService, ICafeService cafeServiceClient)
        {
            this.companyOrderServiceClient = companyOrderServiceClient;
            this.companyService = companyService;
            this.cafeServiceClient = cafeServiceClient;
        }

        // GET: Curator/Schedules
        public async Task<ActionResult> Index()
        {
            List<ScheduleViewModel> listOfSchedules = new List<ScheduleViewModel>();
            var company = await companyService.GetCuratedCompany();
            if (company == null)
            {
                ViewBag.Error = true;
            }
            else
            {
                var schedules = await companyOrderServiceClient.GetListOfCompanyOrderScheduleByCompanyId(company.Id);
                foreach (var schedule in schedules)
                {
                    ScheduleViewModel model = new ScheduleViewModel();
                    model.Schedule = schedule;
                    model.Cafe = await cafeServiceClient.GetCafeById(schedule.CafeId);
                    model.Company = company;

                    model.Schedule.CompanyId = model.Company.Id;
                    model.Schedule.CompanyDeliveryAdress = model.Company.DeliveryAddressId ?? 0;
                    model.Schedule.CreateDate = DateTime.Now;

                    listOfSchedules.Add(model);
                }
            }
            return View(listOfSchedules);
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            ScheduleCreateModel model = new ScheduleCreateModel();
            model.Company = await companyService.GetCuratedCompany();
            if (model.Company == null)
            {
                return RedirectToActionPermanent("Index");
            }
            
            var cafes = await cafeServiceClient.GetCafesForSchedules();
            model.Cafes = GetSelectListItems(cafes);
            model.Schedule = new CompanyOrderScheduleModel();
            model.Schedule.CompanyId = model.Company.Id;
            model.Schedule.CompanyDeliveryAdress = model.Company.DeliveryAddressId ?? 0;

            return View("_Create", model);
        }

        [HttpPost]
        public async Task<ActionResult> Create(ScheduleCreateModel model)
        {
            var cafes = await cafeServiceClient.GetCafesForSchedules();
            model.Cafes = GetSelectListItems(cafes);

            if (!ModelState.IsValid)
            {
                return View("_Create", model);
            }

            List<CompanyOrderScheduleModel> listOfSchedules = await companyOrderServiceClient.GetListOfCompanyOrderScheduleByCompanyId(model.Schedule.CompanyId);
            foreach (var item in listOfSchedules.Where(m => m.CafeId == model.Schedule.CafeId))
                if (item.BeginDate != null && model.Schedule.EndDate != null && item.EndDate != null &&
                    model.Schedule.BeginDate != null &&
                    (model.Schedule.BeginDate.Value.Date >= item.EndDate.Value.Date ||
                     model.Schedule.EndDate.Value.Date <= item.BeginDate.Value.Date) || item.BeginDate == null ||
                    model.Schedule.BeginDate == null || item.EndDate == null || model.Schedule.EndDate == null)
                {
                    model.ValidationError = null;
                }
                else
                {
                    model.ValidationError = "Ошибка валидации! Расписание пересекается с уже существующим";
                    return View("_Create", model);
                }

            await companyOrderServiceClient.AddNewCompanyOrderSchedule(model.Schedule);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<ActionResult> Edit(long id)
        {
            ScheduleEditModel model = new ScheduleEditModel();
            var schedule = await companyOrderServiceClient.GetCompanyOrderScheduleById(id);
            model.Schedule = schedule;

            return View("_Edit", model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(ScheduleEditModel model)
        {
            model.Schedule.LastUpdDate = DateTime.Now;

            if (!ModelState.IsValid)
            {
                return View("_Edit", model);
            }

            List<CompanyOrderScheduleModel> listOfSchedules = await companyOrderServiceClient.GetListOfCompanyOrderScheduleByCompanyId(model.Schedule.CompanyId);
            foreach (var item in listOfSchedules.Where(m => m.CafeId == model.Schedule.CafeId))
            {
                if (item.Id != model.Schedule.Id)
                {
                    if (item.EndDate != null && model.Schedule.BeginDate != null && item.BeginDate != null &&
                        model.Schedule.EndDate != null &&
                        (model.Schedule.BeginDate.Value.Date >= item.EndDate.Value.Date ||
                         model.Schedule.EndDate.Value.Date <= item.BeginDate.Value.Date) || item.BeginDate == null ||
                        model.Schedule.BeginDate == null || item.EndDate == null || model.Schedule.EndDate == null)
                    {
                        model.ValidationError = null;
                    }
                    else
                    {
                        model.ValidationError = "Ошибка валидации! Расписание пересекается с уже существующим";
                        return View("_Edit", model);
                    }
                }
            }

            await companyOrderServiceClient.EditCompanyOrderSchedule(model.Schedule);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Delete(long id)
        {
            var schedule = await companyOrderServiceClient.GetCompanyOrderScheduleById(id);

            return View("_Delete", schedule);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteSchedule(long id)
        {
            try
            {
                await companyOrderServiceClient.RemoveCompanyOrderSchedule(id);

                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        private IEnumerable<SelectListItem> GetSelectListItems(IEnumerable<CafeModel> elements)
        {
            var selectList = new List<SelectListItem>();

            foreach (var element in elements)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element.Id.ToString(),
                    Text = element.Name
                });
            }

            return selectList;
        }


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var token = User.Identity.GetJwtToken();
            ((CompanyOrderService)companyOrderServiceClient)?.AddAuthorization(token);
            ((CompanyService)companyService)?.AddAuthorization(token);
            ((CafeService)cafeServiceClient)?.AddAuthorization(token);
        }
    }
}