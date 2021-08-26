using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Security.Principal;
using ITWebNet.Food.Site;

namespace Kpi.Apps.Web.Tests.Tools
{
    internal class ControllerContextHelper
    {
        public static ControllerContext Setup(long userId, string email = "default", string role = "Admin")
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Email, email));
            identity.AddClaim(new Claim(ClaimTypes.Role, role));

            var controllerContext = new Mock<ControllerContext>();
            var principal = new Mock<IPrincipal>();
            principal.Setup(p => p.IsInRole("Admin")).Returns(true);
            principal.SetupGet(x => x.Identity.Name).Returns(userId.ToString());
            principal.Setup(x => x.Identity).Returns(identity);
            controllerContext.SetupGet<IPrincipal>(x => x.HttpContext.User).Returns(principal.Object);
            controllerContext.SetupGet<object>(s => SessionExtensions.Get<string>(s.HttpContext.Session, It.IsAny<string>())).Returns(-1);
            return controllerContext.Object;
        }
    }
}