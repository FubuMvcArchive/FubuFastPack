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
    public class ProjectionTester : InteractionContext<Projection<Case> >
    {
        protected override void beforeEach()
        {
            DatabaseDriver.GetFullFastPackContainer();
            var @case = new Case();
            @case.Identifier = "1";
            @case.Number = 0;
            using (var container = DatabaseDriver.ContainerWithDatabase())
            {
                container.Configure(x => x.UseOnDemandNHibernateTransactionBoundary());

                var session = container.GetInstance<ISession>();
                session.FlushMode = FlushMode.Always;

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
            
            ClassUnderTest.AddColumn(x => x.Person.Name);
            ClassUnderTest.AddColumn(x => x.Number);
            (ClassUnderTest as IDataSourceFilter<Case>).Or(x=> x.WhereEqual(y=> y.Number, 1), x=> x.WhereIn(y=> y.Identifier, new List<string>{"1", "2", "3"}));
            ClassUnderTest.GetAllData().Count().ShouldEqual(1);
        }
    }
}