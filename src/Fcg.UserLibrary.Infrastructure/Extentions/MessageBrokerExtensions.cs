using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.UserLibrary.Infrastructure.Consumers;
using Fcg.UserLibrary.Infrastructure.MessageBroker;
using Fcg.UserLibrary.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fcg.UserLibrary.API.Extensions
{
    internal static class MessageBrokerExtensions
    {
        public static IServiceCollection AddMassTransitExtension(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<AzureServiceBusSettings>().BindConfiguration(AzureServiceBusSettings.SectionName)
           .ValidateDataAnnotations().ValidateOnStart();

            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(GameCreatedConsumer).Assembly);
                x.AddEntityFrameworkOutbox<LibraryDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });
                x.UsingAzureServiceBus((context, cfg) =>
                {
                    var serviceBusConfig = context.GetRequiredService<IOptions<AzureServiceBusSettings>>().Value;
                    cfg.Host(serviceBusConfig.ConnectionString);

                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                    cfg.Message<IGameUserLibraryIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.GameUserLibrary));
                    cfg.Message<IGameCreatedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.GameCreated));
                    cfg.Message<IUserDeactivatedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.UserDeactivated));
                    cfg.Message<IUserReactivatedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.UserReactivated));

                    cfg.ReceiveEndpoint(serviceBusConfig.UserLibraryIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<LibraryDbContext>(context);
                        e.ConfigureConsumer<GameUserLibraryConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(serviceBusConfig.UserLibraryGameCreatedIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<LibraryDbContext>(context);
                        e.ConfigureConsumer<GameCreatedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(serviceBusConfig.DeactiveUserIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<LibraryDbContext>(context);
                        e.ConfigureConsumer<DeactiveUserConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(serviceBusConfig.ReactiveUserIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<LibraryDbContext>(context);
                        e.ConfigureConsumer<ReactiveUserConsumer>(context);
                    });

                });
            });

            return services;
        }
    }
}
