using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuCore;
using FubuFastPack.Querying;

namespace FubuFastPack.JqGrid
{
    /// <summary>
    /// This is the input model for getting a grid data
    /// </summary>
    [DebuggerDisplay("{debug()}")]
    public class GridRequest<TGrid> where TGrid : ISmartGrid
    {
        public GridRequest()
        {
            page = 1;
            rows = 10;
        }

        public int page { get; set; }
        public int rows { get; set; }
        public string sidx { get; set; }
        public string sord { get; set; }
// ReSharper disable InconsistentNaming
        public string _search { get; set; }
// ReSharper restore InconsistentNaming
        public string nd { get; set; }

        public IList<Criteria> criterion { get; set; }
        public string gridName { get; set; }

        // TODO -- put a UT around this.

        public GridDataRequest ToDataRequest()
        {
            var sortAscending = !"desc".Equals(sord, StringComparison.OrdinalIgnoreCase);
            return new GridDataRequest(page, rows, sidx, sortAscending){
                Criterion = criterion == null ? new Criteria[0] : criterion.ToArray()
            };
        }

        string debug()
        {
            return "Grid for '{0}'".ToFormat(typeof(TGrid).Name);
        }
    }
}