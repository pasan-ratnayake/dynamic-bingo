using DynamicBingo.Domain.Enums;

namespace DynamicBingo.Domain.Entities;

public class Friendship
{
    public Guid Id { get; private set; }
    public Guid UserAId { get; private set; }
    public Guid UserBId { get; private set; }
    public FriendshipStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public User UserA { get; private set; } = null!;
    public User UserB { get; private set; } = null!;

    private Friendship() { }

    public static Friendship Create(Guid requesterId, Guid targetId)
    {
        if (requesterId == targetId)
            throw new ArgumentException("Cannot create friendship with self");

        return new Friendship
        {
            Id = Guid.NewGuid(),
            UserAId = requesterId,
            UserBId = targetId,
            Status = FriendshipStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Accept()
    {
        if (Status != FriendshipStatus.Pending)
            throw new InvalidOperationException("Only pending friendships can be accepted");

        Status = FriendshipStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
    }

    public void Block()
    {
        Status = FriendshipStatus.Blocked;
    }

    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
    }

    public bool IsDeleted => DeletedAt.HasValue;
    public bool IsAccepted => Status == FriendshipStatus.Accepted && !IsDeleted;
    public bool IsPending => Status == FriendshipStatus.Pending && !IsDeleted;
    public bool IsBlocked => Status == FriendshipStatus.Blocked && !IsDeleted;

    public Guid GetOtherUserId(Guid userId)
    {
        if (userId == UserAId) return UserBId;
        if (userId == UserBId) return UserAId;
        throw new ArgumentException("User is not part of this friendship");
    }
}
