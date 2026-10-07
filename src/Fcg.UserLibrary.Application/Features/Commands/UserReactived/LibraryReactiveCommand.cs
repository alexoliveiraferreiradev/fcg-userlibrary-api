using MediatR;

namespace Fcg.UserLibrary.Application.Features.Commands.UserReactived
{
    public record LibraryReactiveCommand(Guid EventId, DateTime CreatedAt,Guid UserId) : IRequest;
}
