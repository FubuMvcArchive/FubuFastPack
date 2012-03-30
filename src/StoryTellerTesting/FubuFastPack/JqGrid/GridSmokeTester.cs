using System.Diagnostics;
using FubuFastPack.JqGrid;
using FubuMVC.Core.Http;
using FubuTestApplication;
using FubuTestApplication.Grids;
using NUnit.Framework;
using FubuFastPack.StructureMap;

namespace IntegrationTesting.FubuFastPack.JqGrid
{
    [TestFixture]
    public class GridSmokeTester
    {
        [Test]
        public void try_to_build_out_the_test_harness_case_grid()
        {
            var container = DatabaseDriver.GetFullFastPackContainer();
            container.Configure(x => x.For<ICurrentHttpRequest>().Use<FakeCurrentHttpRequest>());
            container.ExecuteInTransaction<CaseController>(x =>
            {
                Debug.WriteLine(x.AllCases().ToString());
            });

            
        }
    }
    public class FakeCurrentHttpRequest : ICurrentHttpRequest
    {
        public string RawUrl()
        {
            throw new System.NotImplementedException();
        }

        public string RelativeUrl()
        {
            throw new System.NotImplementedException();
        }

        public string FullUrl()
        {
            throw new System.NotImplementedException();
        }

        public string ToFullUrl(string url)
        {
            return url;
        }

        public string HttpMethod()
        {
            throw new System.NotImplementedException();
        }
    }
}