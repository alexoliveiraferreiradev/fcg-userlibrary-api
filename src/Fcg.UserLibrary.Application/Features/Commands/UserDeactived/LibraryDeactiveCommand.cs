using MediatR;

namespace Fcg.UserLibrary.Application.Features.Commands.UserDeactived
{
    public record LibraryDeactiveCommand(Guid EventId, DateTime CreatedAt,Guid UserId): IRequest;
}
