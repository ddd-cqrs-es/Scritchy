﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject.Web.Mvc;
using Ninject;
using Example.Infrastructure;
using Example.Domain.Readmodel;
using Example.Web.Controllers;
using Scritchy.Infrastructure;
using Scritchy.Infrastructure.Implementations;

namespace Example.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : NinjectHttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Bus", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected override void OnApplicationStarted()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);
        }

        protected override IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ICommandBus>().To<CommandBus>().InSingletonScope(); ;
            kernel.Bind<IEventApplier>().To<EventApplier>().InSingletonScope();
            kernel.Bind<IEventStore>().To<InMemoryEventStore>().InSingletonScope();
            kernel.Bind<IHandlerInstanceResolver>().ToMethod(c => new HandlerInstanceResolver(
                c.Kernel.Get<IEventStore>(),
                c.Kernel.Get<HandlerRegistry>(),
                t=>c.Kernel.Get(t))).InSingletonScope();
            kernel.Bind<HandlerRegistry>().To<ExampleRegistry>().InSingletonScope();
            kernel.Bind<StockDictionaryHandler>().ToSelf().InSingletonScope();
            kernel.Bind<StockDictionary>().ToConstant(new StockDictionary());
            kernel.Bind<BusController.FailedCommandExceptionList>().ToSelf().InSingletonScope();
            return kernel;
        }

    }
}