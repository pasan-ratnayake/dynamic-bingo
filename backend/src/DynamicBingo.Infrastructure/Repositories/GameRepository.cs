using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;
using DynamicBingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DynamicBingo.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly DynamicBingoDbContext _context;

    public GameRepository(DynamicBingoDbContext context)
    {
        _context = context;
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        return await _context.Games
            .Include(g => g.Creator)
            .Include(g => g.Opponent)
            .Include(g => g.Players)
                .ThenInclude(p => p.User)
            .Include(g => g.Boards)
            .Include(g => g.Marks)
            .Include(g => g.Turns)
                .ThenInclude(t => t.PlayerToMove)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Game>> GetOngoingGamesForUserAsync(Guid userId)
    {
        return await _context.Games
            .Include(g => g.Creator)
            .Include(g => g.Opponent)
            .Where(g => (g.CreatorId == userId || g.OpponentId == userId) && 
                       g.Status == GameStatus.Active)
            .ToListAsync();
    }

    public async Task<Game> CreateAsync(Game game)
    {
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        return game;
    }

    public async Task UpdateAsync(Game game)
    {
        _context.Games.Update(game);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Game>> GetActiveGamesAsync()
    {
        return await _context.Games
            .Include(g => g.Creator)
            .Include(g => g.Opponent)
            .Where(g => g.Status == GameStatus.Active)
            .ToListAsync();
    }

    public async Task<int> GetTotalMovesAsync(Guid gameId)
    {
        return await _context.Marks.CountAsync(m => m.GameId == gameId);
    }
}
