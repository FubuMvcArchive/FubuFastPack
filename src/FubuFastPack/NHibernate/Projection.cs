using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FubuCore.Reflection;
using FubuFastPack.Domain;
using FubuFastPack.Querying;
using NHibernate;
using NHibernate.Criterion;
using Expression = System.Linq.Expressions.Expression;
using FubuCore;

namespace FubuFastPack.NHibernate
{
    public class Projection<T> : IProjection, IDataSourceFilter<T> where T : DomainEntity
    {
        private const int INITIAL_MAX_PAGE_COUNT = 1000;
        private readonly IList<ProjectionColumn<T>> _columns = new List<ProjectionColumn<T>>();
        private readonly ISession _session;
        private readonly List<ICriterion> _wheres = new List<ICriterion>();
        private readonly List<ProjectionAlias> _whereAliases = new List<ProjectionAlias>();
 
        public Projection(ISession session)
        {
            MaxCount = INITIAL_MAX_PAGE_COUNT;
            SortBy = SortRule<T>.Ascending(x => x.Id);
            _session = session;
        }

        public int WhereCount
        {
            get { return _wheres.Count(); }
        }

        public IEnumerable<ProjectionAlias> Aliases {get
        {
            //var a = _columns.SelectMany(x => x.Aliases)
            //    .Union(_whereAliases)
            //    .Distinct();
            //^^^^ this fails because PropertyInfo.GetHashCode() is a FAIL ^^^

            //.Distinct() relies on GetHashCode() to determine if it has seen an item already
            //since PropertyInfo.GetHashCode() is based on Object.GetHashCode()
            //it is not sutible for this usage. Therefore we must hand 'distinct'
            //list uses 'equality' rather than hash codes so switching to that.
            var result = new List<ProjectionAlias>();
            foreach(var alias in _whereAliases)
            {
                if(!result.Contains(alias) && alias.PropertyAccessor != null)
                {
                    result.Add(alias);
                }
            }

            foreach (var alias in _columns.SelectMany(x => x.Aliases))
            {
                if (!result.Contains(alias) && alias.PropertyAccessor != null)
                {
                    result.Add(alias);
                }
            }

            return result;
        }}

        public SortRule<T> SortBy { get; set; }

        public int MaxCount { get; set; }

        void IDataSourceFilter<T>.WhereEqual(Expression<Func<T, object>> property, object value)
        {
            Where(property).IsEqualTo(value);
        }

        void IDataSourceFilter<T>.WhereNotEqual(Expression<Func<T, object>> property, object value)
        {
            Where(property).IsNot(value);
        }

        void IDataSourceFilter<T>.Or(Action<IOrOptions<T>> left, Action<IOrOptions<T>> right)
        {
            var orOptions = new OrOptions<T>();
            left(orOptions);
            right(orOptions);

            var orExpression = orOptions.BuildOut();
            var criterion = ConvertExpressionIntoCriterion.Convert(orExpression);
            _wheres.Add(Restrictions.Conjunction().Add(criterion));
        }

        public int Count()
        {
            return Count(c => c);
        }

        public ProjectionColumn<T> AddColumn(Accessor accessor)
        {
            var column = new ProjectionColumn<T>(accessor);
            _columns.Add(column);

            return column;
        }

        public IEnumerable<Accessor> SelectAccessors()
        {
            return _columns.Select(x => x.PropertyAccessor);
        }

        public void AddColumn(ProjectionColumn<T> column)
        {
            _columns.Add(column);
        }

        public ProjectionColumn<T> AddColumn(Expression<Func<T, object>> expression)
        {
            Debug.WriteLine("Adding column " + expression.ToAccessor().Name);
            return AddColumn(ReflectionHelper.GetAccessor(expression));
        }

        public IEnumerable<T> GetData(GridDataRequest page)
        {
            return assembleCriteria(page, false).List<T>();
        }

        public IEnumerable GetAllData()
        {
            var criteria = _session.CreateCriteria(typeof (T));
            criteria = addAliases(criteria);
            criteria = addWheres(criteria);
            criteria = AddTheProjections(criteria);


            return criteria.List();
        }

        private ICriteria assembleCriteria(GridDataRequest page, bool withProjection)
        {
            var criteria = GetFilteredCriteria();
            if (withProjection) criteria = AddTheProjections(criteria);
            criteria = AddSorting(criteria, page);
            return AddPaging(criteria, page);
        }

