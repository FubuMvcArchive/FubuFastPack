using System;
using System.Linq.Expressions;
using FubuCore.Reflection;
using FubuFastPack.Domain;

namespace FubuFastPack.Querying
{
    public class SingleEntityFilter<T> : IDataSourceFilter<T> where T : DomainEntity
    {
        private readonly T _domainEntity;

        public SingleEntityFilter(T domainEntity)
        {            
            _domainEntity = domainEntity;
            DenyingRestriction = null;
        }

        public void WhereEqual(Expression<Func<T, object>> property, object value)
        {
            if (!CanView) return;
            var entityValue = ReflectionHelper.GetAccessor(property).GetValue(_domainEntity);

            if (ReferenceEquals(entityValue, null))
            {
                if (!ReferenceEquals(value, null)) setCanView(false);
            }
            else
            {
                if (!entityValue.Equals(value)) setCanView(false);
            }
        }

        public void WhereNotEqual(Expression<Func<T, object>> property, object value)
        {
            if (!CanView) return;
            var entityValue = ReflectionHelper.GetAccessor(property).GetValue(_domainEntity);
            if (ReferenceEquals(entityValue, null))
            {
                if (ReferenceEquals(value, null)) setCanView(false);
            }
            else
            {
                if (entityValue.Equals(value)) setCanView(false);
            }
        }

        public void Or(Action<IOrOptions<T>> left, Action<IOrOptions<T>> right)
        {
            Or(new []{left, right});
        }


        public void Or(Action<IOrOptions<T>>[] orOperations)
        {
            var orOptions = new OrOptions<T>();
            foreach (var orOperation in orOperations)
            {
                orOperation(orOptions);
            }
            var compile = orOptions.BuildOut().Compile();
            var canView = false;

            try
            {
                canView = compile.Invoke(_domainEntity);
            }
            catch (NullReferenceException) { }
            finally
            {
                setCanView(canView);
            }
        }

        public void ApplyRestriction(IDataRestriction<T> restriction)
        {
            restriction.Apply(this);
            if (!CanView && DenyingRestriction == null) DenyingRestriction = restriction;
        }

        // could be useful for diagnostics
        public IDataRestriction<T> DenyingRestriction { get; set; }

        private bool _canView = true;

        private void setCanView(bool canView)
        {
            if(!canView)
            {
                _canView = false;
            }
        }
        public bool CanView
        {
            get { return _canView; }            
        }
    }
}