using Fcg.Core.Abstractions.Common;
using Fcg.UserLibrary.Application.Features.Response;
using MediatR;

namespace Fcg.UserLibrary.Application.Features.Queries.GetPagedLibrary
{
    public record GetPagedLibraryQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<PagedResult<UserLibraryItemResponse>>;
}
