namespace ITWebNet.Food.Core.DataContracts.Common
{
    /// <summary>
    /// Контракт для отправки пользователю Push сообщения
    /// </summary>
    public class PushMessageModel
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Сообщение пользователю
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Тип сообщения/действия
        /// </summary>
        public ActionEnum Action { get; set; }
    }
}
