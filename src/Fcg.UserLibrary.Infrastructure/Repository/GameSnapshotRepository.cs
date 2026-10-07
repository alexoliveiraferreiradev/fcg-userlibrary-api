using Fcg.UserLibrary.Domain.Entities;
using Fcg.UserLibrary.Domain.Repositories;
using MongoDB.Driver;

namespace Fcg.UserLibrary.Infrastructure.Repository
{
    public class GameSnapshotRepository : IGameRepository
    {        
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<GameSnapshot> _collection;

        public GameSnapshotRepository(IMongoDatabase database)
        {
            _database = database;
            _collection = _database.GetCollection<GameSnapshot>("games");
        }

        public async Task<Guid> AddSnapshot(GameSnapshot gameSnapshot)
        {
            await _collection.InsertOneAsync(gameSnapshot);
            return gameSnapshot.Id;
        }

        public async Task<GameSnapshot> GetGameByIdAsync(Guid gameId,CancellationToken cancellationToken    )
        {
            var filer = Builders<GameSnapshot>.Filter.Eq(g=>g.Id,gameId);
            return await _collection.Find(filer).SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> UpdateSnapshot(GameSnapshot gameSnapshot)
        {
            var result = await _collection.ReplaceOneAsync(g => g.Id == gameSnapshot.Id, gameSnapshot);
            return result.ModifiedCount > 0;    
        }
    }
}
