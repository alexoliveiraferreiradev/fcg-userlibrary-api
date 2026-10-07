using Fcg.Core.Abstractions.Common;
using Fcg.UserLibrary.Application.Common.Interfaces;
using Fcg.UserLibrary.Application.Features.Response;
using MediatR;

namespace Fcg.UserLibrary.Application.Features.Queries.GetPagedLibrary
{
    public class GetPagedLibraryQueryHandler : IRequestHandler<GetPagedLibraryQuery, PagedResult<UserLibraryItemResponse>>
    {
        private readonly ICacheService _cacheService;
        private readonly ILibraryEntryQueryRepository _libraryEntryQueryRepository;

        public GetPagedLibraryQueryHandler(ICacheService cacheService, 
            ILibraryEntryQueryRepository libraryEntryQueryRepository)
        {
            _cacheService = cacheService;
            _libraryEntryQueryRepository = libraryEntryQueryRepository;
        }

        public async Task<PagedResult<UserLibraryItemResponse>> Handle(GetPagedLibraryQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"library:u_{request.UserId}:p{request.Page}:t{request.PageSize}";
            var libraryInCache = await _cacheService.GetAsync<PagedResult<UserLibraryItemResponse>>(cacheKey, cancellationToken);
            if (libraryInCache != null)
            {
                return libraryInCache;
            }
            var pagedLibrary = await _libraryEntryQueryRepository.GetPagedLibraryAsync(request.UserId, request.Page, request.PageSize, cancellationToken);

            var items = pagedLibrary.Items.Select(l => new UserLibraryItemResponse
            {
                GameId = l.GameLibrary.GameId,
                GameName = l.GameLibrary.Name,
                Description = l.GameLibrary.Description,
                Genre = l.GameLibrary.Genre,
                AcquiredAt = l.CreatedAt
            }).ToList();

            var result = new PagedResult<UserLibraryItemResponse>(items, request.Page, request.PageSize, pagedLibrary.TotalItems);

            if (items.Any())
            {
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2), cancellationToken);
            }

            return result;
        }
    }
}
