using Fcg.Core.Abstractions.Common;
using Fcg.UserLibrary.Application.Common.Interfaces;
using Fcg.UserLibrary.Domain.Entities;
using MongoDB.Driver;

namespace Fcg.UserLibrary.Infrastructure.Queries
{
    internal class LibraryEntryQueryRepository : ILibraryEntryQueryRepository
    {        
        public const int MaxPageSize = 100;
        private readonly IMongoDatabase _database;
        public LibraryEntryQueryRepository(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<PagedResult<LibraryEntry>> GetPagedLibraryAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken)
        {
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
            page = Math.Max(page, 1);

            var collection = _database.GetCollection<LibraryEntry>("libraries");
            
            var filter = Builders<LibraryEntry>.Filter.Eq(l=>l.Customer.CustomerId, customerId);

            var totalCount = (Int32)await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            var items = await collection
                .Find(filter)
                .SortByDescending(l=>l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<LibraryEntry>(items, page, pageSize, totalCount);
        }
    }
}
