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
            _container.Configure(cfg =>
            {
                _arguments.EachService((type, value) =>
                {
                    cfg.For(type).Use(value);
                });
            });
            var request = _container.GetInstance<IFubuRequest>().Get<CurrentRequest>();

            if(request.Url.StartsWith("/_content"))
            {
               
                var behavior = _container.GetInstance<IActionBehavior>(_arguments.ToExplicitArgs(), _behaviorId.ToString());
                behavior.Invoke();
                return;
            }

            _container.ExecuteInTransaction<IContainer>(invokeRequestedBehavior);
        }

        public void InvokePartial()
        {
            // Just go straight to the inner behavior here.  Assuming that the transaction & principal
            // are already set up
            invokeRequestedBehavior(_container);
        }

        private void invokeRequestedBehavior(IContainer c)
        {
            c.Configure(cfg =>
            {
                _arguments.EachService((type, value) =>
                {
                    cfg.For(type).Use(value);
                });
            });
            
            var behavior = c.GetInstance<IActionBehavior>(_arguments.ToExplicitArgs(), _behaviorId.ToString());
            behavior.Invoke();
        }
    }
}