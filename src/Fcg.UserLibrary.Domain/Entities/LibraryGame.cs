using Fcg.Core.Abstractions.Common;
using Fcg.Core.Abstractions.Resources;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fcg.UserLibrary.Domain.Entities
{
    public class LibraryGame
    {
        [BsonRepresentation(BsonType.String)]
        public Guid GameId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string Genre { get; private set; } = string.Empty;

        private LibraryGame()
        {
        }

        public LibraryGame(Guid gameId, string gameName, string descriptionGame, string genreGame)
        {
            GameId = gameId;
            Name = gameName;
            Description = descriptionGame;
            Genre = genreGame;
            ValidateEntity();
        }

        private void ValidateEntity()
        {
            AssertionConcern.AssertArgumentNotEmpty(GameId, DomainMessages.GameNotFound);
            AssertionConcern.AssertArgumentNotNull(Name, DomainMessages.GameNameRequired);
            AssertionConcern.AssertArgumentNotNull(Description, DomainMessages.GameDescriptionRequired);
            AssertionConcern.AssertArgumentNotNull(Genre, DomainMessages.GameGenreInvalid);
        }
    }
}
