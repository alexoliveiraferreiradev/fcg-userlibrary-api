using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Common.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace Fcg.UserLibrary.Domain.Entities
{
    public class LibraryEntry : IAggregateRoot
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid Id { get;private set; }              
        public LibraryCustomer Customer { get; private set; }    
        public LibraryGame GameLibrary { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdateAt { get; private set; }
        public bool IsActive { get; private set; }

        private LibraryEntry()
        {            
        }       
        public LibraryEntry(LibraryCustomer libraryCustomer,LibraryGame libraryGame)
        {
            Id = Guid.NewGuid();
            Customer = libraryCustomer;
            GameLibrary = libraryGame;
            CreatedAt = DateTime.UtcNow;
            UpdateAt = CreatedAt;
            IsActive = true;
            ValidateEntity();
        }

        private void ValidateEntity()
        {
            if (GameLibrary is null)
                throw new DomainException("É preciso ter um jogo para adicionar a biblioteca");

            if (Customer is null)
                throw new DomainException("É preciso ter um usuário para adicionar o jogo a biblioteca");
        }

        public void DeactivateLibraryEntry()
        {
            IsActive = false;
            UpdateAt = DateTime.UtcNow; 
        }

        public void ReactiveLibraryEntry()
        {
            IsActive = true;
            UpdateAt = DateTime.UtcNow;
        }

    }
}
