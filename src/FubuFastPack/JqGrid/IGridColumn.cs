using System;
using System.Collections.Generic;
using FubuCore;
using FubuCore.Reflection;
using FubuFastPack.Querying;
using FubuMVC.Core.Urls;

namespace FubuFastPack.JqGrid
{
    public interface IGridColumn
    {
        IEnumerable<IDictionary<string, object>> ToDictionary();
        Action<EntityDTO> CreateDtoFiller(IGridData data, IDisplayFormatter formatter, IUrlRegistry urls);

        /// <summary>
        /// The primary accessor used by this column. Exposed to allow security checks.
        /// </summary>
        Accessor Accessor { get; }

        
        IEnumerable<Accessor> SelectAccessors();
        IEnumerable<Accessor> AllAccessors();

        IEnumerable<FilteredProperty> FilteredProperties();
        IEnumerable<string> Headers();

        string GetHeader();

        bool IsOuterJoin { get; set; }
    }
}