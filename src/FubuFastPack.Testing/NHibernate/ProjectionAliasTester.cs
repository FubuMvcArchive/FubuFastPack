using System.Collections.Generic;
using FubuCore.Reflection;
using FubuFastPack.NHibernate;
using FubuTestApplication.Domain;
using FubuTestingSupport;
using NHibernate;
using NHibernate.SqlCommand;
using NUnit.Framework;
using System.Linq;
using Rhino.Mocks;

namespace FubuFastPack.Testing.NHibernate
{
    [TestFixture]
    public class when_creating_a_projection_alias
    {         
        [Test]
        public void should_not_create_an_alias_for_a_simple_property()
        {
            var accessor = ReflectionHelper.GetAccessor<Case>(x => x.Condition);
            ProjectionAlias.For(accessor, false).ShouldHaveCount(0);
        }

        [Test]
        public void should_create_an_alias_for_a_chained_property()
        {
            var accessor = ReflectionHelper.GetAccessor<Case>(x => x.Person.Created);
            ProjectionAlias.For(accessor, false).ShouldHaveCount(1);
        }

        [Test]
        public void should_set_the_accessor_on_the_projection_alias()
        {
            var accessor = ReflectionHelper.GetAccessor<Case>(x => x.Person.Created);
            var projectionAlias = ProjectionAlias.For(accessor, false).ElementAt(0);
            projectionAlias.PropertyAccessor.ShouldEqual(accessor);
        }

        [Test]
        public void should_set_outer_join_on_the_projection_alias()
        {
            var accessor = ReflectionHelper.GetAccessor<Case>(x => x.Person.Created);
            var projectionAlias = ProjectionAlias.For(accessor, false).ElementAt(0);
            projectionAlias.OuterJoin.ShouldEqual(false);

            projectionAlias = ProjectionAlias.For(accessor, true).ElementAt(0);
            projectionAlias.OuterJoin.ShouldEqual(true);
        }
    }

    [TestFixture]
    public class when_applying_a_projection_alias
    {
        private ICriteria _criteria;
        private Accessor _propertyAccessor;

        [SetUp]
        public void SetUp() {
            _criteria = MockRepository.GenerateStub<ICriteria>();
            _propertyAccessor = ReflectionHelper.GetAccessor<Case>(x => x.Person.Name);
        }

        [Test]
        public void should_create_the_alias_with_the_property_name()
        {            
            var projectionAlias = new ProjectionAlias(_propertyAccessor, false);
            projectionAlias.Apply(_criteria);
            _criteria.AssertWasCalled(x=> x.CreateAlias(Arg<string>.Is.Equal("Person"), Arg<string>.Is.Equal("Person"), Arg<JoinType>.Is.Anything));
        }

        [Test]
        public void should_indicate_left_outer_join_if_outer_join_is_specified()
        {
            var projectionAlias = new ProjectionAlias(_propertyAccessor, true);
            projectionAlias.Apply(_criteria);
            _criteria.AssertWasCalled(x => x.CreateAlias(Arg<string>.Is.Equal("Person"), Arg<string>.Is.Equal("Person"), Arg<JoinType>.Is.Equal(JoinType.LeftOuterJoin)));
        }

        [Test]
        public void should_indicate_inner_join_by_default()
        {
            var projectionAlias = new ProjectionAlias(_propertyAccessor);
            projectionAlias.Apply(_criteria);
            _criteria.AssertWasCalled(x => x.CreateAlias(Arg<string>.Is.Equal("Person"), Arg<string>.Is.Equal("Person"), Arg<JoinType>.Is.Equal(JoinType.InnerJoin)));
        }

    }
}