        public ICriteria GetFilteredCriteria()
        {
            var criteria = _session.CreateCriteria(typeof (T));
            criteria = addAliases(criteria);
            criteria = addWheres(criteria);
            return criteria;
        }

        public ICriteria AddSorting(ICriteria criteria, GridDataRequest page)
        {
            SortBy.ApplyDefaultSorting(page);

            criteria = criteria.AddOrder(new Order(page.SortColumn, page.SortAscending));
            return criteria;
        }

        public ICriteria AddPaging(ICriteria criteria, GridDataRequest page)
        {
            if (page.ResultsPerPage > MaxCount)
            {
                page.ResultsPerPage = MaxCount;
            }

            criteria = criteria.SetFirstResult(page.ResultsToSkip()).SetMaxResults(page.ResultsPerPage);
            return criteria;
        }

        public IList ExecuteCriteriaWithProjection(GridDataRequest page)
        {
            return assembleCriteria(page, true).List();
        }

        public ICriteria AddTheProjections(ICriteria criteria)
        {
            var projections = Projections.ProjectionList();

            _columns.Each(column => column.AddProjection(projections));

            return criteria.SetProjection(projections);
        }

        protected virtual ICriteria addWheres(ICriteria criteria)
        {
            foreach (ICriterion criterion in _wheres)
            {
                criteria = criteria.Add(criterion);
            }

            return criteria;
        }


        public void AddRestriction(ICriterion criterion)
        {
            _wheres.Add(criterion);
        }

        public WhereExpression Where(Expression<Func<T, object>> expression)
        {
            var accessor = ReflectionHelper.GetAccessor(expression);
            ProjectionAlias.For(accessor, false).Each(x => _whereAliases.Fill(x));
            return new WhereExpression(expression, _wheres);
        }


        // TODO -- Reuse the CriterionBuilder here 

        public int Count(Func<ICriteria, ICriteria> chain)
        {
            var criteria = criteriaForCount(chain);
            var count = (int) criteria.UniqueResult();
            return count;
        }

        public void CountsOf<U>(Expression<Func<T, U>> property, Action<U, int> callback)
        {
            var criteria = _session.CreateCriteria(typeof (T));
            criteria = addWheres(criteria);

            var propertyName = property.ToPropertyName();
            var projections = Projections.ProjectionList();
            projections.Add(Projections.GroupProperty(propertyName));
            projections.Add(Projections.Count(propertyName));


            criteria = criteria.SetProjection(projections);


            criteria.List().Cast<object[]>().Each(o => { callback((U) o.GetValue(0), (int) o.GetValue(1)); });
        }

        protected ICriteria criteriaForCount(Func<ICriteria, ICriteria> chain)
        {
            var criteria = _session.CreateCriteria(typeof (T));
            criteria = addAliases(criteria);
            criteria = addWheres(criteria);
            criteria = chain(criteria);

            var propertyName = _columns.Any() ? _columns[0].PropertyName : "Id";
            criteria = criteria.SetProjection(Projections.Count(propertyName));
            return criteria;
        }

        private ICriteria addAliases(ICriteria criteria)
        {
            Aliases.Each(x => criteria = x.Apply(criteria));
            return criteria;            
        }

        public void OuterJoin(Accessor accessor)
        {
            var column = _columns.FirstOrDefault(c => c.PropertyAccessor == accessor);
            if (column != null)
            {
                column.OuterJoin = true;
            }
            var alias = _whereAliases.FirstOrDefault(c => c.PropertyAccessor == accessor);
            if(alias != null)
            {
                alias.OuterJoin = true;
            }
        }

        public void AddWhere(ICriterion criterion)
        {
            _wheres.Add(criterion);
        }

        #region Nested type: AndExpression

        public interface AndExpression
        {
            WhereExpression And(Expression<Func<T, object>> expression);
        }

        #endregion

        #region Nested type: WhereExpression

        public class WhereExpression : AndExpression
        {
            private readonly List<ICriterion> _wheres;
            private Accessor _lastAccessor;

            public WhereExpression(Expression<Func<T, object>> expression, List<ICriterion> wheres)
            {
                _wheres = wheres;
                _lastAccessor = ReflectionHelper.GetAccessor(expression);
            }

            public WhereExpression And(Expression<Func<T, object>> expression)
            {
                _lastAccessor = ReflectionHelper.GetAccessor(expression);
                return this;
            }

            public AndExpression IsEqualTo(object value)
            {
                var criterion = Restrictions.Eq(getPropertyName(), value);
                _wheres.Add(criterion);

                return this;
            }

