using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.AuthorizationService;

namespace ITWebNet.Food.Site
{
    public class ServiceManager
    {
        public AccountService AccountSevice { get; set; }
        public AddressesService AddressServiceClient { get; set; }
        public BanketsService BanketsService { get; set; }
        public CafeService CafeService { get; set; }
        public CompanyOrderService CompanyOrderServiceClient { get; set; }
        public CompanyService CompanyService { get; set; }
        public DishCategoryService DishCategoryService { get; set; }
        public DishService DishService { get; set; }
        public MenuService MenuServiceClient { get; set; }
        public OrderItemService OrderItemServiceClient { get; set; }
        public OrderService OrderServiceClient { get; set; }
        public ProfileService ProfileClient { get; set; }
        public RatingService RatingServiceClient { get; set; }
        public TagService TagServiceClient { get; set; }
        public UsersService UserServiceClient { get; set; }
    }
}