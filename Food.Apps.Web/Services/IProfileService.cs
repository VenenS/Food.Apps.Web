using ITWebNet.Food.Core.DataContracts.Account;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services.AuthorizationService
{
    public interface IProfileService : IDisposable
    {
        //TODO: Авторизация через соцсети. Удалить
        //Task<HttpResult<string>> AddExternalLogin(AddExternalLoginModel model);
        Task<HttpResult<string>> ChangePassword(ChangePasswordModel model);
        Task<HttpResult<UserInfoModel>> ConfirmPhone(UserInfoModel model);
        Task<List<BanketModel>> GetAvailableBankets(long cafeId);
        Task<List<CompanyModel>> GetMyCompaniesAsync(long userId);
        Task<UserInCompanyModel> GetMyCompany(long companyId);
        Task<HttpResult<UserInfoModel>> GetUserInfo();
        Task<HttpResult<string>> RemoveLogin(RemoveLoginModel model);
        Task<HttpResult<TokenModel>> SaveUserInfo(UserInfoModel model);
        Task<HttpResult<string>> SetPassword(SetPasswordModel model);
        Task<long?> GetUserCompanyId();
        Task<HttpResult<bool>> SendEmailConfirmationCode();
        Task<HttpResult<UserInfoModel>> ConfirmEmail(UserInfoModel model);
    }
}