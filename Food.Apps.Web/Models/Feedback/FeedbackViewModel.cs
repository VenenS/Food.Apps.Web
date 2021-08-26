using System.ComponentModel.DataAnnotations;

namespace ITWebNet.Food.Site.Models
{
    public class FeedbackViewModel
    {
        [Required(ErrorMessage ="Имя не указано")]
        [RegularExpression(@"^[а-яА-ЯёЁa-zA-Z0-9@\.\s]+$", ErrorMessage = "В имени могу присутствовать только буквы и цифры, символы @ и .")]
        [Display(Name = "Ваше имя")]
        [StringLength(50, ErrorMessage = "Максимальное количество символов для отображаемого имени - {1}")]
        public string UserName { get; set; }

        [Required(ErrorMessage ="Не указан Email")]
        [Display(Name = "Email")]
        [RegularExpression(@"^([a-zA-Z0-9_\.-]+)@([A-Za-z0-9_\.-]+)\.([A-Za-z\.]{2,6})$",
            ErrorMessage = "Некорректный Email")]
        [StringLength(70, ErrorMessage = "Максимальное количество символов для электронного адреса - {1}")]
        public string Email { get; set; }

        [Display(Name = "Тема")]
        [StringLength(50, ErrorMessage = "Максимальное количество символов для темы - {1}")]
        public string Title { get; set; }

        [Required(ErrorMessage ="Сообщение не должно быть пустым")]
        [Display(Name = "Сообщение"), DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "Длина сообщения не должна превышать {1} символов")]
        public string Message { get; set; }

        public string Url { get; set; }
    }
}