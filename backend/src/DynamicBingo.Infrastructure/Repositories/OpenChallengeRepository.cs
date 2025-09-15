using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;
using DynamicBingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DynamicBingo.Infrastructure.Repositories;

public class OpenChallengeRepository : IOpenChallengeRepository
{
    private readonly DynamicBingoDbContext _context;

    public OpenChallengeRepository(DynamicBingoDbContext context)
    {
        _context = context;
    }

    public async Task<OpenChallenge?> GetByIdAsync(Guid id)
    {
        return await _context.OpenChallenges
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<OpenChallenge>> GetActiveChallengesAsync()
    {
        return await _context.OpenChallenges
            .Include(c => c.Creator)
            .Where(c => c.IsActive)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<OpenChallenge>> GetPublicChallengesAsync()
    {
        return await _context.OpenChallenges
            .Include(c => c.Creator)
            .Where(c => c.IsActive && c.Visibility == ChallengeVisibility.Public)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<OpenChallenge>> GetFriendsChallengesAsync(Guid userId)
    {
        var friendIds = await _context.Friendships
            .Where(f => (f.UserAId == userId || f.UserBId == userId) && 
                       f.Status == FriendshipStatus.Accepted && 
                       f.DeletedAt == null)
            .Select(f => f.UserAId == userId ? f.UserBId : f.UserAId)
            .ToListAsync();

        return await _context.OpenChallenges
            .Include(c => c.Creator)
            .Where(c => c.IsActive && 
                       c.Visibility == ChallengeVisibility.Friends && 
                       friendIds.Contains(c.CreatorId))
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<OpenChallenge> CreateAsync(OpenChallenge challenge)
    {
        _context.OpenChallenges.Add(challenge);
        await _context.SaveChangesAsync();
        return challenge;
    }

    public async Task UpdateAsync(OpenChallenge challenge)
    {
        _context.OpenChallenges.Update(challenge);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var challenge = await _context.OpenChallenges.FindAsync(id);
        if (challenge != null)
        {
            _context.OpenChallenges.Remove(challenge);
            await _context.SaveChangesAsync();
        }
    }
}
