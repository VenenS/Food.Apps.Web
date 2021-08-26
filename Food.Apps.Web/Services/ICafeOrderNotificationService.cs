using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface ICafeOrderNotificationService
    {
        Task<long> GetNotificationForUser(long userId);
        Task<List<string>> NewOrder(long cafeId, DateTime deliveryDate);
        Task<ResponseModel> SendSmsCode(string phone, bool isConfirming = false);
        Task<bool> SetOrdersViewed(long cafeId);
        Task<bool> StopNotifyUser(long userId);
    }
}