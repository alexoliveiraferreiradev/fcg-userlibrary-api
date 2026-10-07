using Fcg.Core.WebApi.MessageBroker;
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
            services.AddOptions<UserLibraryServiceRabbitMqSettings>().BindConfiguration(UserLibraryServiceRabbitMqSettings.SectionName)
           .ValidateDataAnnotations().ValidateOnStart();

            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(GameCreatedConsumer).Assembly);
                x.AddEntityFrameworkOutbox<LibraryDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqConfig = context.GetRequiredService<IOptions<UserLibraryServiceRabbitMqSettings>>().Value;

                    cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.Port, "/", h =>
                    {
                        h.Username(rabbitMqConfig.Username);
                        h.Password(rabbitMqConfig.Password);
                    });

                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                    cfg.ReceiveEndpoint(rabbitMqConfig.UserLibraryIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<LibraryDbContext>(context);
                        e.ConfigureConsumer<GameUserLibraryConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.UserLibraryGameCreatedIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<LibraryDbContext>(context);
                        e.ConfigureConsumer<GameCreatedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.DeactiveUserIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<LibraryDbContext>(context);
                        e.ConfigureConsumer<DeactiveUserConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.ReactiveUserIntegrationQueue, e =>
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
