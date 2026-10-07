using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Interfaces;
using Fcg.UserLibrary.Application.Common.Interfaces;
using Fcg.UserLibrary.Application.Features.Commands.OrderFinalize;
using Fcg.UserLibrary.Domain.Entities;
using Fcg.UserLibrary.Domain.Events;
using Fcg.UserLibrary.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.UserLibrary.Application.Tests.Features.Commands.OrderFinalize
{
    public class LibraryOrderCommandHandlerTests
    {
        private readonly Mock<IGameRepository> _gameSnapshotRepositoryMock = new();
        private readonly Mock<ILibraryEntryRepository> _libraryRepositoryMock = new();
        private readonly Mock<ILogger<LibraryOrderCommandHandler>> _loggerMock = new();
        private readonly Mock<IPublisher> _publisherMock = new();

        private LibraryOrderCommandHandler CreateHandler() => new(
            _gameSnapshotRepositoryMock.Object,
            _libraryRepositoryMock.Object,
            _loggerMock.Object,
            _publisherMock.Object);

        [Fact]
        public async Task Handle_ShouldAddLibraryEntryAndReturnResponse_WhenGameExists()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var acquiredAt = DateTime.UtcNow;
            var snapshot = new GameSnapshot(gameId, "Jogo Valido", "Descricao valida", "Acao", DateTime.UtcNow);
            var command = new LibraryOrderCommand(Guid.NewGuid(),DateTime.Now,userId, gameId, "Jogo Valido", true, "Descricao valida", "Acao", acquiredAt);

            _gameSnapshotRepositoryMock.Setup(r => r.GetGameByIdAsync(gameId,CancellationToken.None)).ReturnsAsync(snapshot);
            _libraryRepositoryMock.Setup(r => r.ExistsAsync(userId, gameId, CancellationToken.None)).ReturnsAsync(false);
            

            var handler = CreateHandler();

            var result = await handler.Handle(command, CancellationToken.None);

            result.GameId.Should().Be(gameId);
            result.GameName.Should().Be(snapshot.Name);
            result.Description.Should().Be(snapshot.Description);
            result.Genre.Should().Be(snapshot.Genre);
            result.AcquiredAt.Should().Be(acquiredAt);
            _libraryRepositoryMock.Verify(r => r.AddLibrary(It.Is<LibraryEntry>(e => e.Customer.CustomerId == userId)), Times.Once);          
        }

        [Fact]
        public async Task Handle_ShouldPersistGameId_WhenCreatingLibraryEntry()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var snapshot = new GameSnapshot(gameId, "Jogo Valido", "Descricao valida", "Acao", DateTime.UtcNow);
            var command = new LibraryOrderCommand(Guid.NewGuid(), DateTime.Now, userId, gameId, "Jogo Valido", true, "Descricao valida", "Acao", DateTime.UtcNow);

            _gameSnapshotRepositoryMock.Setup(r => r.GetGameByIdAsync(gameId, CancellationToken.None)).ReturnsAsync(snapshot);
            _libraryRepositoryMock.Setup(r => r.ExistsAsync(userId, gameId, CancellationToken.None)).ReturnsAsync(false);

            LibraryEntry? capturedEntry = null;
            _libraryRepositoryMock
                .Setup(r => r.AddLibrary(It.IsAny<LibraryEntry>()))
                .Callback<LibraryEntry>(e => capturedEntry = e);

            var handler = CreateHandler();

            await handler.Handle(command, CancellationToken.None);

            capturedEntry.Should().NotBeNull();
            capturedEntry!.GameLibrary.GameId.Should().Be(gameId);
            capturedEntry.Customer.CustomerId.Should().Be(userId);
        }

        [Fact]
        public async Task Handle_ShouldPublishLibraryEntryEvent_WhenEntryIsCreated()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var snapshot = new GameSnapshot(gameId, "Jogo Valido", "Descricao valida", "Acao", DateTime.UtcNow);
            var command = new LibraryOrderCommand(Guid.NewGuid(), DateTime.Now, userId, gameId, "Jogo Valido", true, "Descricao valida", "Acao", DateTime.UtcNow);

            _gameSnapshotRepositoryMock.Setup(r => r.GetGameByIdAsync(gameId, CancellationToken.None)).ReturnsAsync(snapshot);
            _libraryRepositoryMock.Setup(r => r.ExistsAsync(userId, gameId, CancellationToken.None)).ReturnsAsync(false);

            var handler = CreateHandler();

            await handler.Handle(command, CancellationToken.None);

            _publisherMock.Verify(
                p => p.Publish(It.Is<LibraryEntryEvent>(e => e.UserId == userId), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotDuplicateEntry_WhenGameIsAlreadyInLibrary()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var acquiredAt = DateTime.UtcNow;
            var snapshot = new GameSnapshot(gameId, "Jogo Valido", "Descricao valida", "Acao", DateTime.UtcNow);
            var command = new LibraryOrderCommand(Guid.NewGuid(), DateTime.Now, userId, gameId, "Jogo Valido", true, "Descricao valida", "Acao", acquiredAt);

            _gameSnapshotRepositoryMock.Setup(r => r.GetGameByIdAsync(gameId, CancellationToken.None)).ReturnsAsync(snapshot);
            _libraryRepositoryMock.Setup(r => r.ExistsAsync(userId, gameId, CancellationToken.None)).ReturnsAsync(true);

            var handler = CreateHandler();

            var result = await handler.Handle(command, CancellationToken.None);

            result.GameId.Should().Be(gameId);
            result.AcquiredAt.Should().Be(acquiredAt);
            _libraryRepositoryMock.Verify(r => r.AddLibrary(It.IsAny<LibraryEntry>()), Times.Never);
            _publisherMock.Verify(
                p => p.Publish(It.IsAny<LibraryEntryEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowDomainException_WhenGameIsNotFound()
        {
            var command = new LibraryOrderCommand(Guid.NewGuid(), DateTime.Now, Guid.NewGuid(), Guid.NewGuid(), "Jogo", true, "Descricao", "Acao", DateTime.UtcNow);

            _gameSnapshotRepositoryMock.Setup(r => r.GetGameByIdAsync(command.GameId, CancellationToken.None)).ReturnsAsync((GameSnapshot?)null!);

            var handler = CreateHandler();

            var act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
            _libraryRepositoryMock.Verify(r => r.AddLibrary(It.IsAny<LibraryEntry>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowDomainException_WhenUserIdIsEmpty()
        {
            var gameId = Guid.NewGuid();
            var snapshot = new GameSnapshot(gameId, "Jogo Valido", "Descricao valida", "Acao", DateTime.UtcNow);
            var command = new LibraryOrderCommand(Guid.NewGuid(), DateTime.Now, Guid.Empty, gameId, "Jogo Valido", true, "Descricao valida", "Acao", DateTime.UtcNow);

            _gameSnapshotRepositoryMock.Setup(r => r.GetGameByIdAsync(gameId, CancellationToken.None)).ReturnsAsync(snapshot);
            _libraryRepositoryMock.Setup(r => r.ExistsAsync(Guid.Empty, gameId, CancellationToken.None)).ReturnsAsync(false);

            var handler = CreateHandler();

            var act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
            _libraryRepositoryMock.Verify(r => r.AddLibrary(It.IsAny<LibraryEntry>()), Times.Never);
        }
    }
}
