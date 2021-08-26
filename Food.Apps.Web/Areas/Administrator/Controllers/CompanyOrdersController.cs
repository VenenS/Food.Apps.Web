using ITWebNet.Food.Site.Areas.Administrator.Models;
using ITWebNet.Food.Site.Attributes;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Areas.Administrator.Controllers
{
    public class CompanyOrdersController : BaseController
    {
        private ICompanyOrderService CompanyOrderService { get; set; }

        public CompanyOrdersController(ICompanyOrderService companyOrderService)
        {
            CompanyOrderService = companyOrderService;
        }

        public async Task<ActionResult> Index()
        {
            var model = (await CompanyOrderService.GetCompanyOrderByDate(DateTime.Now))
                .Select(o => new CompanyOrderShortModel
                {
                    Id = o.Id,
                    CompanyName = o.Company.Name,
                    CafeName = o.CafeFullName,
                    OrderOpenTime = o.OrderOpenDate?.TimeOfDay,
                    OrderAutoCloseTime = o.OrderAutoCloseDate?.TimeOfDay,
                    OrderDeliveryTime = o.DeliveryDate?.TimeOfDay,
                    WorkingTimeFrom = o.Cafe.WorkingTimeFrom?.TimeOfDay,
                    WorkingTimeTo = o.Cafe.WorkingTimeTo?.TimeOfDay
                });
            ViewBag.ResultMessage = TempData["ResultMessage"];
            ViewBag.ResultMessageType = TempData["ResultMessageType"];
            return View(model);
        }

        /// <summary>
        /// Изменяет корпоративный заказ.
        /// </summary>
        /// <param name="order">Модель заказа</param>
        /// <returns>
        /// Если время открытия больше или равно времени закрытия, возвращает частичное представление с ошибками.
        /// Если в модели остуствуют ошибки, возвращает Json, с указанием обновить страницу
        /// </returns>
        [AjaxOnly]
        [HttpPost]
        public async Task<ActionResult> EditOrder(CompanyOrderShortModel order)
        {
            if (order.OrderOpenTime >= order.OrderAutoCloseTime)
            {
                ModelState.AddModelError("OrderOpenTime", "Время открытия заказа должно быть меньше времени закрытия.");
            }
            if (order.OrderAutoCloseTime >= order.OrderDeliveryTime)
            {
                ModelState.AddModelError("OrderDeliveryTime", "Время доставки заказа должно быть больше времени закрытия.");
            }
            if (!ModelState.IsValid)
            {
                return PartialView("_CompanyOrderItem", order);
            }
            var oldOrder = await CompanyOrderService.GetCompanyOrder(order.Id);
            oldOrder.OrderOpenDate = DateTime.Parse(order.OrderOpenTime.ToString());
            oldOrder.OrderAutoCloseDate = DateTime.Parse(order.OrderAutoCloseTime.ToString());
            oldOrder.DeliveryDate = DateTime.Parse(order.OrderDeliveryTime.ToString());
            var result = await CompanyOrderService.EditCompanyOrder(oldOrder);
            TempData["ResultMessage"] = result != -1 ? "Заказ отредактирован." : "Произошла ошибка. Заказ не отредактирован.";
            TempData["ResultMessageType"] = result != -1 ? "success" : "danger";
            TempData.Keep();
            return Json(new { result = "Redirect", url = Url.Action("Index", "CompanyOrders") });
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string token = User.Identity.GetJwtToken();
            ((CompanyOrderService)CompanyOrderService)?.AddAuthorization(token);
        }
    }
}