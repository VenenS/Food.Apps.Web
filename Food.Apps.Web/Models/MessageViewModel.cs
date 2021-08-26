namespace ITWebNet.Food.Site.Models
{
    /// <summary>
    /// Модель для отображения сообщения
    /// </summary>
    public class MessageViewModel
    {
        public string Text { get; set; }

        /// <summary>
        /// Из ConstantsMessage
        /// </summary>
        public string Type { get; set; }

        public int Status { get; set; }

        public MessageViewModel() { }

        public MessageViewModel(string text, string type)
        {
            Text = text;
            Type = type;
        }

        public static MessageViewModel Success(string text)
        {
            return new MessageViewModel(text, ConstantsMessage.Success);
        }

        public static MessageViewModel Error(string text)
        {
            return new MessageViewModel(text, ConstantsMessage.Danger);
        }

        public static MessageViewModel Info(string text)
        {
            return new MessageViewModel(text, ConstantsMessage.Info);
        }
    }

    public static class ConstantsMessage
    {
        public const string Success = "success";
        public const string Danger = "danger";
        public const string Info = "info";
    }
}