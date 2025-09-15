using DynamicBingo.Domain.Enums;

namespace DynamicBingo.Domain.Entities;

public class Presence
{
    public Guid UserId { get; private set; }
    public PresenceStatus Status { get; private set; }
    public DateTime LastSeenAt { get; private set; }

    public User User { get; private set; } = null!;

    private Presence() { }

    public static Presence Create(Guid userId, PresenceStatus status = PresenceStatus.Available)
    {
        return new Presence
        {
            UserId = userId,
            Status = status,
            LastSeenAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(PresenceStatus status)
    {
        Status = status;
        LastSeenAt = DateTime.UtcNow;
    }

    public void UpdateLastSeen()
    {
        LastSeenAt = DateTime.UtcNow;
    }

    public bool IsOnline => DateTime.UtcNow - LastSeenAt < TimeSpan.FromMinutes(5);
}
