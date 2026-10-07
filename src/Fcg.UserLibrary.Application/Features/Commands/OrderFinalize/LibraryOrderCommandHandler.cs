using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Resources;
using Fcg.UserLibrary.Application.Features.Response;
using Fcg.UserLibrary.Domain.Entities;
using Fcg.UserLibrary.Domain.Events;
using Fcg.UserLibrary.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.UserLibrary.Application.Features.Commands.OrderFinalize
{
    public class LibraryOrderCommandHandler : IRequestHandler<LibraryOrderCommand, UserLibraryItemResponse>
    {
        private readonly IGameRepository _gameSnapshotRepository;
        private readonly ILibraryEntryRepository _libraryRepository;
        private readonly ILogger<LibraryOrderCommandHandler> _logger;
        private readonly IPublisher _publisher;

        public LibraryOrderCommandHandler(IGameRepository gameSnapshotRepository,
            ILibraryEntryRepository libraryRepository, 
            ILogger<LibraryOrderCommandHandler> logger, IPublisher publisher)
        {
            _gameSnapshotRepository = gameSnapshotRepository;
            _libraryRepository = libraryRepository;
            _logger = logger;
            _publisher = publisher;
        }
        public async Task<UserLibraryItemResponse> Handle(LibraryOrderCommand request, CancellationToken cancellationToken)
        {
            if(request.UserId == Guid.Empty)
                throw new DomainException(DomainMessages.UserNotFound);

            var gameSnapshot = await _gameSnapshotRepository.GetGameByIdAsync(request.GameId,cancellationToken);
            if (gameSnapshot == null)
                throw new DomainException(DomainMessages.GameNotFound);

            var response = new UserLibraryItemResponse
            {
                GameId = request.GameId,
                AcquiredAt = request.AcquiredAt,
                Description = gameSnapshot.Description,
                GameName = gameSnapshot.Name,
                Genre = gameSnapshot.Genre,
            };

            if (await _libraryRepository.ExistsAsync(request.UserId, request.GameId,cancellationToken))
            {
                _logger.LogInformation(
                    "[UserLibraryAPI] Jogo {GameId} já consta na biblioteca do Usuário {UserId}. Evento ignorado.",
                    request.GameId, request.UserId);

                return response;
            }

            var gamelibrary = new LibraryGame(gameSnapshot.Id, gameSnapshot.Name, gameSnapshot.Description, gameSnapshot.Genre);

            var customerLibrary = new LibraryCustomer(request.UserId);

            var userLibrary = new LibraryEntry(customerLibrary,gamelibrary);

            await  _libraryRepository.AddLibrary(userLibrary);
            
            _logger.LogInformation(
                "[UserLibraryAPI] Jogo {GameId} adicionado à biblioteca do Usuário {UserId}",
                request.GameId, request.UserId);

            await _publisher.Publish(new LibraryEntryEvent(request.UserId), cancellationToken);

            return response;
        }
    }
}
