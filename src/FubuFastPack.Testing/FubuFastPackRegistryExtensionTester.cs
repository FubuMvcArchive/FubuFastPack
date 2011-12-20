using Bottles;
using FubuCore;
using FubuCore.Conversion;
using FubuFastPack.Binding;
using FubuFastPack.JqGrid;
using FubuFastPack.Persistence;
using FubuFastPack.Querying;
using FubuFastPack.StructureMap;
using FubuFastPack.Testing.jqGrid;
using FubuMVC.Core;
using FubuMVC.Core.Assets;
using FubuMVC.Core.Http;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using NUnit.Framework;
using StructureMap;

namespace FubuFastPack.Testing
{
    [TestFixture]
    public class FubuFastPackRegistryExtensionTester
    {
        private Container container;

        [SetUp]
        public void SetUp()
        {
            var registry = new FubuRegistry();
            container = new Container(x =>
            {
                x.For<IRepository>().Use<InMemoryRepository>();
                x.For<IEntityFinder>().Use<EntityFinder>();
                x.For<IEntityFinderCache>().Use<StructureMapEntityFinderCache>();
                x.For<ICurrentHttpRequest>().Use<FakeCurrentHttpRequest>();
            });
            new FubuFastPackRegistryExtension().Configure(registry);

            AssetDeclarationVerificationActivator.Latched = true;

            FubuApplication.For(() => registry).StructureMap(() => container).Bootstrap();
            PackageRegistry.AssertNoFailures();
        }

        [Test]
        public void object_converter_is_registered()
        {
            container.GetAllInstances<IObjectConverterFamily>().ShouldContain(x => x.GetType().CanBeCastTo<DomainEntityConverterFamily>());
        }

        [Test]
        public void query_service_is_registered()
        {
            container.GetInstance<IQueryService>().ShouldBeOfType<QueryService>();
        }

        [Test]
        public void smart_grid_service_is_registered()
        {
            container.GetInstance<ISmartGridService>().ShouldBeOfType<SmartGridService>();
        }
    }
}