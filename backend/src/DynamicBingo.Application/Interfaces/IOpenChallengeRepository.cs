using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;

namespace DynamicBingo.Application.Interfaces;

public interface IOpenChallengeRepository
{
    Task<OpenChallenge?> GetByIdAsync(Guid id);
    Task<IEnumerable<OpenChallenge>> GetActiveChallengesAsync();
    Task<IEnumerable<OpenChallenge>> GetPublicChallengesAsync();
    Task<IEnumerable<OpenChallenge>> GetFriendsChallengesAsync(Guid userId);
    Task<OpenChallenge> CreateAsync(OpenChallenge challenge);
    Task UpdateAsync(OpenChallenge challenge);
    Task DeleteAsync(Guid id);
}
