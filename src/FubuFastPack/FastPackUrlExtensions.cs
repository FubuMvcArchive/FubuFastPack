using System;
using FubuFastPack.Crud.Properties;
using FubuMVC.Core.Urls;

namespace FubuFastPack
{
    public static class FastPackUrlExtensions
    {
        public static string UrlForPropertyUpdate(this IUrlRegistry registry, object model)
        {
            return registry.UrlFor(model, FastPackUrlCategories.PROPERTY_EDIT);
        }

        public static string UrlForPropertyUpdate(this IUrlRegistry registry, Type type)
        {
            var o = Activator.CreateInstance(type);
            return registry.UrlForPropertyUpdate(o);
        }
    }
}