using Fcg.UserLibrary.Application.Features.Commands.OrderFinalize;
using Fcg.UserLibrary.Domain.Entities;
using Fcg.UserLibrary.Infrastructure.Repository;
using Fcg.UserLibrary.IntegrationTests.Fixtures;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;

namespace Fcg.UserLibrary.IntegrationTests.Features.Commands.OrderFinalize
{
    // Handler de ponta a ponta contra MongoDB real (Testcontainers); logger e mediator sao mockados.
    [Collection("Mongo")]
    public class LibraryOrderCommandHandlerMongoIntegrationTests
    {
        private readonly MongoFixture _fixture;

        public LibraryOrderCommandHandlerMongoIntegrationTests(MongoFixture fixture) => _fixture = fixture;

        private static LibraryOrderCommandHandler CreateHandler(
            LibraryEntryRepository libraryRepo,
            GameSnapshotRepository gameRepo)
        {
            return new LibraryOrderCommandHandler(gameRepo, 
                libraryRepo, 
                new Mock<ILogger<LibraryOrderCommandHandler>>().Object,
                new Mock<IMediator>().Object);
        }

        [SkippableFact]
        public async Task Handle_ShouldPersistLibraryInMongo_WhenSimulatingRealUserPurchase()
        {
            _fixture.SkipIfUnavailable();

            var userId = Guid.NewGuid();    
            var database = _fixture.CreateDatabase();
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
