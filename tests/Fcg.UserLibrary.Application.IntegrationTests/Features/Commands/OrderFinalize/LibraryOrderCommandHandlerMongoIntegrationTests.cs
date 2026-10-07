using Fcg.UserLibrary.Application.Features.Commands.OrderFinalize;
using Fcg.UserLibrary.Domain.Entities;
using Fcg.UserLibrary.Infrastructure.Repository;
using Fcg.UserLibrary.Infrastructure.Persistence.Mongo;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;

namespace Fcg.UserLibrary.IntegrationTests.Features.Commands.OrderFinalize
{
    // Testes manuais contra um MongoDB real.
    public class LibraryOrderCommandHandlerMongoIntegrationTests
    {
        private static IMongoDatabase CreateDatabase()
        {
            MongoConventions.EnsureRegistered();
            var client = new MongoClient("mongodb://admin:password123@localhost:27017/?directConnection=true");
            return client.GetDatabase($"fcg_userlibraries_playtest_{Guid.NewGuid():N}");
        }

        private static LibraryOrderCommandHandler CreateHandler(
            LibraryEntryRepository libraryRepo,
            GameSnapshotRepository gameRepo)
        {
            return new LibraryOrderCommandHandler(gameRepo, 
                libraryRepo, 
                new Mock<ILogger<LibraryOrderCommandHandler>>().Object,
                new Mock<IMediator>().Object);
        }

        [Fact]
        public async Task Handle_ShouldPersistLibraryInMongo_WhenSimulatingRealUserPurchase()
        {
            var userId = Guid.NewGuid();    
            var database = CreateDatabase();    
            var libraryRepo = new LibraryEntryRepository(database);   
            var gameRepo = new GameSnapshotRepository(database);

            var witcher = new GameSnapshot(Guid.NewGuid(), "The Witcher 3", "RPG de mundo aberto", "RPG",  DateTime.UtcNow);
            await gameRepo.AddSnapshot(witcher);

            var now = DateTime.UtcNow;

            var command = new LibraryOrderCommand(Guid.NewGuid(), now, userId,
                                                    witcher.Id, witcher.Name, true, witcher.Description,
                                                    witcher.Genre, now);

            var handler  = CreateHandler(libraryRepo, gameRepo);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(witcher.Name,result.GameName);

            var libraryCollection = database.GetCollection<LibraryEntry>("libraries");
            var persistLibrary = await libraryCollection
                .Find(o=>o.Customer.CustomerId == userId).SingleAsync();
                        
            persistLibrary.GameLibrary.Should().NotBeNull();

        }
    }
}
