using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.UserLibrary.Application.Features.Commands.RatingGame;
using Fcg.UserLibrary.Domain.Entities;
using Fcg.UserLibrary.Domain.Events;
using Fcg.UserLibrary.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.UserLibrary.Application.Tests.Features.Commands.RatingGame
{
    public class RatingGameCommandHandlerTests
    {
        private readonly Mock<ILibraryEntryRepository> _libraryEntryRepositoryMock = new();
        private readonly Mock<IIntegrationEventPublisher> _integrationEventPublisherMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IPublisher> _publisherMock = new();
        private readonly Mock<ILogger<RatingGameCommandHandler>> _loggerMock = new();

        public RatingGameCommandHandlerTests()
        {
            _libraryEntryRepositoryMock
                .Setup(r => r.UpdateLibrary(It.IsAny<LibraryEntry>()))
                .ReturnsAsync(true);
        }

        private RatingGameCommandHandler CreateHandler() => new(
            _libraryEntryRepositoryMock.Object,
            _integrationEventPublisherMock.Object,
            _unitOfWorkMock.Object,
            _publisherMock.Object,
            _loggerMock.Object);

        private static LibraryEntry CreateEntry(Guid userId, Guid gameId) =>
            new LibraryEntry(new LibraryCustomer(userId),
                new LibraryGame(gameId, "Qualquer Jogo", "Qualquer Descricao", "Genero Qualquer"));

        [Fact]
        public async Task Handle_ShouldRateEntryPublishEventAndCommit_WhenUserOwnsGame()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var entry = CreateEntry(userId, gameId);
            _libraryEntryRepositoryMock
                .Setup(r => r.GetByUserAndGameAsync(userId, gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entry);

            var result = await CreateHandler().Handle(new RatingGameCommand(userId, gameId, 4.5), CancellationToken.None);

            result.Should().Be(RatingGameResult.Rated);
            entry.Rating.Should().Be(4.5);
            _libraryEntryRepositoryMock.Verify(r => r.UpdateLibrary(entry), Times.Once);
            _integrationEventPublisherMock.Verify(p => p.PublishAsync<IGameRatingIntegrationEvent>(
                It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            _publisherMock.Verify(p => p.Publish(
                It.Is<LibraryEntryEvent>(e => e.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotInLibrary_WhenGameIsNotInUserLibrary()
        {
            _libraryEntryRepositoryMock
                .Setup(r => r.GetByUserAndGameAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((LibraryEntry?)null);

            var result = await CreateHandler().Handle(new RatingGameCommand(Guid.NewGuid(), Guid.NewGuid(), 4), CancellationToken.None);

            result.Should().Be(RatingGameResult.NotInLibrary);
            _integrationEventPublisherMock.Verify(p => p.PublishAsync<IGameRatingIntegrationEvent>(
                It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldNotPublish_WhenRatingIsOutOfRange()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            _libraryEntryRepositoryMock
                .Setup(r => r.GetByUserAndGameAsync(userId, gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateEntry(userId, gameId));

            var act = () => CreateHandler().Handle(new RatingGameCommand(userId, gameId, 7), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
            _libraryEntryRepositoryMock.Verify(r => r.UpdateLibrary(It.IsAny<LibraryEntry>()), Times.Never);
            _integrationEventPublisherMock.Verify(p => p.PublishAsync<IGameRatingIntegrationEvent>(
                It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldRestorePreviousRating_WhenEventCannotBeSaved()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var entry = CreateEntry(userId, gameId);
            entry.Rate(2, DateTime.UtcNow.AddDays(-1));
            var previousRatedAt = entry.RatedAt;
            _libraryEntryRepositoryMock
                .Setup(r => r.GetByUserAndGameAsync(userId, gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entry);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ThrowsAsync(new InvalidOperationException("SQL fora do ar"));

            var act = () => CreateHandler().Handle(new RatingGameCommand(userId, gameId, 5), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
            entry.Rating.Should().Be(2);
            entry.RatedAt.Should().Be(previousRatedAt);
            _libraryEntryRepositoryMock.Verify(r => r.UpdateLibrary(entry), Times.Exactly(2));
            _publisherMock.Verify(p => p.Publish(It.IsAny<LibraryEntryEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenRatingCannotBeSavedInLibrary()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            _libraryEntryRepositoryMock
                .Setup(r => r.GetByUserAndGameAsync(userId, gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateEntry(userId, gameId));
            _libraryEntryRepositoryMock
                .Setup(r => r.UpdateLibrary(It.IsAny<LibraryEntry>()))
                .ReturnsAsync(false);

            var act = () => CreateHandler().Handle(new RatingGameCommand(userId, gameId, 4), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
            _integrationEventPublisherMock.Verify(p => p.PublishAsync<IGameRatingIntegrationEvent>(
                It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowDomainException_WhenUserIdIsEmpty()
        {
            var act = () => CreateHandler().Handle(new RatingGameCommand(Guid.Empty, Guid.NewGuid(), 4), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }
}