            public AndExpression StartsWith(string beginning)
            {
                var criterion = Restrictions.InsensitiveLike(getPropertyName(), beginning + "%");
                _wheres.Add(criterion);
                return this;
            }

            private string getPropertyName()
            {
                return _lastAccessor.PropertyNames.Join(".");
            }

            public AndExpression IsNotIs(object value)
            {
                var criterion = Restrictions.Not(Restrictions.Eq(getPropertyName(), value));
                _wheres.Add(criterion);

                return this;
            }

            public AndExpression IsIn(object[] collection)
            {
                var criterion = Restrictions.In(getPropertyName(), collection);
                _wheres.Add(criterion);

                return this;
            }

            public AndExpression ContainsCaseInsensitive(object value)
            {
                var criterion = Restrictions.InsensitiveLike(getPropertyName(), "%" + value + "%");
                _wheres.Add(criterion);
                return this;
            }

            public AndExpression IsNot(object value)
            {
                var criterion = Restrictions.Not(Restrictions.Eq(_lastAccessor.ToPropertyName(), value));
                _wheres.Add(criterion);
                return this;
            }

            public AndExpression IsNull()
            {
                var criterion = Restrictions.IsNull(_lastAccessor.ToPropertyName());
                _wheres.Add(criterion);
                return this;
            }

            public AndExpression IsOnOrAfter(DateTime date)
            {
                var criterion = Restrictions.Ge(_lastAccessor.ToPropertyName(), date);
                _wheres.Add(criterion);
                return this;
            }

            public AndExpression IsOnOrBefore(DateTime date)
            {
                var criterion = Restrictions.Le(_lastAccessor.ToPropertyName(), date);

                _wheres.Add(criterion);
                return this;
            }
        }

        #endregion
    }


    public class ConvertExpressionIntoCriterion 
    {
        public static ICriterion Convert(Expression exp)
        {
            try
            {
                if (exp.NodeType == ExpressionType.Lambda)
                {
                    return ConvertLambda(exp);
                }

                if (exp.NodeType == ExpressionType.OrElse)
                {
                    return ConvertOrElse(exp);
                }

                if (exp.NodeType == ExpressionType.Call)
                {
                    return ConvertCall(exp);
                }

                if (exp.NodeType == ExpressionType.Equal)
                {
                    return ConvertEqual(exp);
                }
            }
            catch (Exception ex)
            {
                var message ="I made a mess in my pants. Derp! I got a node type of '{0}' with expression '{1}'".ToFormat(exp.NodeType, exp.ToString());
                throw new Exception(message, ex);
            }

            var message2 = "I don't know what to do. Derp! I got a node type of '{0}' with expression '{1}'".ToFormat(exp.NodeType, exp.ToString());
            throw new Exception(message2);
        }

        private static ICriterion ConvertEqual(Expression exp)
        {
            var a = (BinaryExpression) exp;
            
            var left = (MemberExpression) a.Left;
            var right = (ConstantExpression)a.Right;

            //need to walk the '.'s
            
            var path = getPath(left);
            return global::NHibernate.Criterion.Expression.Eq(path, right.Value);

        }

        //TODO: to fubucore?
        private static string getPath(MemberExpression exp)
        {
            var stack = new Stack<string>();
            var exp1 = exp;

            while (exp1 != null)
            {
                stack.Push(exp1.Member.Name);
                exp1 = exp1.Expression as MemberExpression;
            }


            var path = stack.Aggregate((l, r) => l + "." + r);
            return path;
        }

        public static ICriterion ConvertOrElse(Expression exp)
        {
            var b = (BinaryExpression) exp;

            var left = Convert(b.Left);
            var right = Convert(b.Right);

            return global::NHibernate.Criterion.Expression.Or(left, right);
        }

        public static ICriterion ConvertCall(Expression exp)
        {
            var call = (MethodCallExpression) exp;
            var collectionType = call.Arguments.Skip(1).First().Type;
           var subCrit = DetachedCriteria.For(collectionType);
           subCrit.SetProjection(Projections.Id());

           var arg = (MemberExpression)call.Arguments.Skip(1).First();
           
           var propertyToCheck = arg.Member.Name;;

           return Subqueries.PropertyIn(propertyToCheck + ".Id", subCrit);
        }

        public static ICriterion ConvertLambda(Expression exp)
        {
            var lambda = (LambdaExpression) exp;
            return Convert(lambda.Body);
        }
    }
}