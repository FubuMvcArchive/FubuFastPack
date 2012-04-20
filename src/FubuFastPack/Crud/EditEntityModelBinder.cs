using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuCore.Binding;
using FubuFastPack.Domain;
using FubuFastPack.Validation;

namespace FubuFastPack.Crud
{
    public interface IEntityDefaults
    {
        void ApplyDefaultsToNewEntity(DomainEntity entity);
    }

    public class NulloEntityDefaults : IEntityDefaults
    {
        public void ApplyDefaultsToNewEntity(DomainEntity entity)
        {
            // nothing
        }
    }

    // TODO:  Get some integration tests around this monster.  It's well covered in Dovetail
    // tests, but still...
    public class EditEntityModelBinder : IModelBinder
    {
        private readonly IEntityDefaults _entityDefaults;
        private readonly IObjectResolver _resolver;
        private readonly IServiceLocator _locator;

        public EditEntityModelBinder(IEntityDefaults entityDefaults, IObjectResolver resolver, IServiceLocator locator)
        {
            _entityDefaults = entityDefaults;
            
            _resolver = resolver;
            _locator = locator;
        }

        public bool Matches(Type type)
        {
            return type.CanBeCastTo<EditEntityModel>();
        }

        public void Bind(Type type, object instance, IBindingContext context)
        {
            throw new NotImplementedException();
        }

        public object Bind(Type inputModelType, IBindingContext context)
        {
            //we determine the type by sniffing the ctor arg
            var entityType = inputModelType
                .GetConstructors()
                .Single(x => x.GetParameters().Count() == 1)
                .GetParameters()
                .Single()
                .ParameterType;
           
            var entity = tryFindExistingEntity(entityType, context) 
                ?? createNewEntity(entityType, context);

            var model = (EditEntityModel)Activator.CreateInstance(inputModelType, entity);


            _resolver.BindProperties(inputModelType, model, context);

            // Get the binding errors from conversion of the EditEntityModel
            context.Problems.Each(x =>
            {
                model.Notification.RegisterMessage(x.Property, FastPackKeys.PARSE_VALUE);
            });

            return model;
        }

        private DomainEntity tryFindExistingEntity(Type entityType, IBindingContext context)
        {
            DomainEntity entity = null;

            context.Data.ValueAs(entityType, "Id", o =>
            {
                entity = (DomainEntity) o;

                //TODO: this should be on context
                var childData = context.GetSubRequest(entityType.Name);
                var c = new BindingContext(childData, _locator, context.Logger);
                //TODO: End fail

                
                _resolver.BindProperties(entityType, entity, c);

                //TODO: I have to move the 'problems' forward - because I used a new context object
                c.Problems.Each(p =>
                {
                    context.Problems.Add(p);
                });
            });

            return entity;
        }

        private DomainEntity createNewEntity(Type entityType, IBindingContext context)
        {
            //TODO: this should be on context
            var childData = context.GetSubRequest(entityType.Name);
            var c = new BindingContext(childData, _locator, context.Logger);
            //TODO: End fail

            var result = _resolver.BindModel(entityType, c);
            var entity = (DomainEntity) result.Value;
            entity.Id = Guid.Empty;
            _entityDefaults.ApplyDefaultsToNewEntity(entity);

            //TODO: I have to move the 'problems' forward - because I used a new context object
            c.Problems.Each(p =>
            {
                context.Problems.Add(p);
            });

            return entity;
        }
    }
}