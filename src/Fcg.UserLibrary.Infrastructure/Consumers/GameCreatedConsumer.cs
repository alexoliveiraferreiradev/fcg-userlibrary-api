using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.UserLibrary.Application.IntegrationEvent.Handlers.CreatedGame;
using MassTransit;
using MediatR;

namespace Fcg.UserLibrary.Infrastructure.Consumers
{
    public class GameCreatedConsumer : IConsumer<IGameCreatedIntegrationEvent>
    {
        private readonly ISender _sender;
        public GameCreatedConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<IGameCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            var gameCommand = new GameCreatedCommand(message.EventId,message.CreatedAt,message.GameId,
                message.Name, message.Description, message.Genre, message.OccurredAt);
            await _sender.Send(gameCommand, context.CancellationToken);
        }
    }
}
