using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Models
{
    [ModelBinder(typeof(DeliveryInfoViewModelBinder))]
    public class DeliveryInfoViewModel
    {
        [Display(Name = "Телефон")]
        [Required(ErrorMessage = "Пожалуйста, укажите номер телефона", AllowEmptyStrings = false)]
        //[Phone]
        //[StringLength(15, ErrorMessage = "Максимальное количество символов для телефона - {1}")]
        //[MinLength(6, ErrorMessage = "Минимальное количество символов для телефона - {1}")]
        //[RegularExpression(@"^9[0-9]{9}$", ErrorMessage = "Номер телефона должен содержать 10 цифр")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Электронная почта")]
        [EmailAddress(ErrorMessage = "Укажите правильный электронный адрес")]
        [StringLength(70, ErrorMessage = "Максимальное количество символов для электронного адреса - {1}")]
        [RegularExpression(@"^([a-z0-9_-]+\.)*[a-z0-9_-]+@[a-z0-9_-]+(\.[a-z0-9_-]+)*\.[a-z]{2,6}$", ErrorMessage = "Ваша почта введена некорректно")]
        public string Email { get; set; }

        [Display(Name = "Комментарий к заказу")]
        [DataType(DataType.MultilineText)]
        [StringLength(300, ErrorMessage = "Максимальное количество символов для комментария - {1}")]
        public string OrderComment { get; set; }

        [Display(Name = "Подготовить сдачу с ")]
        [RegularExpression(@"[0-9]{0,10}", ErrorMessage = "Некорректное значение")]
        [StringLength(10, ErrorMessage = "Максимум символов для сдачи {1}")]
        public string MoneyComment { get; set; }
        /// <summary>
        /// Принятие пользовательского соглашения
        /// </summary>
        public bool ConfirmAgreement { get; set; }

        /// <summary>
        /// чекбокс "без сдачи"
        /// </summary>
        public bool NoChange { get; set; }

        /// <summary>
        /// Определяет является ли заказ корпоративным
        /// </summary>
        public bool IsCompanyOrder { get; set; }

        #region Sms оповещение

        /// <summary>
        /// Флаг включения оповещения по СМС на уровне компании
        /// </summary>
        public bool SmsEnabledCompany { get; set; } = false;
        /// <summary>
        /// Флаг включения оповещения по СМС на уровне пользователя
        /// </summary>
        [Display(Name = "Оповещать по SMS об изменении состояния заказов")]
        public bool SmsEnabledUser { get; set; } = false;
        /// <summary>
        /// Флаг включения оповещения по СМС на уровне пользователя, не изменяемый самим пользователем.
        /// Нужен, чтобы понять, изменилось ли значение и надо ли сохранить его в настройки пользователя.
        /// </summary>
        public bool SmsUser_BeforeChange { get; set; } = false;

        #endregion

        public DeliveryAddressViewModel Address { get; set; }

        public List<DateTime> DateDelivery { get; set; }

        /// <summary>
        /// Способ оплаты заказа
        /// </summary>
        public EnumOrderPayType PayType { get; set; }
        
        /// <summary>
        /// для юриков безнал
        /// </summary>
        [Display(Name = "Безналичный расчет")]
        public string PayTypeLegalEntity { get; set; }
    }

    public class DeliveryAddressViewModel
    {
        [Display(Name = "Адрес доставки")]
        [Required(ErrorMessage = "Пожалуйста, укажите адрес доставки", AllowEmptyStrings = false)]
        [DataType(DataType.MultilineText)]
        [StringLength(150, ErrorMessage = "Максимальное количество символов для адреса - {1}")]
        public string DeliveryAddress { get; set; }
        [Display(Name = "Улица")]
        public string Street { get; set; }
        [Display(Name = "Дом")]
        public string House { get; set; }
        [Display(Name = "Строение")]
        public string Building { get; set; }
        [Display(Name = "Квартира")]
        public string Flat { get; set; }
        [Display(Name = "Офис")]
        public string Office { get; set; }
        [Display(Name = "Подъезд")]
        public string Entrance { get; set; }
        [Display(Name = "Этаж")]
        public string Storey { get; set; }
        [Display(Name = "Домофон")]
        public string Intercom { get; set; }
        [Display(Name = "Заказ в компанию:")]
        public string CompanyName { get; set; }
        public long CompanyId { get; set; }

        [Display(Name = "Адрес доставки")]
        public IEnumerable<DeliveryAddressModel> CompanyAddresses { get; set; }
        public Int64? OrderId { get; set; }

        public DeliveryAddressViewModel()
        {
            CompanyAddresses = new List<DeliveryAddressModel>();
        }

        public static explicit operator DeliveryAddressViewModel(CompanyModel address)
        {
            return new DeliveryAddressViewModel()
            {
                 CompanyAddresses = address.Addresses,
                 CompanyName = address.Name,
                 CompanyId = address.Id
            };
        }
    }

    // Привязчик модели DeliveryInfoViewModel
    public class DeliveryInfoViewModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult;
            var model = (bindingContext.Model as DeliveryInfoViewModel) ?? new DeliveryInfoViewModel();
            // Привязывает основные свойства
            model.Address = new DeliveryAddressViewModel { DeliveryAddress = bindingContext.ValueProvider.GetValue("Address.DeliveryAddress").FirstValue };
            if (bool.TryParse(bindingContext.ValueProvider.GetValue("ConfirmAgreement").FirstValue, out bool confirmAgreement))
                model.ConfirmAgreement = confirmAgreement;
            model.Email = bindingContext.ValueProvider.GetValue("Email").FirstValue;
            if (bool.TryParse(bindingContext.ValueProvider.GetValue("IsCompanyOrder").FirstValue, out bool isCompanyOrder))
                model.IsCompanyOrder = isCompanyOrder;
            model.OrderComment = bindingContext.ValueProvider.GetValue("OrderComment").FirstValue;
            model.PhoneNumber = bindingContext.ValueProvider.GetValue("PhoneNumber").FirstValue;
            if (bool.TryParse(bindingContext.ValueProvider.GetValue("SmsEnabledCompany").FirstValue, out bool smsEnabledCompany))
                model.SmsEnabledCompany = smsEnabledCompany;
            if (bool.TryParse(bindingContext.ValueProvider.GetValue("SmsEnabledUser").FirstValue, out bool smsEnabledUser))
                model.SmsEnabledUser = smsEnabledUser;
            if (bool.TryParse(bindingContext.ValueProvider.GetValue("SmsUser_BeforeChange").FirstValue, out bool smsUserBefore))
                model.SmsUser_BeforeChange = smsUserBefore;
            model.MoneyComment = bindingContext.ValueProvider.GetValue("MoneyComment").FirstValue;
            string firstValue = bindingContext.ValueProvider.GetValue("PayType").FirstValue;
            if (bool.TryParse(bindingContext.ValueProvider.GetValue("NoChange").FirstValue, out bool noChange))
                model.NoChange = noChange;
            if (!String.IsNullOrEmpty(firstValue))
            {
                var x = Convert.ToInt32(firstValue);
                model.PayType = Enum.Parse<EnumOrderPayType>(x.ToString());
            }
            model.PayTypeLegalEntity = bindingContext.ValueProvider.GetValue("PayTypeLegalEntity").FirstValue;
            if (!model.IsCompanyOrder)
            {
                // Привязка дат
                var listTime = new List<DateTime>();
                for (int i = 0; ; i++)
                {
                    valueResult = bindingContext.ValueProvider.GetValue($"date_{i}");
                    if (valueResult != null && !string.IsNullOrWhiteSpace(valueResult.FirstValue))
                    {
                        string strDate = valueResult.FirstValue;
                        DateTime tempDate = !string.IsNullOrWhiteSpace(strDate) ? DateTime.Parse(strDate) : DateTime.Now.Date;

                        valueResult = bindingContext.ValueProvider.GetValue($"time{i}");
                        string strTime = valueResult.FirstValue;

                        DateTime deliveryDateTime = tempDate.Date;
                        if (strTime == "-1")
                        {
                            if (deliveryDateTime == DateTime.Now.Date)
                            {
                                deliveryDateTime = DateTime.Now;
                            }
                            else
                            {
                                var strTimeOpen = bindingContext.ValueProvider.GetValue($"cafe_open_{i}").FirstValue;
                                deliveryDateTime += TimeSpan.Parse(strTimeOpen);
                            }
                        }
                        else
                        {
                            deliveryDateTime += TimeSpan.Parse(strTime ?? "00:00");
                        }

                        listTime.Add(deliveryDateTime);
                    }
                    else
                        break;
                }
                model.DateDelivery = listTime;
            }
            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }

}
