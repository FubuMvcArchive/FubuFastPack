using System;
using FubuCore;
using FubuCore.Conversion;
using FubuCore.Reflection;
using FubuFastPack.Crud;
using FubuFastPack.Domain;
using FubuFastPack.Persistence;

namespace FubuFastPack.Binding
{
    public class DomainEntityConverterFamily : IObjectConverterFamily
    {
        // Matches any type deriving from DomainEntity
        // CanBeCastTo<> is an extension method in FubuCore as well
        public bool Matches(Type type, ConverterLibrary converter)
        {
            return type.CanBeCastTo<DomainEntity>() && !type.HasAttribute<IgnoreEntityInBindingAttribute>();
        }

        // In this case we find the correct object by looking it up by Id
        // from our repository
        public IConverterStrategy CreateConverter(Type type, Func<Type, IConverterStrategy> converterSource)
        {

            return new LambdaConverterStrategy<DomainEntity, IRepository>((repo, text) =>
                                                                             {
                                                                                 if (text.IsEmpty()) return null;

                                                                                 var id = new Guid(text);
                                                                                 return repo.Find(type, id);
                                                                             },"Converting domain entities");

        }
    }
}