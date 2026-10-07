using Fcg.UserLibrary.Application.Features.Queries.GetPagedLibrary;
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

            group.MapGet("",
                async ([FromServices] ISender mediator,
                       ClaimsPrincipal user,
                       [FromQuery] int page = 1,
                       [FromQuery] int pageSize = 10) =>
                {
                    var currentUserIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!Guid.TryParse(currentUserIdClaim, out var currentUserId))
                    {
                        return Results.Unauthorized();
                    }

                    var query = new GetPagedLibraryQuery(currentUserId, page, pageSize);

                    var response = await mediator.Send(query);

                    if (response == null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(response);
                })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Lista a biblioteca de games do usuário autenticado.")
            .WithDescription("Retorna a lista paginada de games adquiridos pelo próprio usuário autenticado. O ID do usuário é obtido de forma segura diretamente a partir das claims do token JWT.")
            .WithName("GetUserLibrary");
        }
    }
}
