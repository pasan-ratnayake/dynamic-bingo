using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DynamicBingo.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DynamicBingoDbContext _context;

    public UserRepository(DynamicBingoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);
    }

    public async Task<IEnumerable<User>> GetOnlineUsersAsync()
    {
        var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
        return await _context.Users
            .Join(_context.Presences, u => u.Id, p => p.UserId, (u, p) => new { User = u, Presence = p })
            .Where(up => up.User.DeletedAt == null && up.Presence.LastSeenAt > fiveMinutesAgo)
            .Select(up => up.User)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.Delete();
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<User>> GetGuestsForCleanupAsync()
    {
        var twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);
        var oneMonthAgo = DateTime.UtcNow.AddDays(-30);

        return await _context.Users
            .Where(u => u.IsGuest && u.DeletedAt == null && 
                       (u.LastActiveAt < twentyFourHoursAgo || u.CreatedAt < oneMonthAgo))
            .ToListAsync();
    }
}
