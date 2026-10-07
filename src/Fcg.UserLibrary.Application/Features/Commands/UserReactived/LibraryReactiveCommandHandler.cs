using Fcg.Core.Abstractions.Interfaces;
using Fcg.UserLibrary.Domain.Events;
using Fcg.UserLibrary.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.UserLibrary.Application.Features.Commands.UserReactived
{
    public class LibraryReactiveCommandHandler : IRequestHandler<LibraryReactiveCommand>
    {
        private readonly ILibraryEntryRepository _libraryEntryRepository;
        private readonly ILogger<LibraryReactiveCommandHandler> _logger;
        private readonly IPublisher _publisher;

        public LibraryReactiveCommandHandler( ILibraryEntryRepository libraryEntryRepository, ILogger<LibraryReactiveCommandHandler> logger, IPublisher publisher)
        {
            _libraryEntryRepository = libraryEntryRepository;
            _logger = logger;
            _publisher = publisher;
        }

        public async Task Handle(LibraryReactiveCommand request, CancellationToken cancellationToken)
        {
            var libraryEntries = await _libraryEntryRepository.GetLibraryByUserIdAsync(request.UserId,cancellationToken);
            if (!libraryEntries.Any())
            {
                _logger.LogWarning("[UserLibraryAPI] Nenhuma biblioteca encontrada para o usuário {UserId}", request.UserId);
                return;
            }

            var entriesInactived = libraryEntries.Where(x => !x.IsActive).ToList();
            if (!entriesInactived.Any())
            {
                _logger.LogInformation("[UserLibraryAPI] A biblioteca já estava ativa para o usuário {UserId}", request.UserId);
                return;
            }

            foreach (var entry in entriesInactived)
            {
                entry.ReactiveLibraryEntry();

                await _libraryEntryRepository.UpdateLibrary(entry);
            }

            _logger.LogInformation(
                "[UserLibraryAPI] {QuantidadeEntradas} entrada(s) de biblioteca reativada(s) para o usuário {UserId}",
                entriesInactived.Count, request.UserId);

            await _publisher.Publish(new LibraryEntryEvent(request.UserId), cancellationToken);
        }
    }
}
