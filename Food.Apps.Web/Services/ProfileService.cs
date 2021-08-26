using ITWebNet.Food.Core.DataContracts.Account;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services.AuthorizationService
{
    /// <summary>
    /// Autorization service client 
    /// </summary>
    public class ProfileService : BaseClient<ProfileService>, IProfileService
    {
        private static string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static string BaseUrl = ServiceUrl + "api/profile";
        //TODO: Авторизация через соцсети. Удалить
        //private string AddExternalLoginUri = BaseUrl + "/addexternallogin";
        private string ChangePasswordUri = BaseUrl + "/changepassword";
        private string RemoveLoginUri = BaseUrl + "/removelogin";
        private string SetPasswordUri = BaseUrl + "/setpassword";
        private string UserInfoUri = BaseUrl + "/userinfo";
        private string ConfirmPhoneUri = BaseUrl + "/confirmphone";
        private string ConfirmEmailUri = BaseUrl + "/confirmemail";
        private string SendEmailConfirmationUri = BaseUrl + "/SendEmailConfirmation";


        /// <summary>
        /// Initializes a new instance of Autorization service client 
        /// </summary>
        [ActivatorUtilitiesConstructor]
        public ProfileService()
            : base(ServiceUrl)
        {
            //client.BaseAddress = new Uri("http://localhost:1684/");
        }

        /// <summary>
        /// Initializes a new instance of Autorization service client with authentication token
        /// </summary>
        /// <param name="token"></param>
        public ProfileService(string token)
            : base(
                  ServiceUrl,
                  token)
        {
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        public ProfileService(bool debug) : base(debug)
        {
        }

        //TODO: Авторизация через соцсети. Удалить
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //public virtual async Task<HttpResult<string>> AddExternalLogin(AddExternalLoginModel model)
        //{
        //    return await PostAsync<string>(AddExternalLoginUri, model);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<HttpResult<string>> SetPassword(SetPasswordModel model)
        {
            return await PostAsync<string>(SetPasswordUri, model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<HttpResult<string>> RemoveLogin(RemoveLoginModel model)
        {
            return await PostAsync<string>(RemoveLoginUri, model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<HttpResult<string>> ChangePassword(ChangePasswordModel model)
        {
            return await PostAsync<string>(ChangePasswordUri, model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual async Task<HttpResult<UserInfoModel>> GetUserInfo()
        {
            return await GetAsync<UserInfoModel>($"{UserInfoUri}");
        }

        public virtual async Task<HttpResult<TokenModel>> SaveUserInfo(UserInfoModel model)
        {
            return await PostAsync<TokenModel>(UserInfoUri, model);
        }

        public virtual async Task<HttpResult<UserInfoModel>> ConfirmPhone(UserInfoModel model)
        {
            return await PostAsync<UserInfoModel>(ConfirmPhoneUri, model);
        }

        public virtual async Task<List<CompanyModel>> GetMyCompaniesAsync(long userId)
        {
            var response = await GetAsync<List<CompanyModel>>(BaseUrl + $"/companies");
            return response.Succeeded ? response.Content : new List<CompanyModel>();
        }

        public virtual async Task<UserInCompanyModel> GetMyCompany(long companyId)
        {
            var response = await GetAsync<UserInCompanyModel>(BaseUrl + "/companies/" + companyId);
            return response.Succeeded ? response.Content : null;
        }

        public virtual async Task<List<BanketModel>> GetAvailableBankets(long cafeId)
        {
            var response = await GetAsync<List<BanketModel>>(BaseUrl + $"/bankets/{cafeId}");
            return response.Succeeded ? response.Content : new List<BanketModel>();
        }

        public async Task<long?> GetUserCompanyId()
        {
            return (await GetAsync<long?>($"{BaseUrl}/GetUserCompanyId")).Content;
        }

        public virtual async Task<HttpResult<bool>> SendEmailConfirmationCode()
        {
            return await GetAsync<bool>(SendEmailConfirmationUri);
        }

        public virtual async Task<HttpResult<UserInfoModel>> ConfirmEmail(UserInfoModel model)
        {
            return await PostAsync<UserInfoModel>(ConfirmEmailUri, model);
        }
    }
}