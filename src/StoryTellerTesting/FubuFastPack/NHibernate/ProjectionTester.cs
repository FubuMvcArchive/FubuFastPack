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

namespace IntegrationTesting.FubuFastPack.NHibernate
{
    [TestFixture]
    public class ProjectionTester 
    {
        [SetUp]
        protected  void beforeEach()
        {
            DatabaseDriver.GetFullFastPackContainer();
            var @case = new Case();
            @case.Identifier = "1";
            @case.Number = 0;

            var person1 = new Person();
            person1.Name = "Ryan";
            var person2 = new Person();
            person2.Name = "Brandon";
            

            using (var container = DatabaseDriver.ContainerWithDatabase())
            {
                container.Configure(x => x.UseOnDemandNHibernateTransactionBoundary());


                var session = container.GetInstance<ISession>();
                session.FlushMode = FlushMode.Always;

                session.Save(person1);
                session.Save(person2);
                @case.Person = person1;
                session.SaveOrUpdate(@case);
                session.Flush();


                session.CreateCriteria(typeof(Case)).List<Case>().Any()
                    .ShouldBeTrue();

                var case2 = session.Get<Case>(@case.Id);

                @case.Identifier.ShouldEqual(case2.Identifier);
                @case.Number.ShouldEqual(case2.Number);
            }
        }


        
        [Test]
        public void projection_stuff()
        {
            

            using (var container = DatabaseDriver.ContainerWithDatabase())
            using(var trx = container.GetInstance<ITransactionBoundary>())
            {
                trx.Start();

                var session = container.GetInstance<ISession>();
                var persons = session.CreateCriteria<Person>().List<Person>();
                

                var ClassUnderTest = container.GetInstance<Projection<Case>>();

                ClassUnderTest.AddColumn(x => x.Person.Name);
                ClassUnderTest.AddColumn(x => x.Number);
                (ClassUnderTest as IDataSourceFilter<Case>).Or(x => x.WhereEqual(y => y.Number, 1),
                                                               x =>
                                                               x.WhereIn(y => y.Person, persons));
                ClassUnderTest.GetAllData().Count().ShouldEqual(1);
            }
        }
    }
}