using System;
using System.ComponentModel.DataAnnotations;

namespace ITWebNet.Food.Site.Attributes
{
    /// <summary>
    /// Проверяет, чтобы Дата начала была меньше Даты окончания
    /// </summary>
    public class StartDateLessEndDate : ValidationAttribute
    {
        //Имена полей дат в модели
        string _startDate, _endDate;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="namePropertyStartDate">Имя поля в моделе "Дата начала"</param>
        /// <param name="namePropertyEndDate">Имя поля в моделе "Дата окончания"</param>
        /// <param name="errorMessage">Сообщение ошибки</param>
        public StartDateLessEndDate(string namePropertyStartDate, string namePropertyEndDate)
        {
            _startDate = namePropertyStartDate;
            _endDate = namePropertyEndDate;
        }

        protected override ValidationResult IsValid(object modelProperty, ValidationContext validationContext)
        {
            var startDateProperty = validationContext.ObjectType.GetProperty(_startDate);
            var endDateProperty = validationContext.ObjectType.GetProperty(_endDate);
            if (startDateProperty == null || endDateProperty == null)
                throw new ArgumentException("Одно из полей модели равно null: {_startDate} или {_endDate}.");

            var start = (DateTime)startDateProperty.GetValue(validationContext.ObjectInstance);
            var end = (DateTime)endDateProperty.GetValue(validationContext.ObjectInstance);
            if (start < end) return ValidationResult.Success;
            return new ValidationResult(ErrorMessage);
        }
    }
}