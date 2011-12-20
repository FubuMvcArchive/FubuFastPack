using System;
using FubuCore;
using FubuCore.Conversion;
using FubuFastPack.Binding;
using FubuFastPack.Crud;
using FubuFastPack.JqGrid;
using FubuFastPack.Querying;
using FubuMVC.Core;
using FubuMVC.Core.Registration.ObjectGraph;

namespace FubuFastPack
{
    public class FubuFastPackRegistryExtension : IFubuRegistryExtension
    {
        public void Configure(FubuRegistry registry)
        {
            registry.Services(x => x.AddService(typeof(IObjectConverterFamily), ObjectDef.ForType<DomainEntityConverterFamily>()));
            registry.Services(x => x.SetServiceIfNone<IQueryService, QueryService>());
            registry.Services(x => x.SetServiceIfNone<ISmartGridService, SmartGridService>());

            registry.Models
                //.BindModelsWith<EditEntityModelBinder>()
                //.BindModelsWith<EntityModelBinder>()
                .ConvertUsing<EntityConversionFamily>();
        }
    }
}