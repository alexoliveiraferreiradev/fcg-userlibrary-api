using Fcg.UserLibrary.Domain.Entities;
using Fcg.UserLibrary.Domain.Repositories;
using MongoDB.Driver;

namespace Fcg.UserLibrary.Infrastructure.Repository
{
    public class LibraryEntryRepository : ILibraryEntryRepository
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<LibraryEntry> _collection;
        public LibraryEntryRepository(IMongoDatabase database)
        {
            _database = database;
            _collection = _database.GetCollection<LibraryEntry>("libraries");
        }
        public async Task<Guid> AddLibrary(LibraryEntry library)
        {
            await _collection.InsertOneAsync(library);   
            return library.Id;  
        }

        public async Task<IEnumerable<LibraryEntry>> GetLibraryByUserIdAsync(Guid userId,CancellationToken cancellationToken)
        {
            var filter = Builders<LibraryEntry>.Filter.Eq(l=>l.Customer.CustomerId, userId);
            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid gameId,CancellationToken cancellationToken)
        {
            var filter = Builders<LibraryEntry>.Filter.And(
                Builders<LibraryEntry>.Filter.Eq(l=>l.Customer.CustomerId,userId),
                Builders<LibraryEntry>.Filter.Eq(l=>l.GameLibrary.GameId,gameId));

            return await _collection.Find(filter).AnyAsync(cancellationToken);
        }

        public async Task<bool> UpdateLibrary(LibraryEntry library)
        {
            var result = await _collection.ReplaceOneAsync(l => l.Id == library.Id, library);
            return result.ModifiedCount > 0;
        }
    }
}
