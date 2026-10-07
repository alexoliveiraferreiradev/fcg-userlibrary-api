using Fcg.UserLibrary.Application.Features.Response;
using MediatR;

namespace Fcg.UserLibrary.Application.Features.Commands.OrderFinalize
{
    public record LibraryOrderCommand(Guid EventId, DateTime CreatedAt, Guid UserId,Guid GameId, string NameGame, bool IsAvaiable, 
        string DescriptionGame, string GenreGame, DateTime AcquiredAt) : IRequest<UserLibraryItemResponse>;
}
