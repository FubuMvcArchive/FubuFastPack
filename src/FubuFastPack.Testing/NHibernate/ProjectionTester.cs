using FubuCore.Reflection;
using FubuFastPack.NHibernate;
using FubuTestApplication.Domain;
using FubuTestingSupport;
using NUnit.Framework;

namespace FubuFastPack.Testing.NHibernate
{
    [TestFixture]
    public class when_adding_a_where_expression : InteractionContext<Projection<Case>>
    {         
        [Test]
        public void the_where_should_be_added_to_the_aliases()
        {
            ClassUnderTest.Where(x => x.Person.Name).Equals("foo");
            ClassUnderTest.Aliases.ShouldHaveCount(1);
        }

        [Test]
        public void should_only_return_the_same_alias_one_time()
        {
            ClassUnderTest.Where(x => x.Person.Name).Equals("foo");
            ClassUnderTest.Where(x => x.Person.Name).Equals("foo");
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
            ClassUnderTest.Where(x => x.Person.Name).Equals("foo");
            
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