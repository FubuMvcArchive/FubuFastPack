using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FubuFastPack.Querying
{
    public interface IDataSourceFilter<T>
    {
        void WhereEqual(Expression<Func<T, object>> property, object value);
        void WhereNotEqual(Expression<Func<T, object>> property, object value);
        void Or(Expression<Func<T, bool>> or);
        void OrIsIn(Expression<Func<T, object>> property, ICollection<object> values);
    }
}