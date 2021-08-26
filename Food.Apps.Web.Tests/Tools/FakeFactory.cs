using System.Linq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;

namespace Kpi.Apps.Web.Tests.Tools
{
    static class FakeFactory
    {
        public static readonly Fixture Fixture = new Fixture();

        static FakeFactory()
        {
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
           // Fixture.Customize<SystemModel>(m => m.With(e => e.UpdatePeriod, 10).With(e => e.IsUpdatable, true));
           // Fixture.Customize<SelectList>(m => m.FromFactory(() => new SelectList(new List<string>())));
            Fixture.Customizations
            .OfType<FilteringSpecimenBuilder>()
            .Where(x => x.Specification is DictionarySpecification)
            .ToList().ForEach(c => Fixture.Customizations.Remove(c));
            Fixture.Customizations
                .OfType<FilteringSpecimenBuilder>()
                .Where(x => x.Specification is CollectionSpecification)
                .ToList().ForEach(c => Fixture.Customizations.Remove(c));
            Fixture.Customizations
                .OfType<FilteringSpecimenBuilder>()
                .Where(x => x.Specification is HashSetSpecification)
                .ToList().ForEach(c => Fixture.Customizations.Remove(c));
            Fixture.Customizations
                .OfType<FilteringSpecimenBuilder>()
                .Where(x => x.Specification is ListSpecification)
                .ToList().ForEach(c => Fixture.Customizations.Remove(c));
        }
    }
}
