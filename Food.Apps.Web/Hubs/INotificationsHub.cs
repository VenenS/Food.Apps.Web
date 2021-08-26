using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;

namespace ITWebNet.Food.Site.Hubs
{
    public interface INotificationsHub
    {
        void Alerts();
        void NorifyAboutOrder(List<string> managerEmails, string url);
        void SendMessage(List<PushMessageModel> messages);
    }
}