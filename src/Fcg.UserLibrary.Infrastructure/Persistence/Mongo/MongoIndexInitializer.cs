using Fcg.UserLibrary.Domain.Entities;
using MongoDB.Driver;

namespace Fcg.UserLibrary.Infrastructure.Persistence.Mongo
{
    internal static class MongoIndexInitializer
    {
        public static async Task EnsureLibraryEntryIndexesAsync(IMongoDatabase database)
        {
            var collection = database.GetCollection<LibraryEntry>("libraries");
            var keys = Builders<LibraryEntry>.IndexKeys;

            var indexModels = new List<CreateIndexModel<LibraryEntry>>
            {
                new (keys.Ascending("customer.customerId").Ascending("gamelibrary.gameId"),
                    new CreateIndexOptions{Name = "idx_library_game_customer"}),

                new(keys.Ascending("customer.customerId").Descending(o => o.CreatedAt),
                    new CreateIndexOptions { Name = "idx_customer_history" })
            };

            await collection.Indexes.CreateManyAsync(indexModels);
        }
    }
}
