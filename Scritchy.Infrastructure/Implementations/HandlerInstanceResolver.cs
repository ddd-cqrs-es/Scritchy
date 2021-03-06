﻿using System;
using Scritchy.Domain;

namespace Scritchy.Infrastructure.Implementations
{
    public class HandlerInstanceResolver : IHandlerInstanceResolver
    {
        IEventStore eventsource;
        HandlerRegistry handlerregistry;
        Func<Type, object> LoadHandler;

        public HandlerInstanceResolver(IEventStore eventsource,HandlerRegistry handlerregistry,Func<Type,object> LoadHandler)
        {
            this.eventsource = eventsource;
            this.handlerregistry = handlerregistry;
            this.LoadHandler = LoadHandler;
        }

        public void ApplyEventsToInstance(object instance,IParameterResolver pr)
        {
            var instancetype = instance.GetType();
            foreach (var evt in eventsource.GetNewEvents(instance))
            {
                if(this.handlerregistry.ContainsHandler(instancetype,evt.GetType()))
                    this.handlerregistry[instancetype, evt.GetType()](instance, evt, pr);
            }
        }


        public AR LoadARSnapshot(Type t, string Id,IParameterResolver pr)
        {
            var ar = Activator.CreateInstance(t) as AR;
            ar.Id = Id;
            ar.TryApplyEvent = x => {
                if (handlerregistry.ContainsHandler(t, x.GetType()))
                    handlerregistry[t, x.GetType()](ar, x,pr);
            };
            ApplyEventsToInstance(ar,pr);
            return ar;
        }

        public object ResolveHandlerFromType(Type t)
        {
            return LoadHandler(t);
        }
    }
}
