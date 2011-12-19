using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FubuCore;
using FubuCore.Reflection;
using FubuMVC.Core.Urls;

namespace FubuFastPack.JqGrid
{
    public class FilterColumn<T> : GridColumnBase<T, FilterColumn<T>>
    {
        public FilterColumn(Expression<Func<T, object>> expression) : base(expression)
        {
            IsFilterable = true;
        }

        public override IEnumerable<IDictionary<string, object>> ToDictionary()
        {
            yield break;
        }

        public override Action<EntityDTO> CreateDtoFiller(IGridData data, IDisplayFormatter formatter, IUrlRegistry urls)
        {
            return dto => { };
        }

        public override IEnumerable<Accessor> SelectAccessors()
        {
            yield break;
        }

        public override IEnumerable<Accessor> AllAccessors()
        {
            yield return Accessor;
        }

        public override IEnumerable<string> Headers()
        {
            yield break;
        }
    }
}