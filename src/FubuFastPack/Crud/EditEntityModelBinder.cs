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


    public class EditEntityModelBinder : IModelBinder
    {
        private readonly IEntityDefaults _entityDefaults;
       

        public EditEntityModelBinder(IEntityDefaults entityDefaults)
        {
            _entityDefaults = entityDefaults;
            
         
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

            context.BindProperties(model);

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
                if (o == null) return;

                entity = (DomainEntity) o;

                var c = context.GetSubContext(entityType.Name);
                c.BindProperties(entity);

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
            var c = context.GetSubContext(entityType.Name);

            object result = null;
            c.BindObject(entityType, o=>
                                                      {
                                                          c.BindProperties(o);
                                                          result = o;
                                                      });

            var entity = (DomainEntity) result;
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