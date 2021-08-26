using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITWebNet.Food.Site.Areas.Curator.Models
{
    public class ScheduleViewModel
    {
        public CompanyOrderScheduleModel Schedule { get; set; }
        public CafeModel Cafe { get; set; }
        public CompanyModel Company { get; set; }
    }

    public class ScheduleCreateModel : IValidatableObject
    {
        public CompanyOrderScheduleModel Schedule { get; set; }
        public CompanyModel Company { get; set; }
        public IEnumerable<SelectListItem> Cafes { get; set; }
        public string ValidationError { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var model = (ScheduleCreateModel)validationContext.ObjectInstance;

            if (model.Schedule.EndDate != null && (model.Schedule.BeginDate != null && model.Schedule.BeginDate.Value.Date > model.Schedule.EndDate.Value.Date))
                yield return new ValidationResult("Дата окончания действия расписания не должна быть меньше даты начала", new List<string> { "Schedule.EndDate" });

            if (model.Schedule.OrderStartTime > model.Schedule.OrderStopTime)
                yield return new ValidationResult("Время окончания приема заказов должно быть больше времени начала", new List<string> { "Schedule.OrderStopTime" });

            if (model.Schedule.OrderSendTime < model.Schedule.OrderStopTime)
                yield return new ValidationResult("Время отправления заказа должно быть больше времени окончания", new List<string> { "Schedule.OrderSendTime" });
        }
    }

    public class ScheduleEditModel : IValidatableObject
    {
        public CompanyOrderScheduleModel Schedule { get; set; }
        public string ValidationError { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var model = (ScheduleEditModel)validationContext.ObjectInstance;

            if (model.Schedule.EndDate != null && (model.Schedule.BeginDate != null && model.Schedule.BeginDate.Value.Date > model.Schedule.EndDate.Value.Date))
                yield return new ValidationResult("Дата окончания действия расписания не должна быть меньше даты начала", new List<string> { "Schedule.EndDate" });

            if (model.Schedule.OrderStartTime > model.Schedule.OrderStopTime)
                yield return new ValidationResult("Время окончания приема заказов должно быть больше времени начала", new List<string> { "Schedule.OrderStopTime" });

            if (model.Schedule.OrderSendTime < model.Schedule.OrderStopTime)
                yield return new ValidationResult("Время отправления заказа должно быть больше времени окончания", new List<string> { "Schedule.OrderSendTime" });

            if (model.Schedule.OrderStopTime - model.Schedule.OrderStartTime < new TimeSpan(0, 30, 0))
                yield return new ValidationResult("Время окончания заказа должно быть больше Времени начала на 30 минут", new List<string> { "Schedule.OrderStopTime" });
        }
    }
}