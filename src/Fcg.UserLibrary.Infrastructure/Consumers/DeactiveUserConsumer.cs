using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.UserLibrary.Application.Features.Commands.UserDeactived;
using MassTransit;
using MediatR;

namespace Fcg.UserLibrary.Infrastructure.Consumers
{
    public class DeactiveUserConsumer : IConsumer<IUserDeactivatedIntegrationEvent>
    {
        private readonly IMediator _mediator;

        public DeactiveUserConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<IUserDeactivatedIntegrationEvent> context)
        {
            var message = context.Message;
            var libraryDeactiveCommand = new LibraryDeactiveCommand(message.EventId,message.CreatedAt,
                message.UserId);
            await _mediator.Send(libraryDeactiveCommand, context.CancellationToken);
        }
    }
}
