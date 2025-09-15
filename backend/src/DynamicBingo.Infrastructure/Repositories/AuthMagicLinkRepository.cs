using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DynamicBingo.Infrastructure.Repositories;

public class AuthMagicLinkRepository : IAuthMagicLinkRepository
{
    private readonly DynamicBingoDbContext _context;

    public AuthMagicLinkRepository(DynamicBingoDbContext context)
    {
        _context = context;
    }

    public async Task<AuthMagicLink?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.AuthMagicLinks
            .Include(ml => ml.User)
            .FirstOrDefaultAsync(ml => ml.TokenHash == tokenHash);
    }

    public async Task<AuthMagicLink> CreateAsync(AuthMagicLink magicLink)
    {
        _context.AuthMagicLinks.Add(magicLink);
        await _context.SaveChangesAsync();
        return magicLink;
    }

    public async Task UpdateAsync(AuthMagicLink magicLink)
    {
        _context.AuthMagicLinks.Update(magicLink);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredLinksAsync()
    {
        var expiredLinks = await _context.AuthMagicLinks
            .Where(ml => ml.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        _context.AuthMagicLinks.RemoveRange(expiredLinks);
        await _context.SaveChangesAsync();
    }
}
