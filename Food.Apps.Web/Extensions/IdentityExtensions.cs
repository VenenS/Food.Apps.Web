using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace ITWebNet.Food.Site
{
    public static class IdentityExtensions
    {
        public static bool IsAnonymous(this IIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException("identity");

            return string.IsNullOrWhiteSpace(identity.Name) || identity.Name == "anonymous";
        }

        public static string GetJwtToken(this IIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException("identity");
            var ci = identity as ClaimsIdentity;
            return ci?.Claims.FirstOrDefault(c => c.Type == "jwt")?.Value;
        }

        public static string GetUserId(this IIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException("identity");
            var ci = identity as ClaimsIdentity;
            return ci?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        public static T GetUserId<T>(this IIdentity identity) where T :IConvertible
        {
            if (identity == null)
                throw new ArgumentNullException("identity");
            return (T)Convert.ChangeType(GetUserId(identity), typeof(T));
        }

        //TODO: Авторизация через соцсети. Удалить
        //public static bool IsExternal(this IIdentity identity)
        //{
        //    if (identity == null)
        //        throw new ArgumentNullException("identity");

        //    var ci = identity as ClaimsIdentity;
        //    if (ci == null)
        //        return false;

        //    Claim providerKeyClaim = ci.FindFirst(ClaimTypes.NameIdentifier);
        //    if (providerKeyClaim == null
        //            || string.IsNullOrEmpty(providerKeyClaim.Issuer)
        //            || String.IsNullOrEmpty(providerKeyClaim.Value))
        //        return false;

        //    return providerKeyClaim.Issuer != ClaimsIdentity.DefaultIssuer;
        //    return false;
        //}

        public static string GetUserEmail(this IIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException("identity");

            var ci = identity as ClaimsIdentity;
            return ci?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        }

        public static string GetUserPhone(this IIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException("identity");

            var ci = identity as ClaimsIdentity;
            return ci?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value;
        }

        public static string GetUserFullName(this IIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException("identity");

            var ci = identity as ClaimsIdentity;
            return ci?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
        }
    }
}