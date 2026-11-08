using Fcg.Core.SharedContracts.Interfaces;
using MassTransit;

namespace Fcg.UserLibrary.Infrastructure.MessageBroker
{
    internal class MassTransitIntegrationEventPublisher : IIntegrationEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task PublishAsync<T>(object integrationEvent, CancellationToken cancellationToken = default) where T : class
            => _publishEndpoint.Publish<T>(integrationEvent, cancellationToken);
    }
}
