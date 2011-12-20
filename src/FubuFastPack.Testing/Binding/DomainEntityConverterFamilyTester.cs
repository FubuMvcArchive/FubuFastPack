using System;
using FubuCore.Conversion;
using FubuFastPack.Binding;
using FubuFastPack.Persistence;
using FubuFastPack.Testing.Security;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuFastPack.Testing.Binding
{
    [TestFixture]
    public class DomainEntityConverterFamilyTester : InteractionContext<DomainEntityConverterFamily>
    {
        [Test]
        public void should_match_an_entity()
        {
            ClassUnderTest.Matches(typeof(Case), null).ShouldBeTrue();
            ClassUnderTest.Matches(typeof(Site), null).ShouldBeTrue();
            ClassUnderTest.Matches(typeof(Solution), null).ShouldBeTrue();
        }

        [Test]
        public void should_not_match_primitives()
        {
            ClassUnderTest.Matches(typeof(string), null).ShouldBeFalse();
            ClassUnderTest.Matches(typeof(int), null).ShouldBeFalse();
            ClassUnderTest.Matches(typeof(DateTime), null).ShouldBeFalse();
            ClassUnderTest.Matches(typeof(long), null).ShouldBeFalse();
        }

        [Test]
        public void can_return_a_finder_for_an_entity()
        {
            // Mock out a new case
            var id = Guid.NewGuid();
            var @case = new Case{Id = id};

            var repository = MockFor<IRepository>();
            repository.Stub(x => x.Find<Case>(id)).Return(@case);

            // Mock out the Conversion Request
            var request = MockFor<IConversionRequest>();
            request.Expect(x => x.Get<IRepository>()).Return(repository);
            request.Stub(x => x.Text).Return(id.ToString());

            // Assert the converter works
            var converter = ClassUnderTest.CreateConverter(typeof (Case), null);
            converter.Convert(request).ShouldBeOfType<Case>();

        }
    }
}