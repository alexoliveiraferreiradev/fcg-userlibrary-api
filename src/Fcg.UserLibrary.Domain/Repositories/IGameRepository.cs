using Fcg.UserLibrary.Domain.Entities;

namespace Fcg.UserLibrary.Domain.Repositories
{
    public interface IGameRepository
    {
        Task<Guid> AddSnapshot(GameSnapshot gameSnapshot);
        Task<GameSnapshot> GetGameByIdAsync(Guid gameId,CancellationToken cancellationToken);
        Task<bool> UpdateSnapshot(GameSnapshot gameSnapshot);     
    }
}
