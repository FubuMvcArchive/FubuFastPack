using Bottles;
using FubuCore;
using FubuCore.Binding;
using FubuCore.Conversion;
using FubuFastPack.Binding;
using FubuFastPack.Crud;
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
            container = SetupContainerForFastPack();
            new FubuFastPackRegistryExtension().Configure(registry);

            AssetDeclarationVerificationActivator.Latched = true;

            FubuApplication.For(() => registry).StructureMap(() => container).Bootstrap();
            PackageRegistry.AssertNoFailures();
        }

        public static Container SetupContainerForFastPack()
        {
            return new Container(x =>
                                          {
                                              x.For<IRepository>().Use<InMemoryRepository>();
                                              x.For<IEntityFinder>().Use<EntityFinder>();
                                              x.For<IEntityFinderCache>().Use<StructureMapEntityFinderCache>();
                                              x.For<ICurrentHttpRequest>().Use<FakeCurrentHttpRequest>();
                                              //x.For<IModelBinder>().Use(StandardModelBinder.Basic()); - dropped from fubucore 4/20/2012
                                              x.For<IObjectResolver>().Use(ObjectResolver.Basic());
                                              x.For<IEntityDefaults>().Use<NulloEntityDefaults>();
                                          });
        }

        [Test]
        public void no_object_converter_families_registered()
        {
            container.GetAllInstances<IObjectConverterFamily>().ShouldHaveCount(0);
        }

        [Test]
        public void converter_family_is_registered()
        {
            container.GetAllInstances<IConverterFamily>().ShouldContain(x => x.GetType().CanBeCastTo<EntityConversionFamily>());
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