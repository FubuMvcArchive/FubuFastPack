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

        public void Or(Expression<Func<T, bool>> or)
        {
            throw new NotImplementedException();
        }

        public void OrIsIn(Expression<Func<T, object>> property, ICollection<object> values)
        {
            var expression = new CollectionContainsPropertyOperation().GetPredicate(property, values);
            _wheres.Add(expression);
        }

        public IQueryable<T> Filter(IQueryable<T> queryable)
        {
            _wheres.Each(x => queryable = queryable.Where(x));

            return queryable;
        }
    }
}