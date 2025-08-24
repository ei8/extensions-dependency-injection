using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Routing;
using ei8.EventSourcing.Client;
using Nancy.TinyIoc;
using neurUL.Common.Http;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Linq;

namespace ei8.Extensions.DependencyInjection.Cortex
{
    public static class TinyIoCContainerExtensions
    {
        public static void AddDataAdapters(
            this TinyIoCContainer container,
            params Type[] cancellableCommandHandlers
        )
        {
            // create a singleton instance which will be reused for all calls in current request
            var ipb = new Router();
            container.Register<ICommandSender, Router>(ipb);
            container.Register<IHandlerRegistrar, Router>(ipb);

            #region Neuron
            // neuron
            container.Register<INeuronAdapter, NeuronAdapter>();
            container.Register((tic, npo) => new neurUL.Cortex.Application.Neurons.NeuronCommandHandlers(
                container.Resolve<IInMemoryAuthoredEventStore>(),
                container.Resolve<ISession>()
                ));
            // tag
            container.Register<ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.Tag.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.Tag.Application.ItemCommandHandlers(
                container.Resolve<IInMemoryAuthoredEventStore>(),
                container.Resolve<ISession>()
                ));
            // aggregate
            container.Register<ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.Aggregate.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.Aggregate.Application.ItemCommandHandlers(
                container.Resolve<IInMemoryAuthoredEventStore>(),
                container.Resolve<ISession>()
                ));
            // external reference
            container.Register<ei8.Data.Mirror.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.Mirror.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.Mirror.Application.ItemCommandHandlers(
                container.Resolve<IInMemoryAuthoredEventStore>(),
                container.Resolve<ISession>()
                ));
            #endregion

            #region Terminal
            container.Register<ITerminalAdapter, TerminalAdapter>();
            container.Register((tic, npo) => new neurUL.Cortex.Application.Neurons.TerminalCommandHandlers(
                container.Resolve<IInMemoryAuthoredEventStore>(),
                container.Resolve<ISession>()
                ));
            #endregion

            var ticl = new TinyIoCServiceLocator(container);
            container.Register<IServiceProvider, TinyIoCServiceLocator>(ticl);
            var registrar = new RouteRegistrar(ticl);
            cancellableCommandHandlers.ToList().ForEach(cch => registrar.Register(cch));
            // neuron - only one type from an assembly is needed to register all handlers
            registrar.Register(typeof(neurUL.Cortex.Application.Neurons.NeuronCommandHandlers));
            // tag
            registrar.Register(typeof(ei8.Data.Tag.Application.ItemCommandHandlers));
            // aggregate
            registrar.Register(typeof(ei8.Data.Aggregate.Application.ItemCommandHandlers));
            // mirror
            registrar.Register(typeof(ei8.Data.Mirror.Application.ItemCommandHandlers));

            ((TinyIoCServiceLocator)container.Resolve<IServiceProvider>()).SetRequestContainer(container);
        }
    }
}
