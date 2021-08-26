using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Food.Services.Contracts;
using ITWebNet.Food.Core.DataContracts.Admin;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.Extensions.Configuration;

namespace ITWebNet.Food.Site.Services
{
    public class UsersService : BaseClient<UsersService>, IUsersService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static string BaseUrl = ServiceUrl + "api/users";

        public UsersService() : base(ServiceUrl)
        {
        }

        public UsersService(string baseAddress) : base(baseAddress)
        {
        }

        public UsersService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public UsersService(bool debug) : base(debug)
        {
        }

        public virtual async Task<List<UserModel>> GetUsers()
        {
            var response = await GetAsync<List<UserModel>>($"{BaseUrl}");
            return response.Succeeded ? response.Content : new List<UserModel>();
        }

        public virtual async Task<List<UserAdminModel>> GetAdminUsers()
        {
            var response = await GetAsync<List<UserAdminModel>>($"{BaseUrl}/adminUsers");
            return response.Succeeded ? response.Content : new List<UserAdminModel>();
        }

        public virtual async Task<List<UserWithLoginModel>> GetFullListOfUsers()
        {
            var response = await GetAsync<List<UserWithLoginModel>>($"{BaseUrl}/fulllistofusers");
            return response.Succeeded ? response.Content : new List<UserWithLoginModel>();
        }

        public virtual async Task<List<UserModel>> GetUsersWithoutCurators()
        {
            var response = await GetAsync<List<UserModel>>($"{BaseUrl}/without/curators");
            return response.Succeeded ? response.Content : new List<UserModel>();
        }

        public virtual async Task<bool> AddUserReferralLink(long parentId, long referralId)
        {
            var response = await GetAsync<bool>($"{BaseUrl}/adduserreferrallink/{parentId}/{referralId}");
            return response.Content;
        }

        public virtual async Task<bool> AddUserToRole(long roleId, long userId)
        {
            var response = await GetAsync<bool>($"{BaseUrl}/addusertorole/{roleId}/{userId}");
            return response.Content;
        }

        /// <summary>
        /// Проверяет уникальное ли имя блюда в рамках кафе
        /// </summary>
        public virtual async Task<bool> CheckUniqueEmail(string email, long userId = -1)
        {
            var response = await GetAsync<bool>($"{BaseUrl}/checkUniqueEmail?email={email}&userId={userId}");
            return response.Succeeded == true ? response.Content : false;
        }

        public virtual async Task<bool> EditUser(UserWithLoginModel model)
        {
            var response = await PostAsync<bool>($"{BaseUrl}/edituser", model);
            return response.Content;
        }

        public virtual async Task<bool> EditUserPointsByLogin(string login, long typePoints, double points)
        {
            var response = await GetAsync<bool>($"{BaseUrl}/edituserpointsbylogin?login={login}&typePoints={typePoints}&points={points}");
            return response.Content;
        }

        public virtual async Task<bool> EditUserPointsByLoginAndTotalPrice(string login, double totalPrice)
        {
            var response = await GetAsync<bool>($"{BaseUrl}/edituserpointsbyloginandprice?login={login}&totalPrice={totalPrice}");
            return response.Content;
        }

        public virtual async Task<bool> EditUserRole(UserRoleModel model)
        {
            var response = await PostAsync<bool>($"{BaseUrl}/edituserrole", model);
            return response.Content;
        }

        public virtual async Task<List<RoleModel>> GetListRoleToUser(long userId)
        {
            var response = await GetAsync<List<RoleModel>>($"{BaseUrl}/listroletouser/{userId}");
            return response.Succeeded ? response.Content : new List<RoleModel>();
        }

        public virtual async Task<List<UserRoleModel>> GetListUserRole()
        {
            var response = await GetAsync<List<UserRoleModel>>($"{BaseUrl}/listroleuser");
            return response.Succeeded ? response.Content : new List<UserRoleModel>();
        }

        public virtual async Task<List<RoleModel>> GetRoles()
        {
            var response = await GetAsync<List<RoleModel>>($"{BaseUrl}/roles");
            return response.Succeeded ? response.Content : new List<RoleModel>();
        }

        public virtual async Task<UserModel> GetUserByLogin(string login)
        {
            var response = await GetAsync<UserModel>($"{BaseUrl}/userbylogin?login={login}");
            return response.Succeeded ? response.Content : new UserModel();
        }

        public virtual async Task<UserModel> GetUserByEmail(string email)
        {
            var response = await GetAsync<UserModel>($"{BaseUrl}/userbyemail?email={email}");
            return response.Succeeded ? response.Content : new UserModel();
        }

        public virtual async Task<UserModel> GetUserByReferralLink(string referralLink)
        {
            var response = await GetAsync<UserModel>($"{BaseUrl}/userbylogin?referralLink={referralLink}");
            return response.Succeeded ? response.Content : new UserModel();
        }

        public virtual async Task<UserWithLoginModel> GetUserByRoleId(long roleId)
        {
            var response = await GetAsync<UserWithLoginModel>($"{BaseUrl}/userbyroleid/{roleId}");
            return response.Succeeded ? response.Content : new UserWithLoginModel();
        }

        public virtual async Task<double> GetUserPointsByLogin(string login)
        {
            var response = await GetAsync<double>($"{BaseUrl}/userpointsbylogin?login={login}");
            return response.Content;
        }

        public virtual async Task<List<UserReferralModel>> GetUserReferrals(long userId, int[] level)
        {
            var response = await PostAsync<List<UserReferralModel>>($"{BaseUrl}/getuserreferrals?userId={userId}", level);
            return response.Succeeded ? response.Content : new List<UserReferralModel>();
        }

        public virtual async Task<List<UserWithLoginModel>> GetUsersByCafeOrCompany(OrganizationTypeEnum organizationType, long id)
        {
            var response = await GetAsync<List<UserWithLoginModel>>($"{BaseUrl}/usersbycafeorcompany?id={id}&longOrganizationType={(long)organizationType}");
            return response.Succeeded ? response.Content : new List<UserWithLoginModel>();
        }

        public virtual async Task<bool> RemoveUserRole(UserRoleModel model)
        {
            var response = await PostAsync<bool>($"{BaseUrl}/deleteuserrole", model);
            return response.Content;
        }

        public virtual async Task SendFeedback(FeedbackModel feedback)
        {
            await PostAsync($"{BaseUrl}/feedback", feedback);
        }

        public virtual async Task SetUserSmsNotifications(string UserLogin, bool EnableSms)
        {
            await GetAsync<bool>($"{BaseUrl}/SetUserSmsNotifications?UserLogin={UserLogin}&EnableSms={EnableSms.ToString()}");
        }
    }
}