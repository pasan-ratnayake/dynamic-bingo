using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DynamicBingo.Infrastructure.Repositories;

public class BanRepository : IBanRepository
{
    private readonly DynamicBingoDbContext _context;

    public BanRepository(DynamicBingoDbContext context)
    {
        _context = context;
    }

    public async Task<Ban?> GetActiveBanForUserAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        return await _context.Bans
            .Where(b => b.UserId == userId && b.StartsAt <= now && b.EndsAt > now)
            .OrderByDescending(b => b.EndsAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Ban?> GetMostRecentBanForUserAsync(Guid userId, TimeSpan withinTimespan)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(withinTimespan);
        return await _context.Bans
            .Where(b => b.UserId == userId && b.StartsAt >= cutoffTime)
            .OrderByDescending(b => b.StartsAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Ban>> GetBansForUserAsync(Guid userId)
    {
        return await _context.Bans
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.StartsAt)
            .ToListAsync();
    }

    public async Task<Ban> CreateAsync(Ban ban)
    {
        _context.Bans.Add(ban);
        await _context.SaveChangesAsync();
        return ban;
    }

    public async Task UpdateAsync(Ban ban)
    {
        _context.Bans.Update(ban);
        await _context.SaveChangesAsync();
    }
}
