namespace ITWebNet.Food.Core.DataContracts.Common
{
    /// <summary>
    /// Ответ от сервисов
    /// </summary>
    public class ResponseModel
    {
        /// <summary>
        /// Статус операции
        /// 0 - success
        /// 1 - error
        /// 2 - warning
        /// 3 - info
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Message { get; set; }

        public object Result { get; set; }
    }
}
