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
using NHibernate.Linq;
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
                (classUnderTest as IDataSourceFilter<Case>).Or(x => x.WhereEqual(y => y.IsSensitive, true),
                                                               x => x.WhereIn(y => y.Person, persons));
                classUnderTest.GetAllData().Count().ShouldEqual(1);
            }
        }



        [Test]
        public void two_where_equal_clauses()
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
                (classUnderTest as IDataSourceFilter<Case>).Or(x => x.WhereEqual(y => y.IsSensitive, true),
                                                               x => x.WhereEqual(y => y.Person, persons.First()));
                classUnderTest.GetAllData().Count().ShouldEqual(1);
            }
        }

        [Test]
        public void single_where_clause()
        {
            using (var container = DatabaseDriver.ContainerWithDatabase())
            {
                container.Configure(x => x.UseOnDemandNHibernateTransactionBoundary());

                var session = container.GetInstance<ISession>();

                xxx(session, container);    
                
                var classUnderTest = container.GetInstance<Projection<Case>>();

                classUnderTest.AddColumn(x => x.Person.Name);
                classUnderTest.AddColumn(x => x.Number);

                (classUnderTest as IDataSourceFilter<Case>).WhereEqual(y=>y.Person.Name, "Ryan");

                classUnderTest.GetAllData().Count().ShouldEqual(1);
            }
        }
    }
}