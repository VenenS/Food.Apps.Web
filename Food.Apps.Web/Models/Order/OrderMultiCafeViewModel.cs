namespace ITWebNet.Food.Site.Models
{
    public class OrderMultiCafeViewModel
    {
        public CartMultiCafeViewModel CartMulti { get; set; }

        public DeliveryInfoViewModel DeliveryInfo { get; set; }

        /// <summary>
        /// Определение того ушел заказ в кафе или нет
        /// </summary>
        public bool OrderIsDone { get; set; }

        public MessageViewModel Message { get; set; }
    }
}