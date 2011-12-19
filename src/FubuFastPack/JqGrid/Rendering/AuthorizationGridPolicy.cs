using System.Collections.Generic;
using FubuFastPack.Domain;
using Microsoft.Practices.ServiceLocation;

namespace FubuFastPack.JqGrid
{
    public class AuthorizationGridPolicy : IGridPolicy
    {
        private IServiceLocator _services;

        public AuthorizationGridPolicy(IServiceLocator services)
        {
            _services = services;
        }

        public void AlterDefinition<T>(GridDefinition<T> definition) where T : DomainEntity
        {
            var failed = new List<IGridColumn>();

            definition.Columns.Each(col => {
                
                //using an enumeration here, instead of a double dispatch
                //model because we currently only have a two outcome
                //use case - in the future if this needs to get more
                //sophisticated I would expect a double dispatch style
                //model.
                if(col.ApplyAuthorization(_services) == ColumnAuthorizationAction.RemoveColumn)
                {
                    failed.Add(col);
                }
            });

            failed.Each(definition.RemoveColumn);
        }

        public void AlterGrid(ISmartGrid grid)
        {
            //no-op
        }
    }
}