namespace DynamicBingo.Domain.Entities;

public class GamePlayer
{
    public Guid Id { get; private set; }
    public Guid GameId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsCreator { get; private set; }
    public int IdleCount { get; private set; }
    public int Score { get; private set; }
    public int BingoLettersCrossed { get; private set; }
    public bool IsWinner { get; private set; }
    public string? ForfeitReason { get; private set; }

    public Game Game { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private GamePlayer() { }

    public static GamePlayer Create(Guid gameId, Guid userId, bool isCreator)
    {
        return new GamePlayer
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            UserId = userId,
            IsCreator = isCreator,
            IdleCount = 0,
            Score = 0,
            BingoLettersCrossed = 0,
            IsWinner = false
        };
    }

    public void IncrementIdleCount()
    {
        IdleCount++;
    }

    public void AddScore(int points)
    {
        if (points <= 0)
            throw new ArgumentException("Points must be positive");

        Score += points;
        BingoLettersCrossed = Math.Min(BingoLettersCrossed + points, 5);
    }

    public void SetAsWinner()
    {
        IsWinner = true;
    }

    public void Forfeit(string reason)
    {
        ForfeitReason = reason;
    }

    public bool HasWon => Score >= 5;
    public bool HasForfeited => !string.IsNullOrEmpty(ForfeitReason);
}
