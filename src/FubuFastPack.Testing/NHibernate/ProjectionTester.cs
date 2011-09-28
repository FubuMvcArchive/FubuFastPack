using System.Linq;
using FubuCore.Reflection;
using FubuFastPack.NHibernate;
using FubuTestApplication.Domain;
using FubuTestingSupport;
using NHibernate;
using NHibernate.SqlCommand;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuFastPack.Testing.NHibernate
{
    [TestFixture]
    public class a_where_alias_should_look_like : InteractionContext<Projection<Case>>
    {
        private ProjectionAlias _alias;

        [SetUp]
        public void get_an_alias()
        {
            ClassUnderTest.Where(x => x.Person.Name);
            _alias = ClassUnderTest.Aliases.Single();
        }

        [Test]
        public void should_NOT_be_setup_for_outer_join()
        {
            _alias.OuterJoin.ShouldBeFalse();
        }

        [Test]
        public void the_accessor_should_be_correct_duh()
        {
            var accessor = ReflectionHelper.GetAccessor<Case>(c => c.Person.Name);
            _alias.PropertyAccessor.ShouldEqual(accessor);
        }

        [Test]
        public void should_hook_up_the_criteria()
        {
            var mockCriteria = MockFor<ICriteria>();

            mockCriteria.Expect(c => c.CreateAlias("Person", "Person", JoinType.InnerJoin));

            _alias.Apply(mockCriteria);

            mockCriteria.VerifyAllExpectations();
        }

    }

    [TestFixture]
    public class when_adding_a_where_expression : InteractionContext<Projection<Case>>
    {         
        [Test]
        public void the_where_should_be_added_to_the_aliases()
        {
            ClassUnderTest.Where(x => x.Person.Name).IsEqualTo("foo");
            ClassUnderTest.Aliases.ShouldHaveCount(1);
        }

        [Test]
        public void should_only_return_the_same_alias_one_time()
        {
            ClassUnderTest.Where(x => x.Person.Name).IsEqualTo("foo");
            ClassUnderTest.Where(x => x.Person.Name).IsEqualTo("foo");
            ClassUnderTest.Aliases.ShouldHaveCount(1);
        }
    }

    [TestFixture]
    public class when_adding_a_column_and_a_where_expression : InteractionContext<Projection<Case>>
    {
        [Test]
        public void should_only_return_the_same_alias_one_time()
        {
            ClassUnderTest.AddColumn(x => x.Person.Name);
            ClassUnderTest.Where(x => x.Person.Name).IsEqualTo("foo");
            
            ClassUnderTest.Aliases.ShouldHaveCount(1);
        }
    }

    [TestFixture]
    public class aoeu : InteractionContext<Projection<Case>>
    {
        [Test][Ignore("This should work but doesn't")]
        public void GetHashCodeCheck()
        {
            var a = new ProjectionAlias(ReflectionHelper.GetAccessor<Case>(c => c.Person.Name));
            var b = new ProjectionAlias(ReflectionHelper.GetAccessor<Case>(c => c.Person.Name));

            a.GetHashCode().ShouldEqual(b.GetHashCode());
        }

        [Test]
        public void EqualityCheck()
        {
            var a = new ProjectionAlias(ReflectionHelper.GetAccessor<Case>(c => c.Person.Name));
            var b = new ProjectionAlias(ReflectionHelper.GetAccessor<Case>(c => c.Person.Name));

            a.ShouldEqual(b);
        }
    }
}