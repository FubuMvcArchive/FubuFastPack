using System;
using FubuCore;
using FubuCore.Binding;
using FubuCore.Binding.InMemory;
using FubuCore.Conversion;
using FubuFastPack.Crud;
using FubuFastPack.Domain;
using NUnit.Framework;
using FubuTestingSupport;
using Rhino.Mocks;

namespace FubuFastPack.Testing.Binding
{
    [TestFixture]
    public class EditEntityModelBinderTester
    {
        private EditEntityModelBinder _binder;
        private InMemoryRequestData theData;
        private Guid theGuid;
        private InMemoryServiceLocator inMemoryServiceLocator;
        private ConverterLibrary cl;
        private ObjectConverter oc;

        [SetUp]
        public void SetUp()
        {
            inMemoryServiceLocator = new InMemoryServiceLocator();

            _binder = new EditEntityModelBinder(new NulloEntityDefaults());

            theGuid = Guid.NewGuid();

            theData = new InMemoryRequestData();
            theData["BobName"] = "Ryan";
            theData["Flavor"] = "choco";
            theData["Id"] = theGuid.ToString();

            cl = new ConverterLibrary();

            oc = new ObjectConverter(inMemoryServiceLocator, cl);

            inMemoryServiceLocator.Add<IObjectConverter>(oc);

            inMemoryServiceLocator.Add<IObjectResolver>(ObjectResolver.Basic());
        }

        [Test]
        public void cant_find_bob()
        {
            cl.RegisterConverter<Bob>(s => null);

            IObjectConverter oc = new ObjectConverter(inMemoryServiceLocator, cl);

            inMemoryServiceLocator.Add(oc);
            var results = _binder.Bind(typeof(EditBob), new BindingContext(theData, inMemoryServiceLocator, new NulloBindingLogger()));
            
            results.ShouldBeOfType<EditBob>();
            results.As<EditBob>().Target.ShouldNotBeNull();
            results.As<EditBob>().Target.As<Bob>().Name.ShouldEqual("Ryan");
        }

        [Test]
        public void finds_bob()
        {
            cl.RegisterConverter(s => new Bob {Name="Poop"});

            oc = new ObjectConverter(inMemoryServiceLocator, cl);

            inMemoryServiceLocator.Add(oc);
            var results = _binder.Bind(typeof(EditBob), new BindingContext(theData, inMemoryServiceLocator, new NulloBindingLogger()));

            results.ShouldBeOfType<EditBob>();
            results.As<EditBob>().Target.ShouldNotBeNull();
            results.As<EditBob>().Target.As<Bob>().Name.ShouldEqual("Ryan");

            results.As<EditBob>().Flavor.ShouldEqual("choco");
        }

        class EditBob : EditEntityModel
        {
            public EditBob(Bob target) : base(target)
            {
            }

            public string Flavor { get; set; }
        }

        class Bob : DomainEntity
        {
            public string Name { get; set; }
        }
    }

    
}