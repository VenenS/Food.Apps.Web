using System;
using System.Collections.Generic;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ITWebNet.Food.Site.Hubs
{

    public class NotificationsHub : Hub, INotificationsHub
    {
        protected IHubContext<NotificationsHub> _context;

        public NotificationsHub(IHubContext<NotificationsHub> context)
        {
            _context = context;
        }

        [HubMethodName("alerts")]
        public void Alerts()
        {
        }

        public void SendMessage(List<PushMessageModel> messages)
        {
            foreach (var item in messages)
            {
                if (item.Action == ActionEnum.Logout && !string.IsNullOrWhiteSpace(item.UserName))
                {
                    Clients.User(item.UserName).SendAsync("redirectTo", "/account/logout/");
                    continue;
                }

                string action = item.Action.ToString().ToLower();
                string title = item.Action.GetDescription();
                string icon;
                switch (item.Action)
                {
                    case ActionEnum.Info:
                        icon = "info";
                        break;
                    case ActionEnum.Success:
                        icon = "check";
                        break;
                    case ActionEnum.Warning:
                        icon = "exclamation";
                        break;
                    case ActionEnum.Danger:
                        icon = "times";
                        break;
                    default:
                        icon = "info";
                        break;
                }

                if (string.IsNullOrWhiteSpace(item.UserName))
                    Clients.All.SendAsync("SendMessage", title, item.Message, action, icon);
                else
                    Clients.User(item.UserName).SendAsync("SendMessage", title, item.Message, action, icon);
            }
        }

        public void NorifyAboutOrder(List<string> managerEmails, string url)
        {
            //Clients всегда был null
            //Clients?.All?.SendAsync("alertino");
            _context?.Clients?.All?.SendAsync("alertino");
            try
            {
                //Clients?.Users(managerEmails).SendAsync("raiseAlert", url);
                _context.Clients?.Users(managerEmails)?.SendAsync("raiseAlert", url);
            }
            catch (Exception e)
            {

            }
        }
    }
}