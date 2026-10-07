using MediatR;

namespace Fcg.UserLibrary.Application.IntegrationEvent.Handlers.CreatedGame
{
    public record GameCreatedCommand(Guid EventId, DateTime CreatedAt,Guid GameId, string Name, string Description, string Genre, DateTime OccurredAt) : IRequest;
}
