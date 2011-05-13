using System;
using System.Reflection;
using FubuCore;
using FubuCore.Binding;
using FubuCore.Reflection;
using FubuFastPack.Domain;
using FubuFastPack.Persistence;

namespace FubuFastPack.Crud
{
    public class EntityConversionFamily : StatelessConverter
    {
        public override bool Matches(PropertyInfo property)
        {
            return property.PropertyType.CanBeCastTo<DomainEntity>() 
                && !property.PropertyType.HasAttribute<IgnoreEntityInBindingAttribute>();
        }

        public override object Convert(IPropertyContext context)
        {
            var entityType = context.Property.PropertyType;
            var id = context.ValueAs<Guid?>();
            return id.HasValue ? context.Service<IRepository>().Find(entityType, id.Value) : null;
        }
    }
}