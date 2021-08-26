using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Controllers;
using ITWebNet.Food.Site.Models;
using Kpi.Apps.Web.Tests.Tools;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderModel = ITWebNet.Food.Core.DataContracts.Common.OrderModel;

namespace Food.Apps.Web.Tests.Controller
{
    [TestFixture]
    public class OrderControllerTests
    {
        /*
        [Test]
        public async Task DeleteOrderItemTest()
        {
            var setUp = new SetUpClass();
            var orderId = setUp.Rnd.Next();
            var orderItemId = setUp.Rnd.Next();
            var orderModel = FakeFactory.Fixture.Create<OrderModel>();
            var companyOrderModel = FakeFactory.Fixture.Create<CompanyOrderModel>();
            setUp.Items.OrderServiceClient.Setup(e => e.GetOrder(orderId)).ReturnsAsync(orderModel);
            setUp.Items.CompanyOrderServiceClient.Setup(e => e.GetCompanyOrder(orderModel.CompanyOrderId.Value))
                .ReturnsAsync(companyOrderModel);
            var result = await setUp.Controller.DeleteOrderItem(orderId, orderItemId);
            Assert.Pass("Your first passing test");
        }*/

        [Test]
        public async Task DeleteOrderItemTest_Empty_Result()
        {
            var setUp = new SetUpClass();
            setUp.Items.OrderServiceClient.Setup(e =>
                e.GetUserOrdersInDateRange(setUp.User.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync((List<OrderModel>)null);
            var response = await setUp.Controller.Index();
            var result = response as ViewResult;
            var model = result.Model as List<OrderHistoryViewModel>;
            Assert.IsTrue(!model.Any());
        }

        private class SetUpClass
        {
            public readonly OrdersController Controller;
            public readonly Random Rnd = new Random();
            public readonly BaseSetUp Items = new BaseSetUp();
            public readonly UserModel User;

            public SetUpClass()
            {
                Controller = new OrdersController(Items.GetManager(), FoodApps.Web.NetCore.Startup.Configuration);
                Controller.ControllerContext = BaseSetUp.ControllerContext;
                User = BaseSetUp.User;
            }
        }
    }
}
