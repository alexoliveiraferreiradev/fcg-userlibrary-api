using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Resources;
using Fcg.UserLibrary.Domain.Entities;
using FluentAssertions;

namespace Fcg.UserLibrary.Domain.Tests.Entities
{
    public class GameSnapshotTests
    {
        [Fact]
        public void Constructor_ShouldCreateGameSnapshot_WhenDataIsValid()
        {
            var snapshot = new GameSnapshot(Guid.NewGuid(), "Jogo Valido", "Descricao valida", "Acao", DateTime.UtcNow);

            snapshot.Name.Should().Be("Jogo Valido");
            snapshot.Description.Should().Be("Descricao valida");
            snapshot.Genre.Should().Be("Acao");
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenNameIsEmpty()
        {
            var act = () => new GameSnapshot(Guid.NewGuid(), string.Empty, "Descricao valida", "Acao", DateTime.UtcNow);

            var exception = act.Should().Throw<DomainException>().Which;
            exception.Message.Should().Be(DomainMessages.GameNameRequired);
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenDescriptionIsEmpty()
        {
            var act = () => new GameSnapshot(Guid.NewGuid(), "Jogo Valido", string.Empty, "Acao", DateTime.UtcNow);

            var exception = act.Should().Throw<DomainException>().Which;
            exception.Message.Should().Be(DomainMessages.GameDescriptionRequired);
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenNameLengthIsBelowMinimum()
        {
            var act = () => new GameSnapshot(Guid.NewGuid(), "ab", "Descricao valida", "Acao", DateTime.UtcNow);

            var exception = act.Should().Throw<DomainException>().Which;
            exception.Message.Should().Be(DomainMessages.GameNameLengthInvalid);
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenNameLengthIsAboveMaximum()
        {
            var act = () => new GameSnapshot(Guid.NewGuid(), new string('a', 101), "Descricao valida", "Acao", DateTime.UtcNow);

            var exception = act.Should().Throw<DomainException>().Which;
            exception.Message.Should().Be(DomainMessages.GameNameLengthInvalid);
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenDescriptionLengthIsBelowMinimum()
        {
            var act = () => new GameSnapshot(Guid.NewGuid(), "Jogo Valido", "abcd", "Acao", DateTime.UtcNow);

            var exception = act.Should().Throw<DomainException>().Which;
            exception.Message.Should().Be(DomainMessages.GameDescriptionLengthInvalid);
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenDescriptionLengthIsAboveMaximum()
        {
            var act = () => new GameSnapshot(Guid.NewGuid(), "Jogo Valido", new string('a', 501), "Acao", DateTime.UtcNow);

            var exception = act.Should().Throw<DomainException>().Which;
            exception.Message.Should().Be(DomainMessages.GameDescriptionLengthInvalid);
        }

        [Fact]
        public void ApplyChanges_ShouldUpdateFields_WhenOccurredAtIsNewerThanLastSynced()
        {
            var snapshot = new GameSnapshot(Guid.NewGuid(), "Jogo Valido", "Descricao valida", "Acao", DateTime.UtcNow.AddHours(-1));

            snapshot.ApplyChanges("Novo Nome", "Nova Descricao", "RPG", DateTime.UtcNow);

            snapshot.Name.Should().Be("Novo Nome");
            snapshot.Description.Should().Be("Nova Descricao");
            snapshot.Genre.Should().Be("RPG");
        }

        [Fact]
        public void ApplyChanges_ShouldNotUpdateFields_WhenOccurredAtIsOlderThanLastSynced()
        {
            var snapshot = new GameSnapshot(Guid.NewGuid(), "Jogo Valido", "Descricao valida", "Acao", DateTime.UtcNow.AddHours(-1));
            var lastSyncedAt = DateTime.UtcNow;
            snapshot.ApplyChanges("Nome Atual", "Descricao Atual", "RPG", lastSyncedAt);

            snapshot.ApplyChanges("Nome Antigo", "Descricao Antiga", "Terror", lastSyncedAt.AddMinutes(-10));

            snapshot.Name.Should().Be("Nome Atual");
            snapshot.Description.Should().Be("Descricao Atual");
            snapshot.Genre.Should().Be("RPG");
        }
    }
}
