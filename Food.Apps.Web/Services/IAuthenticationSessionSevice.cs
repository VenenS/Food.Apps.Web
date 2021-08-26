using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services.AuthorizationService
{
    public interface IAuthenticationSessionSevice
    {
        Task<HttpResult<Dictionary<string, string>>> GetTickets();
        Task RemoveTicket(string key);
        Task RenewTicket(string key, string ticketData, string userName);
        Task SaveTicket(string key, string ticketData, string userName);
    }
}