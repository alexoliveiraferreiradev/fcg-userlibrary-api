using Fcg.Core.Abstractions.Common;
using Fcg.UserLibrary.Application.Common.Interfaces;
using Fcg.UserLibrary.Application.Features.Queries.GetPagedLibrary;
using Fcg.UserLibrary.Application.Features.Response;
using Fcg.UserLibrary.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Fcg.UserLibrary.Application.Tests.Features.Queries.GetPagedLibrary
{
    public class GetPagedLibraryQueryHandlerTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock = new();
        private readonly Mock<ILibraryEntryQueryRepository> _libraryEntryQueryRepositoryMock = new();

        private GetPagedLibraryQueryHandler CreateHandler() => new(
            _cacheServiceMock.Object,
            _libraryEntryQueryRepositoryMock.Object);

        [Fact]
        public async Task Handle_ShouldReturnCachedResultWithoutQueryingRepository_WhenCacheExists()
        {
            var query = new GetPagedLibraryQuery(Guid.NewGuid());
            var cached = new PagedResult<UserLibraryItemResponse>(new List<UserLibraryItemResponse> { new() }, query.Page, query.PageSize, 1);

            _cacheServiceMock
                .Setup(c => c.GetAsync<PagedResult<UserLibraryItemResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cached);

            var handler = CreateHandler();

            var result = await handler.Handle(query, CancellationToken.None);

            result.Should().BeSameAs(cached);
            _libraryEntryQueryRepositoryMock.Verify(r => r.GetPagedLibraryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldQueryRepositoryAndCacheResult_WhenCacheDoesNotExistAndHasItems()
        {
            var query = new GetPagedLibraryQuery(Guid.NewGuid());
            var libraryCustomer = new LibraryCustomer(Guid.NewGuid());
            var libraryGame = new LibraryGame(Guid.NewGuid(), "Test Game","Description Game","Action");
            var pagedResult = new PagedResult<LibraryEntry>(new List<LibraryEntry> { new(libraryCustomer,libraryGame) }, query.Page, query.PageSize, 1);
            

            _cacheServiceMock
                .Setup(c => c.GetAsync<PagedResult<LibraryEntry>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PagedResult<LibraryEntry>?)null);

            _libraryEntryQueryRepositoryMock
                .Setup(r => r.GetPagedLibraryAsync(query.UserId, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            var handler = CreateHandler();

            var expectedResult = new PagedResult<UserLibraryItemResponse>(pagedResult.Items.Select(o=> new UserLibraryItemResponse
            {
                GameId = o.GameLibrary.GameId,
                GameName = o.GameLibrary.Name,
                Description = o.GameLibrary.Description,
                Genre = o.GameLibrary.Genre,
                AcquiredAt = o.CreatedAt
            }).ToList(), query.Page, query.PageSize, pagedResult.TotalItems);

            PagedResult<UserLibraryItemResponse>? cachedValue = null;
            _cacheServiceMock
                .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedResult<UserLibraryItemResponse>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Callback<string, PagedResult<UserLibraryItemResponse>, TimeSpan, CancellationToken>((_, value, _, _) => cachedValue = value)
                .Returns(Task.CompletedTask);

            var result = await handler.Handle(query, CancellationToken.None);


            result.Should().BeEquivalentTo(expectedResult);
            _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedResult<UserLibraryItemResponse>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            cachedValue.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task Handle_ShouldNotCacheResult_WhenCacheDoesNotExistAndRepositoryReturnsNoItems()
        {
            var query = new GetPagedLibraryQuery(Guid.NewGuid());
            var pagedResult = new PagedResult<LibraryEntry>(new List<LibraryEntry>(), query.Page, query.PageSize, 0);

            _cacheServiceMock
                .Setup(c => c.GetAsync<PagedResult<UserLibraryItemResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PagedResult<UserLibraryItemResponse>?)null);

            _libraryEntryQueryRepositoryMock
                .Setup(r => r.GetPagedLibraryAsync(query.UserId, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            var handler = CreateHandler();

            var result = await handler.Handle(query, CancellationToken.None);

            result.Items.Should().BeEmpty();
            _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedResult<UserLibraryItemResponse>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
