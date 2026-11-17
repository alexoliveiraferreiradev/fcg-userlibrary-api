using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.Abstractions.Resources;
using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.UserLibrary.Domain.Events;
using Fcg.UserLibrary.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.UserLibrary.Application.Features.Commands.RatingGame
{
    public class RatingGameCommandHandler : IRequestHandler<RatingGameCommand, RatingGameResult>
    {
        private readonly ILibraryEntryRepository _libraryRepository;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly ILogger<RatingGameCommandHandler> _logger;

        public RatingGameCommandHandler(ILibraryEntryRepository libraryRepository,
            IIntegrationEventPublisher integrationEventPublisher,
            IUnitOfWork unitOfWork,
            IPublisher publisher,
            ILogger<RatingGameCommandHandler> logger)
        {
            _libraryRepository = libraryRepository;
            _integrationEventPublisher = integrationEventPublisher;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<RatingGameResult> Handle(RatingGameCommand request, CancellationToken cancellationToken)
        {
            if (request.UserId == Guid.Empty)
                throw new DomainException(DomainMessages.UserNotFound);

            var libraryEntry = await _libraryRepository.GetByUserAndGameAsync(request.UserId, request.GameId, cancellationToken);
            if (libraryEntry is null)
                return RatingGameResult.NotInLibrary;

            var previousRating = libraryEntry.Rating;
            var previousRatedAt = libraryEntry.RatedAt;
            var previousUpdateAt = libraryEntry.UpdateAt;

            var now = DateTime.UtcNow;
            libraryEntry.Rate(request.Rating, now);

            if (!await _libraryRepository.UpdateLibrary(libraryEntry))
                throw new InvalidOperationException($"Não foi possível gravar a avaliação do jogo {request.GameId} na biblioteca.");

            // A nota fica no Mongo e o evento no outbox do SQL: não há transação entre os dois.
            // Se o evento não for gravado, a nota volta ao que era e o erro sobe; o usuário pode tentar de novo.
            try
            {
                await _integrationEventPublisher.PublishAsync<IGameRatingIntegrationEvent>(new
                {
                    EventId = Guid.NewGuid(),
                    CreatedAt = now,
                    GameId = request.GameId,
                    UserId = request.UserId,
                    Rating = request.Rating,
                    OccurredAt = now
                }, cancellationToken);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserLibraryAPI] Evento de avaliação não gravado; desfazendo a nota do jogo {GameId} do usuário {UserId}.",
                    request.GameId, request.UserId);
                libraryEntry.RestoreRating(previousRating, previousRatedAt, previousUpdateAt);
                await _libraryRepository.UpdateLibrary(libraryEntry);
                throw;
            }

            _logger.LogInformation("[UserLibraryAPI] Usuário {UserId} avaliou o jogo {GameId} com nota {Rating}",
                request.UserId, request.GameId, request.Rating);

            await _publisher.Publish(new LibraryEntryEvent(request.UserId), cancellationToken);
            return RatingGameResult.Rated;
        }
    }
}
