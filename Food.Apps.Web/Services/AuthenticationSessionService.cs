using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services.AuthorizationService
{
    public class AuthenticationSessionSevice : BaseClient<AuthenticationSessionSevice>, IAuthenticationSessionSevice
    {
        private const string Prefix = "api/authenticationSessions/";
        private const string AllUri = Prefix + "getall";
        private const string RemoveUri = Prefix + "remove/";
        private const string RenewUri = Prefix + "renew/";
        private const string SaveUri = Prefix + "save/";


        public AuthenticationSessionSevice()
            : base(FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/")
        {
            //HttpClient.BaseAddress = new Uri(HttpClient.BaseAddress, "res/AuthenticationSessions");
        }

        public AuthenticationSessionSevice(bool debug) : base(debug)
        {
        }

        public virtual async Task<HttpResult<Dictionary<string, string>>> GetTickets()
        {
            return await GetAsync<Dictionary<string, string>>(AllUri);
        }

        //public virtual async Task<string> GetTicket(string key)
        //{
        //    //return await Get<string>(key);
        //    return await GetResponseData<string>(await HttpClient.GetAsync("res/AuthenticationSessions/" + key));
        //}

        public virtual async Task RemoveTicket(string key)
        {
            await PostAsync<string>(RemoveUri + key, null);
        }

        public virtual async Task RenewTicket(string key, string ticketData, string userName)
        {
            await PostAsync<string>(
                RenewUri + key,
                new
                {
                    id = key,
                    ticketData = ticketData,
                    userName = userName
                });
        }

        public virtual async Task SaveTicket(string key, string ticketData, string userName)
        {

            await PostAsync<string>(
                SaveUri + key,
                new
                {
                    id = key,
                    ticketData = ticketData,
                    userName = userName
                });
        }
    }
}