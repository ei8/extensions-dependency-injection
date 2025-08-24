using CQRSlite.Domain;
using ei8.EventSourcing.Application;
using ei8.EventSourcing.Application.EventStores;
using ei8.EventSourcing.Client;
using ei8.EventSourcing.Domain.Model;
using ei8.EventSourcing.Port.Adapter.IO.Persistence.Events.SQLite;
using ei8.EventSourcing.Port.Adapter.IO.Process.Services;
using Nancy.TinyIoc;

namespace ei8.Extensions.DependencyInjection.EventSourcing
{
    public static class TinyIoCContainerExtensions
    {
        public static void AddTransactions(this TinyIoCContainer container, string eventSourcingInBaseUrl, string eventSourcingOutBaseUrl)
        {
            container.Register<IEventStoreUrlService>(new EventStoreUrlService(eventSourcingInBaseUrl, eventSourcingOutBaseUrl));
            container.Register<IEventSerializer, EventSerializer>();

            container.Register<IAuthoredEventStore, HttpEventStoreClient>();
            container.Register<IInMemoryAuthoredEventStore, InMemoryEventStore>();
            container.Register<IRepository>((tic, npo) => new Repository(container.Resolve<IInMemoryAuthoredEventStore>()));
            container.Register<ISession, Session>();
            container.Register<ITransaction, Transaction>();
        }

        public static void AddInProcessTransactions(this TinyIoCContainer container)
        {
            container.Register<ISettingsService, InProcessSettingsService>();
            container.Register<IEventStore, EventStore>();
            container.Register<IEventStoreApplicationService, EventStoreApplicationService>();
            container.Register<
                ei8.EventSourcing.Port.Adapter.In.InProcess.IEventAdapter,
                ei8.EventSourcing.Port.Adapter.In.InProcess.EventAdapter
            >();
            container.Register<
                ei8.EventSourcing.Port.Adapter.Out.InProcess.IEventAdapter,
                ei8.EventSourcing.Port.Adapter.Out.InProcess.EventAdapter
            >();
            container.Register<IEventSerializer, EventSerializer>();
            container.Register<IAuthoredEventStore, InProcessEventStore>();
            container.Register<IInMemoryAuthoredEventStore, InMemoryEventStore>();
            container.Register<IRepository>(
                (tic, npo) => new Repository(container.Resolve<IInMemoryAuthoredEventStore>())
            );
            container.Register<ISession, Session>();
            container.Register<ITransaction, Transaction>();
        }
    }
}
