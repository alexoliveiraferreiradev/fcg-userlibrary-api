using Fcg.UserLibrary.Application.Common.Interfaces;
using Fcg.UserLibrary.Application.Features.EventHandler;
using Fcg.UserLibrary.Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fcg.UserLibrary.Application.Tests.Features.EventHandler
{
    // LibraryEntryCacheHandler é internal; Moq não consegue gerar proxy dinâmico para
    // ILogger<T> fechado sobre um tipo internal, então usamos NullLogger<T> em vez de mock.
    public class LibraryEntryCacheHandlerTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock = new();

        private LibraryEntryCacheHandler CreateHandler() => new(_cacheServiceMock.Object, NullLogger<LibraryEntryCacheHandler>.Instance);

        [Fact]
        public async Task Handle_ShouldInvalidateLibraryCache_WhenNotificationIsValid()
        {
            var userId = Guid.NewGuid();
            var notification = new LibraryEntryEvent(userId);

            var handler = CreateHandler();

            await handler.Handle(notification, CancellationToken.None);

            _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync($"library:u_{userId}"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldPropagateException_WhenCacheServiceFails()
        {
            var notification = new LibraryEntryEvent(Guid.NewGuid());

            _cacheServiceMock
                .Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Falha ao acessar o cache"));

            var handler = CreateHandler();

            var act = async () => await handler.Handle(notification, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
