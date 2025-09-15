namespace DynamicBingo.Domain.Entities;

public class Mark
{
    public Guid Id { get; private set; }
    public Guid GameId { get; private set; }
    public int Number { get; private set; }
    public Guid MarkedByUserId { get; private set; }
    public DateTime MarkedAt { get; private set; }
    public int TurnIndex { get; private set; }

    public Game Game { get; private set; } = null!;
    public User MarkedByUser { get; private set; } = null!;

    private Mark() { }

    public static Mark Create(Guid gameId, int number, Guid markedByUserId, int turnIndex)
    {
        return new Mark
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            Number = number,
            MarkedByUserId = markedByUserId,
            MarkedAt = DateTime.UtcNow,
            TurnIndex = turnIndex
        };
    }
}
