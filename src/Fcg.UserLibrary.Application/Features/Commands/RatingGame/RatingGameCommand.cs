using MediatR;

namespace Fcg.UserLibrary.Application.Features.Commands.RatingGame
{
    /// <summary>Corpo da requisição: o jogo vem da rota e o usuário do token.</summary>
    public record RatingGameRequest(double Rating);

    public record RatingGameCommand(Guid UserId, Guid GameId, double Rating) : IRequest<RatingGameResult>;

    public enum RatingGameResult
    {
        Rated,
        /// <summary>O jogo não está na biblioteca do usuário (o endpoint responde 404).</summary>
        NotInLibrary
    }
}
