using Fcg.Core.Abstractions.Interfaces;

namespace Fcg.UserLibrary.Infrastructure.Persistence
{
    /// <summary>
    /// Os dados ficam no Mongo; o LibraryDbContext guarda só o outbox do MassTransit.
    /// O SaveChanges grava as mensagens publicadas no request e o outbox as entrega depois.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LibraryDbContext _dbContext;

        public UnitOfWork(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> CommitAsync()
        {
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
