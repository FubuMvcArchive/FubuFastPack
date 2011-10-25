using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public void Or(Action<IOrOptions<T>> left, Action<IOrOptions<T>> right)
        {
            Or(new[]{left,right});
        }

        public void Or(params Action<IOrOptions<T>>[] orClauses)
        {
            var orOptions = new OrOptions<T>();
            foreach (var clause in orClauses)
            {
                clause(orOptions);
            }
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
        private ComposableOrOperation _orOperations = new ComposableOrOperation();

        public void WhereIn(Expression<Func<T, object>> property, IEnumerable<object> value)
        {
            _orOperations.Set(property, value);
        }
        
        public void WhereEqual(Expression<Func<T, object>> property, object value)
        {
            _orOperations.Set(property, value);
        }

        public Expression<Func<T, bool>> BuildOut()
        {
            var a = _orOperations.GetPredicateBuilder<T>();
            return a;
        }
    }

    public interface IOrOptions<T>
    {
        void WhereIn(Expression<Func<T, object>> property, IEnumerable<object> value);
        void WhereEqual(Expression<Func<T, object>> property, object value);        
    }
}