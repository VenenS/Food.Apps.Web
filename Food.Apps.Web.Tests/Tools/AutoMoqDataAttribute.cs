using Ploeh.AutoFixture.NUnit3;

namespace Kpi.Apps.Web.Tests.Tools
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(FakeFactory.Fixture)
        {
        }
    }
}
