using Fcg.UserLibrary.Domain.Entities;

namespace Fcg.UserLibrary.Domain.Repositories
{
    public interface ILibraryEntryRepository
    {
        Task<Guid> AddLibrary(LibraryEntry library);
        Task<IEnumerable<LibraryEntry>> GetLibraryByUserIdAsync(Guid userId,CancellationToken cancellation);
        Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken);

        Task<bool> UpdateLibrary(LibraryEntry library); 
    }
}
