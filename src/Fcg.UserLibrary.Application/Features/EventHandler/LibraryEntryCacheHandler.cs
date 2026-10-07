using Fcg.UserLibrary.Application.Common.Interfaces;
using Fcg.UserLibrary.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.UserLibrary.Application.Features.EventHandler
{
    internal class LibraryEntryCacheHandler : INotificationHandler<LibraryEntryEvent>
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<LibraryEntryCacheHandler> _logger;
        public LibraryEntryCacheHandler(ICacheService cacheService, ILogger<LibraryEntryCacheHandler> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task Handle(LibraryEntryEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[UserLibraryAPI] [Cache] Iniciando invalidação de cache da biblioteca do UsuarioId: {UsuarioId}", notification.UserId);
            await _cacheService.RemoveByPrefixAsync($"library:u_{notification.UserId}");
            _logger.LogInformation("[UserLibraryAPI] [Cache] Cache da biblioteca invalidado com sucesso para UsuarioId: {UsuarioId}", notification.UserId);
        }
    }
}
