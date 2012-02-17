using System;
using FubuCore.Binding;
using FubuMVC.Core;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;
using FubuMVC.StructureMap;
using StructureMap;

namespace FubuFastPack.StructureMap
{
    public class TransactionalStructureMapContainerFacility : StructureMapContainerFacility
    {
        private readonly IContainer _container;

        public TransactionalStructureMapContainerFacility(IContainer container) : base(container)
        {
            _container = container;
        }

        public override IActionBehavior BuildBehavior(ServiceArguments arguments, Guid behaviorId)
        {
            return new TransactionalContainerBehavior(_container, arguments, behaviorId);
        }
    }

    
    public class TransactionalContainerBehavior : IActionBehavior
    {
        private readonly ServiceArguments _arguments;
        private readonly Guid _behaviorId;
        private readonly IContainer _container;

        public TransactionalContainerBehavior(IContainer container, ServiceArguments arguments, Guid behaviorId)
        {
            _container = container;
            _arguments = arguments;
            _behaviorId = behaviorId;
        }

        public void Invoke()
        {
            using (var nested = _container.GetNestedContainer())
            {
                nested.Configure(cfg => _arguments.EachService((type, value) => cfg.For(type).Use(value)));
                
                var request = nested.GetInstance<IFubuRequest>().Get<CurrentRequest>();

                if (request.Url.StartsWith("/_content"))
                {
                    nested.GetInstance<IActionBehavior>(_arguments.ToExplicitArgs(),_behaviorId.ToString()).Invoke();
                    return;
                }

                nested.ExecuteInTransactionWithoutNestedContainer<IContainer>(invokeRequestedBehavior);
            }
          
        }

        public void InvokePartial()
        {
            // this should never be called
            throw new NotSupportedException();
        }

        private void invokeRequestedBehavior(IContainer c)
        {
            var behavior = c.GetInstance<IActionBehavior>(_arguments.ToExplicitArgs(), _behaviorId.ToString());
            behavior.Invoke();
        }
    }
}