using Food.Services.Contracts;
using ITWebNet.Food.Core.DataContracts.Admin;
using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface IUsersService
    {
        Task<bool> AddUserReferralLink(long parentId, long referralId);
        Task<bool> AddUserToRole(long roleId, long userId);
        Task<bool> CheckUniqueEmail(string email, long userId = -1);
        Task<bool> EditUser(UserWithLoginModel model);
        Task<bool> EditUserPointsByLogin(string login, long typePoints, double points);
        Task<bool> EditUserPointsByLoginAndTotalPrice(string login, double totalPrice);
        Task<bool> EditUserRole(UserRoleModel model);
        Task<List<UserAdminModel>> GetAdminUsers();
        Task<List<UserWithLoginModel>> GetFullListOfUsers();
        Task<List<RoleModel>> GetListRoleToUser(long userId);
        Task<List<UserRoleModel>> GetListUserRole();
        Task<List<RoleModel>> GetRoles();
        Task<UserModel> GetUserByEmail(string email);
        Task<UserModel> GetUserByLogin(string login);
        Task<UserModel> GetUserByReferralLink(string referralLink);
        Task<UserWithLoginModel> GetUserByRoleId(long roleId);
        Task<double> GetUserPointsByLogin(string login);
        Task<List<UserReferralModel>> GetUserReferrals(long userId, int[] level);
        Task<List<UserModel>> GetUsers();
        Task<List<UserWithLoginModel>> GetUsersByCafeOrCompany(OrganizationTypeEnum organizationType, long id);
        Task<List<UserModel>> GetUsersWithoutCurators();
        Task<bool> RemoveUserRole(UserRoleModel model);
        Task SendFeedback(FeedbackModel feedback);
        Task SetUserSmsNotifications(string UserLogin, bool EnableSms);
    }
}