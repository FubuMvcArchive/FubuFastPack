using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FubuCore.Reflection;
using FubuFastPack.Domain;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace FubuFastPack.NHibernate
{
    public class ProjectionAlias
    {
        public ProjectionAlias(Accessor propertyAccessor) : this(propertyAccessor, false) {}

        public ProjectionAlias(Accessor propertyAccessor, bool outerJoin)
        {
            PropertyAccessor = propertyAccessor;
            OuterJoin = outerJoin;
        }

        public Accessor PropertyAccessor { get; set; }
        public bool OuterJoin { get; set; }

        public bool Equals(ProjectionAlias other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.PropertyAccessor == null || PropertyAccessor == null) return false;
            return Equals(other.PropertyAccessor.PropertyNames[0], PropertyAccessor.PropertyNames[0]);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ProjectionAlias)) return false;
            return Equals((ProjectionAlias) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((PropertyAccessor != null ? PropertyAccessor.GetHashCode() : 0)*397);
            }
        }

        public ICriteria Apply(ICriteria criteria)
        {
            var associationPath = PropertyAccessor.PropertyNames[0];
            var joinType = OuterJoin ? JoinType.LeftOuterJoin : JoinType.InnerJoin;
            criteria = criteria.CreateAlias(associationPath, associationPath, joinType);

            return criteria;
        }
        
        public static IEnumerable<ProjectionAlias> For(Accessor accessor, bool outerJoin)
        {
            if (accessor == null || accessor.PropertyNames.Length <= 1)
            {
                yield break;
            }
            yield return new ProjectionAlias(accessor, outerJoin) ;
        }
    }

    public class ProjectionColumn<T> where T : DomainEntity
    {
        protected Accessor _accessor;

        public Accessor PropertyAccessor
        {
            get { return _accessor; }
        }

        public ProjectionColumn(Expression<Func<T, object>> expression)
            : this(expression.ToAccessor())
        {

        }

        public ProjectionColumn(Accessor accessor)
        {
            _accessor = accessor;
            if (_accessor != null) PropertyName = _accessor.ToPropertyName();
        }

        public string PropertyName { get; set; }
        public bool OuterJoin { get; set; }

        public virtual void AddProjection(ProjectionList projections)
        {
            var projection = Projections.Property(_accessor.ToPropertyName()).As(_accessor.Name);
            projections.Add(projection);
        }

        public virtual IEnumerable<ProjectionAlias> Aliases
        {
            get { return ProjectionAlias.For(_accessor, OuterJoin); }
        }

//        public virtual void AddAlias(Cache<string, bool> aliasAndJoinTypeMap)
//        {
//            var key = _accessor.PropertyNames[0];
//
//            if (_accessor.PropertyNames.Length <= 1 || aliasAndJoinTypeMap.Has(key)) return;
//
//            aliasAndJoinTypeMap[key] = OuterJoin;
//        }
    }

    public class DiscriminatorProjectionColumn<T> : ProjectionColumn<T> where T : DomainEntity
    {
        public DiscriminatorProjectionColumn() : base(null as Accessor)
        {
        }

        public override void AddProjection(ProjectionList projections)
        {
            projections.Add(Projections.Property("class"));
        }

//        public override void AddAlias(Cache<string, bool> aliasAndJoinTypeMap)
//        {
//            // no-op
//        }
    }
}