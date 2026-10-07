using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.UserLibrary.Application.Features.Commands.OrderFinalize;
using MassTransit;
using MediatR;

namespace Fcg.UserLibrary.Infrastructure.Consumers
{
    public class GameUserLibraryConsumer : IConsumer<IGameUserLibraryIntegrationEvent>
    {
        private readonly IMediator _mediator;

        public GameUserLibraryConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<IGameUserLibraryIntegrationEvent> context)
        {
            var message = context.Message;
            var userLibraryCommand = new LibraryOrderCommand(message.EventId,message.CreatedAt,message.UserId,message.GameId,
                message.Name,message.IsAvaiable,message.Description,message.Genre,message.OccurredAt);
           
            await _mediator.Send(userLibraryCommand, context.CancellationToken);
        }
    }
}
