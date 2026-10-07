using Fcg.Core.Abstractions.Common;
using Fcg.UserLibrary.Domain.Entities;

namespace Fcg.UserLibrary.Application.Common.Interfaces
{
    public interface ILibraryEntryQueryRepository
    {
        Task<PagedResult<LibraryEntry>> GetPagedLibraryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    }
}
