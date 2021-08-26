using ITWebNet.Food.Core.DataContracts.Account;

namespace ITWebNet.Food.Site.Models
{
    public class LoginRegisterViewModel
    {
        public LoginModel LoginModel { get; set; }

        public RegisterModel RegisterModel { get; set; }

        public bool IsLoginActive { get; set; } = true;
    }
}