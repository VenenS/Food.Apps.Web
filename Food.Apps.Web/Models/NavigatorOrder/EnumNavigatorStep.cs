namespace ITWebNet.Food.Site.Models
{
    /// <summary>
    /// Шаги, которые отображаются при оформлении заказа
    /// </summary>
    public enum EnumNavigatorStep
    {
        /// <summary>
        /// Выбор кафе (сейчас не используется)
        /// </summary>
        ChooseCafe,
        /// <summary>
        /// Ознакомьтесь с меню
        /// </summary>
        SelectDishes,
        /// <summary>
        /// Оформите заказ
        /// </summary>
        Checkout,
        /// <summary>
        /// Встречайте курьера
        /// </summary>
        WaitDelivery
    }
}