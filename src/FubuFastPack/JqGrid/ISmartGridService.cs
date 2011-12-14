using System;
using FubuFastPack.Domain;
using FubuFastPack.Querying;

namespace FubuFastPack.JqGrid
{
    public interface ISmartGridService
    {
        GridCounts GetCounts<TGrid, TInput>(params object[] args)
            where TGrid : ISmartGrid
            where TInput : NamedGridRequest, new();

        GridCounts GetCounts<TInput>(string gridName, params object[] args) where TInput : NamedGridRequest, new();
        GridViewModel GetModel(NamedGridRequest request);
        string QuerystringFor(string gridName, params object[] args);

        Type EntityTypeForGrid(string gridName);

        GridState StateForGrid<TGrid>(params object[] args) where TGrid : ISmartGrid;

        int RecordCountFor<TGrid, TEntity>(IDataRestriction<TEntity> restriction, params object[] arguments) 
            where TEntity : DomainEntity
            where TGrid : ISmartGrid<TEntity>;

        int RecordCountFor<TGrid>(params object[] arguments) where TGrid : ISmartGrid;

        string GetUrl<TInput, TGrid>(params object[] args) where TInput : NamedGridRequest, new() where TGrid : ISmartGrid;

        Guid IdOfFirstResult<TGrid>(params object[] args) where TGrid : ISmartGrid;
    }
}