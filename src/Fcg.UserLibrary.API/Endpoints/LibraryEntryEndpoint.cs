using Fcg.UserLibrary.Application.Features.Commands.RatingGame;
using Fcg.UserLibrary.Application.Features.Queries.GetPagedLibrary;
using Fcg.UserLibrary.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fcg.UserLibrary.API.Endpoints
{
    public static class LibraryEntryEndpoint
    {
        public static void MapLibraryUserEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/library").RequireAuthorization("PlayersOnly").WithTags("Biblioteca do Usuário");

            group.MapPut("/{gameId:guid}/rating", ApplyRatingGame)
                .Produces(StatusCodes.Status204NoContent)
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .WithSummary("Avalia um game na biblioteca do usuário autenticado.")
                .WithDescription("Permite que o usuário autenticado avalie um game que ele possui em sua biblioteca. O ID do usuário é obtido de forma segura diretamente a partir das claims do token JWT.")
                .WithName("RateGameInUserLibrary");

            group.MapGet("", GetLibraryByUser)
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .WithSummary("Lista a biblioteca de games do usuário autenticado.")
                .WithDescription("Retorna a lista paginada de games adquiridos pelo próprio usuário autenticado. O ID do usuário é obtido de forma segura diretamente a partir das claims do token JWT.")
                .WithName("GetUserLibrary");
        }

        private static async Task<IResult> GetLibraryByUser(
            [FromServices] ISender sender,
            ClaimsPrincipal user,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var currentUserIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return Results.Unauthorized();
            }

            var query = new GetPagedLibraryQuery(currentUserId, page, pageSize);

            var response = await sender.Send(query);

            if (response == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(response);
        }


        private static async Task<IResult> ApplyRatingGame(
            Guid gameId,
            [FromBody] RatingGameRequest request,
            ClaimsPrincipal user,
            [FromServices] ISender sender,
            CancellationToken cancellationToken)
        {
            var currentUserIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return Results.Unauthorized();
            }

            if (double.IsNaN(request.Rating) || request.Rating < LibraryEntry.MinRating || request.Rating > LibraryEntry.MaxRating)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["rating"] = [$"A avaliação deve ser entre {LibraryEntry.MinRating} e {LibraryEntry.MaxRating}."]
                });
            }

            var result = await sender.Send(new RatingGameCommand(currentUserId, gameId, request.Rating), cancellationToken);
            return result == RatingGameResult.NotInLibrary ? Results.NotFound() : Results.NoContent();
        }
    }
}
