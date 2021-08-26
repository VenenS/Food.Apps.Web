using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.AuthorizationService;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Ploeh.AutoFixture;

namespace Kpi.Apps.Web.Tests.Tools
{
    public class BaseSetUp
    {
        public static readonly UserModel User = FakeFactory.Fixture.Create<UserModel>();
        public static readonly ControllerContext ControllerContext = ControllerContextHelper.Setup(User.Id, User.Email);

        public ServiceManager GetManager()
        {
            var manager = new ServiceManager();
            manager.AccountSevice = AccountSevice.Object;
            manager.ProfileClient = ProfileClient.Object;
            manager.AddressServiceClient = AddressServiceClient.Object;
            manager.BanketsService = BanketsService.Object;
            manager.CafeService = CafeService.Object;
            manager.CompanyService = CompanyService.Object;
            manager.DishCategoryService = DishCategoryService.Object;
            manager.DishService = DishService.Object;
            manager.CompanyOrderServiceClient = CompanyOrderServiceClient.Object;
            manager.OrderItemServiceClient = OrderItemServiceClient.Object;
            manager.OrderServiceClient = OrderServiceClient.Object;
            manager.RatingServiceClient = RatingServiceClient.Object;
            manager.TagServiceClient = TagServiceClient.Object;
            manager.UserServiceClient = UserServiceClient.Object;
            manager.MenuServiceClient = MenuServiceClient.Object;
            return manager;
        }

        public Mock<AccountService> AccountSevice { get; set; }
        public Mock<ProfileService> ProfileClient { get; set; }
        public Mock<AddressesService> AddressServiceClient { get; set; }
        public Mock<BanketsService> BanketsService { get; set; }
        public Mock<CafeService> CafeService { get; set; }
        public Mock<CompanyService> CompanyService { get; set; }
        public Mock<DishCategoryService> DishCategoryService { get; set; }
        public Mock<DishService> DishService { get; set; }
        public Mock<CompanyOrderService> CompanyOrderServiceClient { get; set; }
        public Mock<OrderItemService> OrderItemServiceClient { get; set; }
        public Mock<OrderService> OrderServiceClient { get; set; }
        public Mock<RatingService> RatingServiceClient { get; set; }
        public Mock<TagService> TagServiceClient { get; set; }
        public Mock<UsersService> UserServiceClient { get; set; }
        public Mock<MenuService> MenuServiceClient { get; set; }

        public BaseSetUp()
        {
            AccountSevice = new Mock<AccountService>(MockBehavior.Strict, true);
            ProfileClient = new Mock<ProfileService>(MockBehavior.Strict, true);
            AddressServiceClient = new Mock<AddressesService>(MockBehavior.Strict, true);
            BanketsService = new Mock<BanketsService>(MockBehavior.Strict, true);
            CafeService = new Mock<CafeService>(MockBehavior.Strict, true);
            CompanyService = new Mock<CompanyService>(MockBehavior.Strict, true);
            DishCategoryService = new Mock<DishCategoryService>(MockBehavior.Strict, true);
            DishService = new Mock<DishService>(MockBehavior.Strict, true);
            CompanyOrderServiceClient = new Mock<CompanyOrderService>(MockBehavior.Strict, true);
            OrderItemServiceClient = new Mock<OrderItemService>(MockBehavior.Strict, true);
            OrderServiceClient = new Mock<OrderService>(MockBehavior.Strict, true);
            RatingServiceClient = new Mock<RatingService>(MockBehavior.Strict, true);
            TagServiceClient = new Mock<TagService>(MockBehavior.Strict, true);
            UserServiceClient = new Mock<UsersService>(MockBehavior.Strict, true);
            MenuServiceClient = new Mock<MenuService>(MockBehavior.Strict, true);
        }
    }
}
