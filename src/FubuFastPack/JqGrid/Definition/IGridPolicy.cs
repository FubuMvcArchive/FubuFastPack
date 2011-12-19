using FubuFastPack.Domain;

namespace FubuFastPack.JqGrid
{
    public interface IGridPolicy
    {

        /// <summary>
        /// This represents the definition of the grid in jqGrid
        /// </summary>
        void AlterDefinition<T>(GridDefinition<T> definition) where T : DomainEntity;


        /// <summary>
        /// This represents the data coming down to the grid
        /// </summary>
        /// <param name="grid"></param>
        void AlterGrid(ISmartGrid grid);
    }
}