using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentNHibernate.MappingModel;
using FubuCore.Reflection.Expressions;

namespace FubuFastPack.Querying
{
    public class QueryableDataSourceFilter<T> : IDataSourceFilter<T>
    {
        private readonly IList<Expression<Func<T, bool>>> _wheres
            = new List<Expression<Func<T, bool>>>();

        public IEnumerable<Expression<Func<T, bool>>> Wheres
        {
            get { return _wheres; }
        }

        public void WhereEqual(Expression<Func<T, object>> property, object value)
        {
            var expression = new EqualsPropertyOperation().GetPredicate(property, value);
            _wheres.Add(expression);
        }

        public void WhereNotEqual(Expression<Func<T, object>> property, object value)
        {
            var expression = new NotEqualPropertyOperation().GetPredicate(property, value);
            _wheres.Add(expression);
        }

        public void Or(Expression<Func<T, bool>> or)
        {
            throw new NotImplementedException();
        }

        public void OrIsIn(Expression<Func<T, object>> property, ICollection<object> values)
        {
            throw new NotImplementedException();
        }

        public void Or(Action<IOrOptions<T>> left, Action<IOrOptions<T>> right)
        {
            var orOptions = new OrOptions<T>();
            left(orOptions);
            right(orOptions);
            _wheres.Add(orOptions.BuildOut());

        }

        public IQueryable<T> Filter(IQueryable<T> queryable)
        {
            _wheres.Each(x => queryable = queryable.Where(x));

            return queryable;
        }
    }

    public class OrOptions<T> : IOrOptions<T>
    {
        
        public void WhereIn(Expression<Func<T, object>> property, IEnumerable<object> value)
        {
            o.Set(property, value);
        }
        
        public void WhereEqual(Expression<Func<T, object>> property, object value)
        {
            o.Set(property, value);
        }

        private ComposableOrOperation o;
        public Expression<Func<T, bool>> BuildOut()
        {
            var a = o.GetPredicateBuilder<T>();
            return a;
        }
    }

    public interface IOrOptions<T>
    {
        void WhereIn(Expression<Func<T, object>> property, IEnumerable<object> value);

        void WhereEqual(Expression<Func<T, object>> property, object value);

    }
}