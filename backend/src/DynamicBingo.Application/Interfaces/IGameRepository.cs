using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;

namespace DynamicBingo.Application.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id);
    Task<IEnumerable<Game>> GetOngoingGamesForUserAsync(Guid userId);
    Task<Game> CreateAsync(Game game);
    Task UpdateAsync(Game game);
    Task<IEnumerable<Game>> GetActiveGamesAsync();
    Task<int> GetTotalMovesAsync(Guid gameId);
}
