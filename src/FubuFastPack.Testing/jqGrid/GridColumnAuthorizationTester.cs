using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FubuCore;
using FubuCore.Binding;
using FubuCore.Formatting;
using FubuCore.Reflection;
using FubuFastPack.JqGrid;
using FubuFastPack.Querying;
using FubuFastPack.Testing.Security;
using FubuMVC.Core;
using FubuMVC.Core.Urls;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuFastPack.Testing.jqGrid
{
    [TestFixture]
    public class GridColumnAuthorizationTester
    {
        private IEndpointService endpointService = null;
        IQueryService queryService = null;
        ISmartRequest request = null;
        IServiceLocator services = null;

        [SetUp]
        public void BuildUpGrid()
        {
            
        }

        [Test]
        public void WithNoPolicy()
        {
            var sgh = new SmartGridHarness<AuthGrid>(services, endpointService, queryService, request, new IGridPolicy[0]);

            var grid = sgh.BuildGrid();
            grid.Definition.Columns.Count().ShouldEqual(3);
        }

        [Test]
        public void WithPolicy()
        {
            var policy = new AuthorizationGridPolicy(services);
            var sgh = new SmartGridHarness<AuthGrid>(services, endpointService, queryService, request, new IGridPolicy[]{policy});

            var grid = sgh.BuildGrid();
            grid.Definition.Columns.Count().ShouldEqual(2);
        }
    }

    public class AuthGrid : Grid<Case, object>
    {
        public AuthGrid()
        {
            Show(x => x.Id);
            AddColumn(new SecuredColumn(c => c.Condition));
        }
        public override IGridDataSource<Case> BuildSource(object service)
        {
            return null;
        }
    }

    public class SecuredColumn : GridColumnBase<Case, string>
    {
        public SecuredColumn(Expression<Func<Case, object>> expression) : base(expression)
        {

        }

        public SecuredColumn(Accessor accessor) : base(accessor)
        {
        }

        public override IEnumerable<IDictionary<string, object>> ToDictionary()
        {
            yield return new Dictionary<string, object>{
                {"name", "data"},
                {"index", "data"},
                {"sortable", false},
                {"hidden", false},
                {"secured",true}
            };
        }

        public override Action<EntityDTO> CreateDtoFiller(IGridData data, IDisplayFormatter formatter, IUrlRegistry urls)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Accessor> SelectAccessors()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Accessor> AllAccessors()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> Headers()
        {
            throw new NotImplementedException();
        }

        public override ColumnAuthorizationAction ApplyAuthorization(IServiceLocator services)
        {
            return ColumnAuthorizationAction.RemoveColumn;
        }
    }
}