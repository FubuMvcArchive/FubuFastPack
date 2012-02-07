using FubuFastPack.Domain;
using FubuMVC.Core.Registration.Querying;

namespace FubuFastPack.Crud.Properties
{
    public class PropertyUpdaterForwarder<T> : ChainForwarder<T> where T : DomainEntity
    {
        public PropertyUpdaterForwarder()
            : base(entity => new UpdatePropertyModel<T>(), FastPackUrlCategories.PROPERTY_EDIT)
        {
        }
    }
}