using ITWebNet.Food.Core.DataContracts.Account;

namespace ITWebNet.Food.Site.Models
{
    /// <summary>
    /// Модель авторизации
    /// </summary>
    public class LoginViewModel
    {
        public LoginModel Login { get; set; }
        public LoginSmsViewModel LoginSms { get; set; }
    }
}