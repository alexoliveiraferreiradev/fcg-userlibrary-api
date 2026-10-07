using Fcg.UserLibrary.Domain.Events;
using Fcg.UserLibrary.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.UserLibrary.Application.Features.Commands.UserDeactived
{
    public class LibraryDeactiveCommandHandler : IRequestHandler<LibraryDeactiveCommand>
    {
        private readonly ILibraryEntryRepository _libraryEntryRepository;
        private readonly ILogger<LibraryDeactiveCommandHandler> _logger;
        private readonly IPublisher _publisher;

        public LibraryDeactiveCommandHandler(ILibraryEntryRepository libraryEntryRepository, 
            ILogger<LibraryDeactiveCommandHandler> logger, IPublisher publisher)
        {
            _libraryEntryRepository = libraryEntryRepository;
            _logger = logger;
            _publisher = publisher;
        }
        public async Task Handle(LibraryDeactiveCommand request, CancellationToken cancellationToken)
        {
            var libraryEntries = await _libraryEntryRepository.GetLibraryByUserIdAsync(request.UserId, cancellationToken);

            if (!libraryEntries.Any())
            {
                _logger.LogWarning("[UserLibraryAPI] Nenhuma entrada de biblioteca encontrada para o usuário {UserId}", request.UserId);
                return;
            }

            var entriesActived = libraryEntries.Where(e => e.IsActive).ToList();
            if (!entriesActived.Any())
            {
                _logger.LogInformation("[UserLibraryAPI] Todas as entradas já estavam inativas para o usuário {UserId}", request.UserId);
                return;
            }

            foreach (var entry in entriesActived)
            {
                entry.DeactivateLibraryEntry();

                await _libraryEntryRepository.UpdateLibrary(entry);                
            }

            _logger.LogInformation(
                "[UserLibraryAPI] {QuantidadeEntradas} entrada(s) de biblioteca desativada(s) para o usuário {UserId}",
                entriesActived.Count, request.UserId);

            await _publisher.Publish(new LibraryEntryEvent(request.UserId), cancellationToken);
        }
    }
}
