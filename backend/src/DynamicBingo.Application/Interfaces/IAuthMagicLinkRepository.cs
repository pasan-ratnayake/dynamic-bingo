using DynamicBingo.Domain.Entities;

namespace DynamicBingo.Application.Interfaces;

public interface IAuthMagicLinkRepository
{
    Task<AuthMagicLink?> GetByTokenHashAsync(string tokenHash);
    Task<AuthMagicLink> CreateAsync(AuthMagicLink magicLink);
    Task UpdateAsync(AuthMagicLink magicLink);
    Task DeleteExpiredLinksAsync();
}
