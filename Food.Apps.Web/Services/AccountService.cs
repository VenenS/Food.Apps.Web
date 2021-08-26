using Food.Services.Models;
using Food.Services.Models.Account;
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
    public class AccountService : BaseClient<AccountService>, IAccountService
    {
        //TODO: Авторизация через соцсети. Удалить
        //private const string ExternalLoginsUri = Prefix + "externallogins";
        //private const string ExternalLoginUri = Prefix + "externallogin";
        private const string ForgotPasswordUri = Prefix + "forgotpassword";
        private const string LoginUri = Prefix + "login";
        private const string LoginAnonymousUri = Prefix + "loginanonymous";
        private const string LogoutUri = Prefix + "logout";
        private const string ManageInfoUri = Prefix + "manageinfo";
        private const string Prefix = "api/account/";
        //TODO: Авторизация через соцсети. Удалить
        //private const string RegisterExternalUri = Prefix + "registerexternal";
        private const string RegisterUri = Prefix + "register";
        private const string ResetPasswordUri = Prefix + "resetpassword";
        private const string SendEmailConfirmationUri = Prefix + "SendEmailConfirmation";
        private const string ValidateResetPasswordKey = Prefix + "validateresetpasswordkey";
        private const string SignInUsingPasswordToken = Prefix + "signinusingpasswordtoken";
        private const string ConfirmEmailUrl = Prefix + "ConfirmEmail";


        private static readonly string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static readonly string BaseUrl = ServiceUrl + "api/account";

        [ActivatorUtilitiesConstructor]
        public AccountService() : base(ServiceUrl)
        {
        }

        public AccountService(string baseAddress) : base(baseAddress)
        {
        }

        public AccountService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public AccountService(bool debug) : base(debug)
        {
        }

        //TODO: Авторизация через соцсети. Удалить
        /// <summary>
        /// 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        //public virtual async Task<HttpResult<IEnumerable<ExternalLoginModel>>> ExternalLogins(string returnUrl)
        //{
        //    return await GetAsync<IEnumerable<ExternalLoginModel>>(ExternalLoginsUri, "returnUrl=" + returnUrl);
        //}

        //TODO: Авторизация через соцсети. Удалить
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //public virtual async Task<HttpResult<TokenModel>> RegisterExternal(RegisterExternalModel model)
        //{
        //    return await PostAsync<TokenModel>(RegisterExternalUri, model);
        //}

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<HttpResult<string>> ForgotPassword(ForgotPasswordModel model)
        {
            return await PostAsync<string>(ForgotPasswordUri, model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<HttpResult<TokenModel>> Login(LoginModel model)
        {
            return await PostAsync<TokenModel>(LoginUri, model);
        }

        /// <summary>
        /// Авторизация по номеру телефона и смс коду
        /// </summary>
        public virtual async Task<ResponseModel> LoginSms(LoginSmsModel model)
        {
            var httpResponse = await PostAsync<ResponseModel>(Prefix + "loginsms", model);
            if (httpResponse.Succeeded)
            {
                return httpResponse.Content;
            }
            else
            {
                return new ResponseModel() { Message = "Не удалось авторизировать пользователя", Status = 1 };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<HttpResult<TokenModel>> LoginAnonymous()
        {
            return await PostAsync<TokenModel>(LoginAnonymousUri, null);
        }


        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<HttpResult<string>> Register(RegisterModel model)
        {
            return await PostAsync<string>(RegisterUri, model);
        }

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<HttpResult<string>> ResetPassword(ResetPasswordModel model)
        {
            return await PostAsync<string>(ResetPasswordUri, model);
        }

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<HttpResult<string>> SendEmailConfirmation(EmailConfirmationModel model)
        {
            return await PostAsync<string>(SendEmailConfirmationUri, model);
        }

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<HttpResult<ResetPasswordModel>> ValidateResetPasswordToken(long userId, string tokenKey)
        {
            var result = await PostAsync<ResetPasswordModel>($"{ValidateResetPasswordKey}/user/{userId}/key/{tokenKey}", null);
            return result;
        }
        
        public virtual async Task<ConfirmEmailResultModel> ConfirmEmail(long userId, string code)
        {
            var response = await GetAsync<ConfirmEmailResultModel>(ConfirmEmailUrl + $"/?userId={userId}&code={code}");
            return response.Content;
        }
    }
}
