namespace DynamicBingo.Domain.Entities;

public class Ban
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Reason { get; private set; }
    public int DurationSeconds { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public Guid? PreviousBanId { get; private set; }

    public User User { get; private set; } = null!;
    public Ban? PreviousBan { get; private set; }

    private Ban() { }

    public static Ban Create(Guid userId, string reason, TimeSpan duration, Guid? previousBanId = null)
    {
        var startsAt = DateTime.UtcNow;
        return new Ban
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Reason = reason,
            DurationSeconds = (int)duration.TotalSeconds,
            StartsAt = startsAt,
            EndsAt = startsAt.Add(duration),
            PreviousBanId = previousBanId
        };
    }

    public bool IsActive => DateTime.UtcNow >= StartsAt && DateTime.UtcNow < EndsAt;
    public bool HasExpired => DateTime.UtcNow >= EndsAt;
    public TimeSpan RemainingTime => HasExpired ? TimeSpan.Zero : EndsAt - DateTime.UtcNow;
    public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);
}
