using DynamicBingo.Domain.Entities;

namespace DynamicBingo.Application.Interfaces;

public interface IBanRepository
{
    Task<Ban?> GetActiveBanForUserAsync(Guid userId);
    Task<Ban?> GetMostRecentBanForUserAsync(Guid userId, TimeSpan withinTimespan);
    Task<IEnumerable<Ban>> GetBansForUserAsync(Guid userId);
    Task<Ban> CreateAsync(Ban ban);
    Task UpdateAsync(Ban ban);
}
