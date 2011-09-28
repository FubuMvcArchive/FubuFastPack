using System.Collections.Generic;
using FubuFastPack.Domain;
using FubuFastPack.Querying;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Security;

namespace FubuFastPack.Security
{
    public class RestrictedDataAuthorizationPolicy<T> : IAuthorizationPolicy where T : DomainEntity
    {
        private readonly IEnumerable<IDataRestriction<T>> _dataRestrictions;

        public RestrictedDataAuthorizationPolicy(IEnumerable<IDataRestriction<T>> dataRestrictions)
        {
            _dataRestrictions = dataRestrictions;
        }

        public AuthorizationRight RightsFor(IFubuRequest request)
        {
            var entity = request.Get<T>();
            if(entity.IsNew())
            {
                return AuthorizationRight.None;
            }

            var entityFilter = new SingleEntityFilter<T>(entity);
            _dataRestrictions.Each(entityFilter.ApplyRestriction);
            return entityFilter.CanView ? AuthorizationRight.None : AuthorizationRight.Deny;
        }
    }
}