using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;
using DynamicBingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DynamicBingo.Infrastructure.Repositories;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly DynamicBingoDbContext _context;

    public FriendshipRepository(DynamicBingoDbContext context)
    {
        _context = context;
    }

    public async Task<Friendship?> GetByIdAsync(Guid id)
    {
        return await _context.Friendships
            .Include(f => f.UserA)
            .Include(f => f.UserB)
            .FirstOrDefaultAsync(f => f.Id == id && f.DeletedAt == null);
    }

    public async Task<IEnumerable<Friendship>> GetFriendshipsForUserAsync(Guid userId)
    {
        return await _context.Friendships
            .Include(f => f.UserA)
            .Include(f => f.UserB)
            .Where(f => (f.UserAId == userId || f.UserBId == userId) && 
                       f.Status == FriendshipStatus.Accepted && 
                       f.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<IEnumerable<Friendship>> GetPendingRequestsForUserAsync(Guid userId)
    {
        return await _context.Friendships
            .Include(f => f.UserA)
            .Include(f => f.UserB)
            .Where(f => f.UserBId == userId && 
                       f.Status == FriendshipStatus.Pending && 
                       f.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<Friendship?> GetFriendshipBetweenUsersAsync(Guid userAId, Guid userBId)
    {
        return await _context.Friendships
            .FirstOrDefaultAsync(f => ((f.UserAId == userAId && f.UserBId == userBId) ||
                                      (f.UserAId == userBId && f.UserBId == userAId)) &&
                                     f.DeletedAt == null);
    }

    public async Task<Friendship> CreateAsync(Friendship friendship)
    {
        _context.Friendships.Add(friendship);
        await _context.SaveChangesAsync();
        return friendship;
    }

    public async Task UpdateAsync(Friendship friendship)
    {
        _context.Friendships.Update(friendship);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var friendship = await _context.Friendships.FindAsync(id);
        if (friendship != null)
        {
            friendship.Delete();
            await _context.SaveChangesAsync();
        }
    }
}
