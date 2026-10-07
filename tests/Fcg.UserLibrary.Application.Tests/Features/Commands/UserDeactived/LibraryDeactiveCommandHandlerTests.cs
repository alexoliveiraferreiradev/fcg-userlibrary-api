using Fcg.UserLibrary.Application.Features.Commands.UserDeactived;
using Fcg.UserLibrary.Domain.Entities;
using Fcg.UserLibrary.Domain.Events;
using Fcg.UserLibrary.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.UserLibrary.Application.Tests.Features.Commands.UserDeactived
{
    public class LibraryDeactiveCommandHandlerTests
    {
        private readonly Mock<ILibraryEntryRepository> _libraryEntryRepositoryMock = new();
        private readonly Mock<ILogger<LibraryDeactiveCommandHandler>> _loggerMock = new();
        private readonly Mock<IPublisher> _publisherMock = new();
        private LibraryCustomer _customerMock = new LibraryCustomer(Guid.NewGuid());
        private LibraryGame _gamelibraryMock = new LibraryGame(Guid.NewGuid(),"Qualquer Jogo","Qualquer Descrição","Gênero Qualquer");
        private LibraryDeactiveCommandHandler CreateHandler() => new(
            _libraryEntryRepositoryMock.Object,
            _loggerMock.Object,
            _publisherMock.Object);

        [Fact]
        public async Task Handle_ShouldDeactivateAllEntriesAndCommit_WhenActiveEntriesExist()
        {
            var userId = Guid.NewGuid();
            LibraryCustomer customer = new LibraryCustomer(userId);
            var entry1 = new LibraryEntry(customer, _gamelibraryMock);
            var entry2 = new LibraryEntry(customer, _gamelibraryMock);
            var command = new LibraryDeactiveCommand(Guid.NewGuid(), DateTime.Now, customer.CustomerId);

            _libraryEntryRepositoryMock
                .Setup(r => r.GetLibraryByUserIdAsync(customer.CustomerId, CancellationToken.None))
                .ReturnsAsync(new[] { entry1, entry2 });

            var handler = CreateHandler();

            await handler.Handle(command, CancellationToken.None);

            entry1.IsActive.Should().BeFalse();
            entry2.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldReturnWithoutCommitting_WhenNoEntriesExist()
        {
            var userId = Guid.NewGuid();
            var command = new LibraryDeactiveCommand(Guid.NewGuid(), DateTime.Now, userId);

            _libraryEntryRepositoryMock
                .Setup(r => r.GetLibraryByUserIdAsync(userId,CancellationToken.None))
                .ReturnsAsync(Enumerable.Empty<LibraryEntry>());

            var handler = CreateHandler();

            await handler.Handle(command, CancellationToken.None);
        }

        [Fact]
        public async Task Handle_ShouldReturnWithoutCommitting_WhenAllEntriesAreAlreadyInactive()
        {            
            var entry = new LibraryEntry(_customerMock, _gamelibraryMock);
            entry.DeactivateLibraryEntry();
            var command = new LibraryDeactiveCommand(Guid.NewGuid(), DateTime.Now, _customerMock.CustomerId);

            _libraryEntryRepositoryMock
                .Setup(r => r.GetLibraryByUserIdAsync(_customerMock.CustomerId, CancellationToken.None))
                .ReturnsAsync(new[] { entry });

            var handler = CreateHandler();

            await handler.Handle(command, CancellationToken.None);
        }

        [Fact]
        public async Task Handle_ShouldDeactivateOnlyActiveEntries_WhenLibraryIsMixed()
        {
            var userId = Guid.NewGuid();
            LibraryCustomer customer = new LibraryCustomer(userId);
            var activeEntry = new LibraryEntry(customer, _gamelibraryMock);
            var inactiveEntry = new LibraryEntry(customer, _gamelibraryMock);
            inactiveEntry.DeactivateLibraryEntry();
            var updateAtBefore = inactiveEntry.UpdateAt;
            var command = new LibraryDeactiveCommand(Guid.NewGuid(), DateTime.Now, customer.CustomerId);

            _libraryEntryRepositoryMock
                .Setup(r => r.GetLibraryByUserIdAsync(customer.CustomerId, CancellationToken.None))
                .ReturnsAsync(new[] { activeEntry, inactiveEntry });

            var handler = CreateHandler();

            await handler.Handle(command, CancellationToken.None);

            activeEntry.IsActive.Should().BeFalse();
            inactiveEntry.IsActive.Should().BeFalse();
            inactiveEntry.UpdateAt.Should().Be(updateAtBefore);
        }

        [Fact]
        public async Task Handle_ShouldPublishLibraryEntryEvent_WhenEntriesAreDeactivated()
        {
            var userId = Guid.NewGuid();
            var entry = new LibraryEntry(_customerMock, _gamelibraryMock);
            var command = new LibraryDeactiveCommand(Guid.NewGuid(), DateTime.Now, _customerMock.CustomerId);

            _libraryEntryRepositoryMock
                .Setup(r => r.GetLibraryByUserIdAsync(_customerMock.CustomerId, CancellationToken.None))
                .ReturnsAsync(new[] { entry });

            var handler = CreateHandler();

            await handler.Handle(command, CancellationToken.None);

            _publisherMock.Verify(
                p => p.Publish(It.Is<LibraryEntryEvent>(e => e.UserId == _customerMock.CustomerId), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
