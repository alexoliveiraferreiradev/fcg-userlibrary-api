using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.UserLibrary.Application.Features.Commands.UserReactived;
using MassTransit;
using MediatR;

namespace Fcg.UserLibrary.Infrastructure.Consumers
{
    public class ReactiveUserConsumer : IConsumer<IUserReactivatedIntegrationEvent>
    {
        private readonly IMediator _mediator;

        public ReactiveUserConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<IUserReactivatedIntegrationEvent> context)
        {
            var message = context.Message;
            var reactiveCommand = new LibraryReactiveCommand(message.EventId,message.CreatedAt,message.UserId);
            await _mediator.Send(reactiveCommand, context.CancellationToken);
        }
    }
}
