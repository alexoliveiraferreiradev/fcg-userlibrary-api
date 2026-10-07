using Fcg.UserLibrary.Domain.Entities;
using Fcg.UserLibrary.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.UserLibrary.Application.IntegrationEvent.Handlers.CreatedGame
{
    public class GameCreatedCommandHandler : IRequestHandler<GameCreatedCommand>
    {
        private readonly IGameRepository _gameRepository;
        private readonly ILogger<GameCreatedCommandHandler> _logger;

        public GameCreatedCommandHandler(IGameRepository gameRepository,
            ILogger<GameCreatedCommandHandler> logger)
        {
            _gameRepository = gameRepository;
            _logger = logger;
        }

        public async Task Handle(GameCreatedCommand request, CancellationToken cancellationToken)
        {
            var game = await _gameRepository.GetGameByIdAsync(request.GameId,cancellationToken);

            if (game is null)
            {
                game = new GameSnapshot(request.GameId, request.Name, request.Description, 
                    request.Genre, request.OccurredAt);

               var guidGame = await _gameRepository.AddSnapshot(game);
                _logger.LogInformation("[UserLibraryAPI] Jogo adicionado com sucesso com o {Id}", guidGame);
            }
            else
            {
                game.ApplyChanges(request.Name, request.Description, request.Genre, request.OccurredAt);

                var sucess = await _gameRepository.UpdateSnapshot(game);

                if(sucess)
                {
                    _logger.LogInformation("[UserLibraryAPI] Jogo atualizado com sucesso para com o {Id}", game.Id);
                }
                else
                {
                    _logger.LogWarning("[UserLibraryAPI] Erro ao atualizar o jogo com o {Id}", game.Id);
                }

            }

        }
    }
}
