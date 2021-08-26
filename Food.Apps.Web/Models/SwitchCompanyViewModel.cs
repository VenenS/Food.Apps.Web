using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;

namespace ITWebNet.Food.Site.Models
{
    /// <summary>
    /// Используется для выбора компании, в рамках которой будет выполняться корпоративный заказ
    /// </summary>
    public class SwitchCompanyViewModel
    {
        ///// <summary>
        ///// Имя компании, в рамках которой будет выполняться корпоративный заказ
        ///// </summary>
        //public string SelectedCompanyName { get; set; }

        /// <summary>
        /// Список компаний для выбора
        /// </summary>
        public List<CompanyModel> Companys { get; set; }
    }
}