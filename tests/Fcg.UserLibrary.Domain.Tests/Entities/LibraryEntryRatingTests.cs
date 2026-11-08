using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.UserLibrary.Domain.Entities;
using FluentAssertions;

namespace Fcg.UserLibrary.Domain.Tests.Entities
{
    public class LibraryEntryRatingTests
    {
        private static LibraryEntry CreateEntry() =>
            new LibraryEntry(new LibraryCustomer(Guid.NewGuid()),
                new LibraryGame(Guid.NewGuid(), "Qualquer Jogo", "Qualquer Descricao", "Genero Qualquer"));

        [Fact]
        public void Constructor_ShouldStartWithoutRating()
        {
            var entry = CreateEntry();

            entry.Rating.Should().BeNull();
            entry.RatedAt.Should().BeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3.5)]
        [InlineData(5)]
        public void Rate_ShouldStoreRating_WhenValueIsInRange(double rating)
        {
            var entry = CreateEntry();
            var ratedAt = DateTime.UtcNow;

            entry.Rate(rating, ratedAt);

            entry.Rating.Should().Be(rating);
            entry.RatedAt.Should().Be(ratedAt);
            entry.UpdateAt.Should().Be(ratedAt);
        }

        [Fact]
        public void Rate_ShouldReplacePreviousRating_WhenUserRatesAgain()
        {
            var entry = CreateEntry();
            entry.Rate(2, DateTime.UtcNow);

            entry.Rate(4, DateTime.UtcNow);

            entry.Rating.Should().Be(4);
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(5.1)]
        [InlineData(double.NaN)]
        public void Rate_ShouldThrowDomainException_WhenValueIsOutOfRange(double rating)
        {
            var entry = CreateEntry();

            var act = () => entry.Rate(rating, DateTime.UtcNow);

            act.Should().Throw<DomainException>();
            entry.Rating.Should().BeNull();
        }

        [Fact]
        public void Rate_ShouldThrowDomainException_WhenEntryIsDeactivated()
        {
            var entry = CreateEntry();
            entry.DeactivateLibraryEntry();

            var act = () => entry.Rate(4, DateTime.UtcNow);

            act.Should().Throw<DomainException>();
        }
    }
}
