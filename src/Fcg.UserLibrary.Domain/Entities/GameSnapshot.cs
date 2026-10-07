using Fcg.Core.Abstractions.Common;
using Fcg.Core.Abstractions.Resources;
using MongoDB.Bson.Serialization.Attributes;

namespace Fcg.UserLibrary.Domain.Entities
{
    public class GameSnapshot
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid Id { get; private set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string Genre { get; private set; } = string.Empty;
        public DateTime LastSyncedAt { get; private set; }

        public GameSnapshot()
        {

        }

        public GameSnapshot(Guid gameId, string nameGame, string description,
            string genre,DateTime occuredAt)
        {
            Id = gameId;
            Name = nameGame;
            Description = description;
            Genre = genre;
            LastSyncedAt = occuredAt;
            ValidateEntity();
        }

        public void ApplyChanges(string newNameGame, string newNameDescription, string newGenre,
            DateTime occuredAt)
        {
            if (occuredAt < LastSyncedAt) return;
            Name = newNameGame;
            Description = newNameDescription;
            Genre = newGenre;
            LastSyncedAt = occuredAt;
        }

        private void ValidateEntity()
        {
            AssertionConcern.AssertArgumentEmpty(Name, DomainMessages.GameNameRequired);
            AssertionConcern.AssertArgumentEmpty(Description, DomainMessages.GameDescriptionRequired);
            AssertionConcern.AssertArgumentLength(Name, 3, 100, DomainMessages.GameNameLengthInvalid);
            AssertionConcern.AssertArgumentLength(Description, 5, 500, DomainMessages.GameDescriptionLengthInvalid);
        }
    }
}
