using DynamicBingo.Domain.Enums;

namespace DynamicBingo.Domain.Entities;

public class Turn
{
    public Guid Id { get; private set; }
    public Guid GameId { get; private set; }
    public int Index { get; private set; }
    public Guid PlayerToMoveId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public TurnOutcome? Outcome { get; private set; }
    public int? MarkedNumber { get; private set; }

    public Game Game { get; private set; } = null!;
    public User PlayerToMove { get; private set; } = null!;

    private Turn() { }

    public static Turn Create(Guid gameId, int index, Guid playerToMoveId, TimeSpan turnDuration)
    {
        var startedAt = DateTime.UtcNow;
        return new Turn
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            Index = index,
            PlayerToMoveId = playerToMoveId,
            StartedAt = startedAt,
            ExpiresAt = startedAt.Add(turnDuration)
        };
    }

    public void Resolve(TurnOutcome outcome, int? markedNumber = null)
    {
        if (ResolvedAt.HasValue)
            throw new InvalidOperationException("Turn has already been resolved");

        Outcome = outcome;
        MarkedNumber = markedNumber;
        ResolvedAt = DateTime.UtcNow;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsResolved => ResolvedAt.HasValue;
    public TimeSpan RemainingTime => IsExpired ? TimeSpan.Zero : ExpiresAt - DateTime.UtcNow;
}
