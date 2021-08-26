using Food.Services.Models;
using Food.Services.Models.Account;
using ITWebNet.Food.Core.DataContracts.Account;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services.AuthorizationService
{
    public interface IAccountService : IDisposable
    {
        //TODO: Авторизация через соцсети. Удалить
        //Task<HttpResult<IEnumerable<ExternalLoginModel>>> ExternalLogins(string returnUrl);
        Task<HttpResult<string>> ForgotPassword(ForgotPasswordModel model);
        Task<HttpResult<TokenModel>> Login(LoginModel model);
        Task<HttpResult<TokenModel>> LoginAnonymous();
        Task<ResponseModel> LoginSms(LoginSmsModel model);
        Task<HttpResult<string>> Register(RegisterModel model);

        //TODO: Авторизация через соцсети. Удалить
        //Task<HttpResult<TokenModel>> RegisterExternal(RegisterExternalModel model);
        Task<HttpResult<string>> ResetPassword(ResetPasswordModel model);
        Task<HttpResult<string>> SendEmailConfirmation(EmailConfirmationModel model);
        Task<HttpResult<ResetPasswordModel>> ValidateResetPasswordToken(long userId, string tokenKey);
        Task<ConfirmEmailResultModel> ConfirmEmail(long userId, string code);
    }
}