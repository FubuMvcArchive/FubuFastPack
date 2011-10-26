using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FubuCore;
using FubuCore.Reflection;
using FubuFastPack.Domain;
using NHibernate.Criterion;
using Expression = System.Linq.Expressions.Expression;

namespace FubuFastPack.NHibernate
{
    public class ConvertExpressionIntoCriterion<T> where T : DomainEntity
    {
        private readonly List<ProjectionAlias> _whereAliases = new List<ProjectionAlias>();
        private Projection<T> _projection;

        public ConvertExpressionIntoCriterion(Projection<T> projection)
        {
            _projection = projection;
        }

        public void AddAliases(Action<List<ProjectionAlias>> action )
        {
            action(_whereAliases);
        }

        public ICriterion Convert(Expression exp)
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

                if(exp.NodeType == ExpressionType.Constant)
                {
                    return ConvertConstant(exp);
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

        private ICriterion ConvertConstant(Expression exp)
        {
            var consa = (ConstantExpression) exp;

            return global::NHibernate.Criterion.Expression.Eq(Projections.Constant(consa.Value), true);
        }

        private ICriterion ConvertEqual(Expression exp)
        {
            var a = (BinaryExpression) exp;
            
            var left = (MemberExpression) a.Left;
            var right = (ConstantExpression)a.Right;

            //need to walk the '.'s

            var bits = getPath(left);
            var accessor = ReflectionHelper.GetAccessor(left);
            ProjectionAlias.For(accessor, false).Each(x => _whereAliases.Fill(x));
            
            return global::NHibernate.Criterion.Expression.Eq(bits, right.Value);

        }

        //TODO: to fubucore?
        private string getPath(MemberExpression exp)
        {
            var stack = new Stack<string>();
            var exp1 = exp;

            while (exp1 != null)
            {
                stack.Push(exp1.Member.Name);
                exp1 = exp1.Expression as MemberExpression;
            }

            return stack.Aggregate((l,r)=>l+"."+r);
        }

        public ICriterion ConvertOrElse(Expression exp)
        {
            var b = (BinaryExpression) exp;

            var left = Convert(b.Left);
            var right = Convert(b.Right);

            return global::NHibernate.Criterion.Expression.Or(left, right);
        }

        public ICriterion ConvertCall(Expression exp)
        {
            var call = (MethodCallExpression) exp;
            var collectionType = call.Arguments.Skip(1).First().Type;
            var subCrit = DetachedCriteria.For(collectionType);
            subCrit.SetProjection(Projections.Id());

            var arg = (MemberExpression)call.Arguments.Skip(1).First();
           
            var propertyToCheck = arg.Member.Name;;

            return Subqueries.PropertyIn(propertyToCheck + ".Id", subCrit);
        }

        public ICriterion ConvertLambda(Expression exp)
        {
            var lambda = (LambdaExpression) exp;
            return Convert(lambda.Body);
        }
    }
}