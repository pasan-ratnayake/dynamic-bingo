using DynamicBingo.Domain.Entities;

namespace DynamicBingo.Application.Interfaces;

public interface IFriendshipRepository
{
    Task<Friendship?> GetByIdAsync(Guid id);
    Task<IEnumerable<Friendship>> GetFriendshipsForUserAsync(Guid userId);
    Task<IEnumerable<Friendship>> GetPendingRequestsForUserAsync(Guid userId);
    Task<Friendship?> GetFriendshipBetweenUsersAsync(Guid userAId, Guid userBId);
    Task<Friendship> CreateAsync(Friendship friendship);
    Task UpdateAsync(Friendship friendship);
    Task DeleteAsync(Guid id);
}
