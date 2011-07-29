using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Hql.Ast;
using NHibernate.Linq;
using NHibernate.Linq.Functions;
using NHibernate.Linq.Visitors;

namespace FubuFastPack.NHibernate
{
    public class ExtendedLinqToHqlGeneratorRegistry : DefaultLinqToHqlGeneratorsRegistry
    {
        public ExtendedLinqToHqlGeneratorRegistry()
        {
            this.Merge(new EqualsIgnoreCaseStringGenerator());
        }
    }

    public class EqualsIgnoreCaseStringGenerator : BaseHqlGeneratorForMethod
    {
        public EqualsIgnoreCaseStringGenerator()
        {
            SupportedMethods = new[]
            {ReflectionHelper.GetMethodDefinition<string>(x => x.Equals(null, StringComparison.CurrentCulture))};
        }

        public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObject, ReadOnlyCollection<Expression> arguments, HqlTreeBuilder treeBuilder, IHqlExpressionVisitor visitor)
        {
            var comparison = (StringComparison)(arguments[1].As<ConstantExpression>().Value);
            if (comparison == StringComparison.CurrentCultureIgnoreCase ||
                comparison == StringComparison.InvariantCultureIgnoreCase ||
                comparison == StringComparison.OrdinalIgnoreCase)
            {
                return treeBuilder.Equality(
                    treeBuilder.MethodCall("lower", new[] { visitor.Visit(targetObject).AsExpression() }),
                    treeBuilder.MethodCall("lower", new[] { visitor.Visit(arguments[0]).AsExpression() }));
            }
            return treeBuilder.Equality(
                visitor.Visit(targetObject).AsExpression(),
                visitor.Visit(arguments[0]).AsExpression());
        }
    }
}