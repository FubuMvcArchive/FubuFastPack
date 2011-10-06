using System;
using System.Collections.Generic;
using System.Linq;
using FubuFastPack.NHibernate;
using FubuFastPack.Persistence;
using FubuFastPack.Querying;
using FubuFastPack.StructureMap;
using FubuTestApplication;
using FubuTestApplication.Domain;
using FubuTestingSupport;
using NHibernate;
using NUnit.Framework;
using FubuValidation;
using StructureMap;

namespace IntegrationTesting.FubuFastPack.NHibernate
{
    [TestFixture]
    public class ProjectionTester 
    {
        [SetUp]
        protected  void beforeEach()
        {
            DatabaseDriver.GetFullFastPackContainer();
           
            

            using (var container = DatabaseDriver.ContainerWithDatabase())
            {
                container.Configure(x => x.UseOnDemandNHibernateTransactionBoundary());
            var session = container.GetInstance<ISession>();
                xxx(session, container);


            }
        }

        public void xxx(ISession session, IContainer container)
        {
            var @case = new Case();
            @case.Identifier = "1";
            @case.Number = 0;

            var person1 = new Person();
            person1.Name = "Ryan";
            var person2 = new Person();
            person2.Name = "Brandon";

            session.FlushMode = FlushMode.Always;

            session.Save(person1);
            session.Save(person2);
            @case.Person = person1;
            session.SaveOrUpdate(@case);
            session.Flush();


            session.CreateCriteria(typeof (Case)).List<Case>().Any()
                .ShouldBeTrue();

            var case2 = session.Get<Case>(@case.Id);

            @case.Identifier.ShouldEqual(case2.Identifier);
            @case.Number.ShouldEqual(case2.Number);
        }

        [Test]
        public void projection_stuff()
        {
            

            using (var container = DatabaseDriver.ContainerWithDatabase())
            {
                container.Configure(x => x.UseOnDemandNHibernateTransactionBoundary());

                var session = container.GetInstance<ISession>();

                xxx(session, container);
                var persons = session.CreateCriteria<Person>().List<Person>();
                
                
                var classUnderTest = container.GetInstance<Projection<Case>>();

                classUnderTest.AddColumn(x => x.Person.Name);
                classUnderTest.AddColumn(x => x.Number);
                (classUnderTest as IDataSourceFilter<Case>).Or(x => x.WhereEqual(y => y.Person.Name, "Ryan"),
                                                               x =>
                                                               x.WhereIn(y => y.Person, persons));
                classUnderTest.GetAllData().Count().ShouldEqual(1);
            }
        }
    }
}