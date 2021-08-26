using ITWebNet.Food.Site;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace Food.Apps.Web.Tests.Extensions
{
    [TestFixture]
    class UrlHelperExtensionsTest
    {
        private Mock<UrlHelper> _urlHelper;

        [SetUp]
        public void SetUp()
        {
            _urlHelper = new Mock<UrlHelper>();
        }

        private void MockActionWillReturn(Mock<UrlHelper> h, string retval)
        {
            h.Setup(x => x.Action(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<RouteValueDictionary>(),
                It.IsAny<string>()))
             .Returns(retval);
        }

        [Test]
        public void ActionWithoutSubdomainTest()
        {
            MockActionWillReturn(_urlHelper, "http://cafename.localhost/test?abc=qed");
            Assert.AreEqual(
                ITWebNet.Food.Site.UrlHelperExtensions.ActionWithoutSubdomain(_urlHelper.Object, "doesn't", "matter"),
                "http://localhost/test?abc=qed"
            );

            MockActionWillReturn(_urlHelper, "http://localhost/test?abc=qed");
            Assert.AreEqual(
                ITWebNet.Food.Site.UrlHelperExtensions.ActionWithoutSubdomain(_urlHelper.Object, "doesn't", "matter"),
                "http://localhost/test?abc=qed"
            );

            MockActionWillReturn(_urlHelper, "http://cafename.abc.test.com/test?abc=qed");
            Assert.AreEqual(
                ITWebNet.Food.Site.UrlHelperExtensions.ActionWithoutSubdomain(_urlHelper.Object, "doesn't", "matter"),
                "http://abc.test.com/test?abc=qed"
            );

            MockActionWillReturn(_urlHelper, "http://cafename.abc.test.com:8080/test?abc=qed");
            Assert.AreEqual(
                ITWebNet.Food.Site.UrlHelperExtensions.ActionWithoutSubdomain(_urlHelper.Object, "doesn't", "matter"),
                "http://abc.test.com:8080/test?abc=qed"
            );
        }
    }
}
