using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FoodApps.Web.NetCore.Extensions
{
    public static class HttpContextExtension
    {
        public static async Task SignInAsync(this HttpContext context, string token, bool isPersistent = false)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            var extraClaims = securityToken.Claims.Where(c => !identity.Claims.Any(x => x.Type == c.Type)).ToList();
            extraClaims.Add(new Claim("jwt", token));
            identity.AddClaims(extraClaims);
            var authenticationProperties = new AuthenticationProperties()
            {
                IssuedUtc = DateTimeOffset.FromUnixTimeSeconds(long.Parse(identity.Claims.First(c => c.Type == JwtRegisteredClaimNames.Nbf)?.Value)),
                ExpiresUtc = DateTimeOffset.FromUnixTimeSeconds(long.Parse(identity.Claims.First(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value)),
                IsPersistent = isPersistent
            };
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticationProperties);
        }
    }
}
