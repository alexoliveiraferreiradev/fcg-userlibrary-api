using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.UserLibrary.Domain.Entities;
using FluentAssertions;

namespace Fcg.UserLibrary.Domain.Tests.Entities
{
    public class LibraryEntryTests
    {
        private LibraryCustomer _customerMock = new LibraryCustomer(Guid.NewGuid());
        private LibraryGame _gamelibraryMock = new LibraryGame(Guid.NewGuid(), "Qualquer Jogo", "Qualquer Descrição", "Gênero Qualquer");
        [Fact]
        public void Constructor_ShouldCreateActiveLibraryEntry_WhenUserIdIsValid()
        {
            var entry = new LibraryEntry(_customerMock, _gamelibraryMock);

            entry.Customer.CustomerId.Should().Be(_customerMock.CustomerId);
            entry.GameLibrary.GameId.Should().Be(_gamelibraryMock.GameId);
            entry.IsActive.Should().BeTrue();
            entry.CreatedAt.Should().Be(entry.UpdateAt);
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenUserIsNull()
        {
            LibraryCustomer customer = null; 
            var act = () => new LibraryEntry(customer, _gamelibraryMock);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenGameIsNull()
        {
            LibraryGame game = null;
            var act = () => new LibraryEntry(_customerMock, game);

            act.Should().Throw<DomainException>();
        }


        [Fact]
        public void DeactivateLibraryEntry_ShouldSetIsActiveToFalse_WhenEntryIsActive()
        {
            var entry = new LibraryEntry(_customerMock,_gamelibraryMock);

            entry.DeactivateLibraryEntry();

            entry.IsActive.Should().BeFalse();
        }

        [Fact]
        public void ReactiveLibraryEntry_ShouldSetIsActiveToTrue_WhenEntryIsInactive()
        {
            var entry = new LibraryEntry(_customerMock,_gamelibraryMock);
            entry.DeactivateLibraryEntry();

            entry.ReactiveLibraryEntry();

            entry.IsActive.Should().BeTrue();
        }
    }
}
