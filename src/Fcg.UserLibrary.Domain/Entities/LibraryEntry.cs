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
        /// <summary>Nota do usuário para o jogo; null enquanto ele não avaliou.</summary>
        public double? Rating { get; private set; }
        public DateTime? RatedAt { get; private set; }

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

        /// <summary>Só quem tem o jogo ativo na biblioteca avalia; avaliar de novo substitui a nota anterior.</summary>
        public void Rate(double rating, DateTime ratedAt)
        {
            if (!IsActive)
                throw new DomainException("Só é possível avaliar jogos ativos na biblioteca.");

            if (double.IsNaN(rating) || rating < MinRating || rating > MaxRating)
                throw new DomainException($"A avaliação deve ser entre {MinRating} e {MaxRating}.");

            Rating = rating;
            RatedAt = ratedAt;
            UpdateAt = ratedAt;
        }
        /// <summary>
        /// Volta à nota e às datas de antes da última avaliação. Usado quando a nota foi gravada,
        /// mas o evento para o Catalog não, para a biblioteca não ficar com uma nota que a média não conhece.
        /// </summary>
        public void RestoreRating(double? previousRating, DateTime? previousRatedAt, DateTime previousUpdateAt)
        {
            Rating = previousRating;
            RatedAt = previousRatedAt;
            UpdateAt = previousUpdateAt;
        }

        public const double MinRating = 0;
        public const double MaxRating = 5;
    }
}
