using System.ComponentModel.DataAnnotations;

namespace ITWebNet.Food.Site.Models
{
    /// <summary>
    /// Модель входа на сайт через СМС код
    /// </summary>
    public class LoginSmsViewModel
    {
        public MessageViewModel Message { get; set; }

        /// <summary>
        /// Отправлен ли код 
        /// </summary>
        public bool IsSendCode { get; set; }

        /// <summary>
        /// Телефон
        /// </summary>
        [Required(ErrorMessage = "Введите номер телефона")]
        public string Phone { get; set; }

        /// <summary>
        /// Код из СМС
        /// </summary>
        [Required(ErrorMessage = "Поле обязательно")]
        public string SmsCode { get; set; }
    }
}