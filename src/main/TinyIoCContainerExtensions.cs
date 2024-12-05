using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Routing;
using ei8.EventSourcing.Client;
using Nancy.TinyIoc;
using neurUL.Common.Http;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Linq;
using System.Net.Http;

namespace ei8.Extensions.DependencyInjection
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
            container.Register<ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.ExternalReference.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.ExternalReference.Application.ItemCommandHandlers(
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
            // external reference
            registrar.Register(typeof(ei8.Data.ExternalReference.Application.ItemCommandHandlers));

            ((TinyIoCServiceLocator)container.Resolve<IServiceProvider>()).SetRequestContainer(container);
        }

        public static void AddReaders(this TinyIoCContainer container)
        {
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.IAggregateParser,
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.AggregateParser
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.IExpressionReader,
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.ExpressionReader
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.IInstantiatesClassReader,
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.InstantiatesClassReader
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.IPropertyAssignmentReader,
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.PropertyAssignmentReader
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.IPropertyAssociationReader,
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.PropertyAssociationReader
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.IPropertyValueExpressionReader,
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.PropertyValueExpressionReader
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.IUnitReader,
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.UnitReader
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.IValueReader,
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.ValueReader
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.IValueExpressionReader,
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.ValueExpressionReader
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.IInstanceReader,
                Cortex.Coding.d23.neurULization.Processors.Readers.Inductive.InstanceReader
            >();
        }

        public static void AddRequestProvider(this TinyIoCContainer container)
        {
            var rp = new RequestProvider();
            rp.SetHttpClientHandler(new HttpClientHandler());
            container.Register<IRequestProvider>(rp);
        }

        public static void AddTransactions(this TinyIoCContainer container, string eventSourcingInBaseUrl, string eventSourcingOutBaseUrl)
        {
            container.Register<IEventStoreUrlService>(new EventStoreUrlService(eventSourcingInBaseUrl, eventSourcingOutBaseUrl));
            container.Register<IEventSerializer, EventSerializer>();

            container.Register<IAuthoredEventStore, HttpEventStoreClient>();
            container.Register<IInMemoryAuthoredEventStore, InMemoryEventStore>();
            container.Register<IRepository>((tic, npo) => new Repository(container.Resolve<IInMemoryAuthoredEventStore>()));
            container.Register<CQRSlite.Domain.ISession, CQRSlite.Domain.Session>();
            container.Register<ITransaction, EventSourcing.Client.Transaction>();
        }

        public static void AddWriters(this TinyIoCContainer container)
        {
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Writers.IExpressionWriter,
                Cortex.Coding.d23.neurULization.Processors.Writers.ExpressionWriter
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Writers.IInstantiatesClassWriter,
                Cortex.Coding.d23.neurULization.Processors.Writers.InstantiatesClassWriter
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Writers.IPropertyAssignmentWriter,
                Cortex.Coding.d23.neurULization.Processors.Writers.PropertyAssignmentWriter
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Writers.IPropertyAssociationWriter,
                Cortex.Coding.d23.neurULization.Processors.Writers.PropertyAssociationWriter
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Writers.IPropertyValueExpressionWriter,
                Cortex.Coding.d23.neurULization.Processors.Writers.PropertyValueExpressionWriter
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Writers.IUnitWriter,
                Cortex.Coding.d23.neurULization.Processors.Writers.UnitWriter
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Writers.IValueWriter,
                Cortex.Coding.d23.neurULization.Processors.Writers.ValueWriter
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Writers.IValueExpressionWriter,
                Cortex.Coding.d23.neurULization.Processors.Writers.ValueExpressionWriter
            >();
            container.Register<
                Cortex.Coding.d23.neurULization.Processors.Writers.IInstanceWriter,
                Cortex.Coding.d23.neurULization.Processors.Writers.InstanceWriter
            >();
        }
    }
}
