using System.Linq;
using System.Threading.Tasks;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Services;
using System;
using ITWebNet.Food.Site.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Food.Apps.Web.Areas.Curator.Models.CompanyOrders;
using Microsoft.Extensions.DependencyInjection;

namespace ITWebNet.Food.Site.Areas.Curator.Controllers
{
    public class CompanyOrdersController : BaseController
    {
        private ICompanyOrderService CompanyOrderClient { get; set; }
        private IOrderItemService OrderItemClient { get; set; }
        private IOrderService OrderServiceClient { get; set; }

        public CompanyOrdersController()
        {

        }

        [ActivatorUtilitiesConstructor]
        public CompanyOrdersController(ICompanyOrderService companyOrderClient, IOrderItemService orderItemClient, IOrderService orderServiceClient)
        {
            CompanyOrderClient = companyOrderClient;
            OrderItemClient = orderItemClient;
            OrderServiceClient = orderServiceClient;
        }

        // GET: Administrator/CompanyOrders
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetCompanyOrders()
        {
            DateTime date = DateTime.Now;
            var data = await CompanyOrderClient.GetCompanyOrderByDate(date);
            var jsonObj = Json(data);
            return jsonObj;
        }

        [HttpGet]
        public async Task<ActionResult> Edit(long id)
        {
            var order = await CompanyOrderClient.GetCompanyOrder(id);
            if (order == null)
                return NotFound($"Заказ №{id} не найден");

            var vm = CreateEditCompanyOrderVm(order, saveSuccess: TempData["EditSuccess"] != null);
            return View(vm);
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Edit(int id, EditCompanyOrderInputModel model)
        {
            var order = await CompanyOrderClient.GetCompanyOrder(id);
            if (order == null)
                return NotFound();

            // Поверхностная валидация времени заказа. Более продвинутые и базовые кейсы типа
            // юзер не имеет прав на редактирование данного заказа или дата заказа
            // изменена в связи с TimeSpan'ом > 24 ч. должна производится на стороне апи.
            //
            // FIXME: в апи отсутствует валидация.
            if (model.OrderStartTime >= model.OrderEndTime) {
                ModelState.AddModelError(nameof(model.OrderEndTime), "Неправильный диапазон заказа");
            } else if (model.OrderDeliveryTime < model.OrderEndTime) {
                ModelState.AddModelError(
                    nameof(model.OrderDeliveryTime),
                    "Время доставки должно быть позднее времени закрытия"
                );
            }

            var vm = CreateEditCompanyOrderVm(order, model);

            if (!ModelState.IsValid)
                return View(vm);

            // Редактирование и сохранение заказа.
            order.OrderOpenDate = order.OrderOpenDate.Value.Date + model.OrderStartTime;
            order.OrderAutoCloseDate = order.OrderAutoCloseDate.Value.Date + model.OrderEndTime;
            order.DeliveryDate = order.DeliveryDate.Value.Date + model.OrderDeliveryTime;

            var result = await CompanyOrderClient.EditCompanyOrder(order);
            if (result < 0) {
                ModelState.AddModelError("", "Не удалось отредактировать корпоративный заказ");
                return View(vm);
            }

            TempData["EditSuccess"] = true;
            return Redirect(Url.Action(nameof(Edit)));
        }

        [AjaxOnly]
        public async Task<ActionResult> DeleteOrderItem(long cafeId, long orderItemId, long orderId, bool isDecrement = false)
        {
            var model = await OrderServiceClient.GetOrder(orderId);
            if (model == null)
                return BadRequest();

            if (model.CafeId != cafeId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var response = await DecrementOrderItem(orderItemId, model, isDecrement);
                if (!response)
                    return BadRequest();
                //если в заказе не осталось позиций, то отменяем его
                if (model.itemsCount.HasValue && model.itemsCount == 0)
                {
                    OrderModel abortedOrder = new OrderModel
                    {
                        Id = orderId,
                        Status = (long)OrderStatusEnum.Abort
                    };
                    await OrderServiceClient.ChangeOrder(abortedOrder);
                }
                model = await OrderServiceClient.GetOrder(orderId);
            }
            ViewData["order"] = model;
            return PartialView("_OrderItem", model.OrderItems.FirstOrDefault(c => c.Id == orderItemId));
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            string token = User.Identity.GetJwtToken();

            ((CompanyOrderService)CompanyOrderClient)?.AddAuthorization(token);
            ((OrderItemService)OrderItemClient)?.AddAuthorization(token);
            ((OrderService)OrderServiceClient)?.AddAuthorization(token);
        }

        private async Task<bool> DecrementOrderItem(long orderItemId, OrderModel model, bool isDecrement)
        {
            if (isDecrement)
            {
                var item = model.OrderItems.First(i => i.Id == orderItemId);
                item.DishCount--;
                model.itemsCount--;
                if (item.DishCount > 0)
                {
                    var result = await OrderItemClient.ChangeOrderItem(item);
                    return result.ExceptionList == null || result.ExceptionList.Count == 0;
                }
            }
            return await OrderItemClient.RemoveOrderItem(orderItemId);
        }

        /// <summary>
        /// Создает вью-модель для редактирования корп. заказа.
        /// </summary>
        /// <param name="model">Модель корп. заказа</param>
        /// <param name="postedData">Инпут модель полученная от юзера, если была произведена
        /// попытка редактирования</param>
        /// <param name="saveSuccess">Было ли редактирование успешным</param>
        private EditCompanyOrderViewModel CreateEditCompanyOrderVm(
            CompanyOrderModel model,
            EditCompanyOrderInputModel postedData = null,
            bool saveSuccess = false)
        {
            EditCompanyOrderInputModel inputModel;
            if (postedData == null) {
                inputModel = new EditCompanyOrderInputModel {
                    OrderStartTime = model.OrderOpenDate.Value - model.OrderOpenDate.Value.Date,
                    OrderDeliveryTime = model.DeliveryDate.Value - model.DeliveryDate.Value.Date,
                    OrderEndTime = model.OrderAutoCloseDate.Value - model.OrderAutoCloseDate.Value.Date,
                };
            } else {
                inputModel = postedData;
            }

            return new EditCompanyOrderViewModel {
                Id = model.Id,
                CafeName = model.CafeFullName,
                OrderDate = model.OrderOpenDate.Value.Date,
                AfterPost = saveSuccess || postedData != null,
                OrderDeliveryTime = inputModel.OrderDeliveryTime,
                OrderEndTime = inputModel.OrderEndTime,
                OrderStartTime = inputModel.OrderStartTime
            };
        }
    }
}