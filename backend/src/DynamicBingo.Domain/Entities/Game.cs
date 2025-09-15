using DynamicBingo.Domain.Enums;
using DynamicBingo.Domain.ValueObjects;

namespace DynamicBingo.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string Word { get; private set; }
    public int N { get; private set; }
    public Guid CreatorId { get; private set; }
    public Guid? OpponentId { get; private set; }
    public string SettingsJson { get; private set; }
    public GameStatus Status { get; private set; }
    public StarterChoice StarterChoice { get; private set; }
    public Guid? ResolvedStarterId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }

    public User Creator { get; private set; } = null!;
    public User? Opponent { get; private set; }
    public ICollection<GamePlayer> Players { get; private set; } = new List<GamePlayer>();
    public ICollection<Board> Boards { get; private set; } = new List<Board>();
    public ICollection<Mark> Marks { get; private set; } = new List<Mark>();
    public ICollection<Turn> Turns { get; private set; } = new List<Turn>();

    private Game() { }

    public static Game Create(string word, Guid creatorId, FillMode fillMode, StarterChoice starterChoice)
    {
        ValidateWord(word);

        var gameSettings = new GameSettings
        {
            FillMode = fillMode,
            StarterChoice = starterChoice
        };

        return new Game
        {
            Id = Guid.NewGuid(),
            Word = word.ToUpper(),
            N = word.Length,
            CreatorId = creatorId,
            SettingsJson = System.Text.Json.JsonSerializer.Serialize(gameSettings),
            Status = GameStatus.Pending,
            StarterChoice = starterChoice,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddOpponent(Guid opponentId)
    {
        if (Status != GameStatus.Pending)
            throw new InvalidOperationException("Cannot add opponent to a game that is not pending");

        if (OpponentId.HasValue)
            throw new InvalidOperationException("Game already has an opponent");

        if (opponentId == CreatorId)
            throw new InvalidOperationException("Creator cannot be their own opponent");

        OpponentId = opponentId;
    }

    public void Start()
    {
        if (Status != GameStatus.Pending)
            throw new InvalidOperationException("Only pending games can be started");

        if (!OpponentId.HasValue)
            throw new InvalidOperationException("Cannot start game without an opponent");

        Status = GameStatus.Active;
        StartedAt = DateTime.UtcNow;

        ResolvedStarterId = StarterChoice switch
        {
            StarterChoice.Creator => CreatorId,
            StarterChoice.Opponent => OpponentId.Value,
            StarterChoice.Random => new Random().Next(2) == 0 ? CreatorId : OpponentId.Value,
            _ => throw new InvalidOperationException("Invalid starter choice")
        };
    }

    public void End(GameEndReason reason, Guid? winnerId = null)
    {
        if (Status != GameStatus.Active)
            throw new InvalidOperationException("Only active games can be ended");

        Status = reason switch
        {
            GameEndReason.Win => GameStatus.Finished,
            GameEndReason.Draw => GameStatus.Draw,
            GameEndReason.Forfeit => GameStatus.Forfeited,
            _ => throw new ArgumentException("Invalid end reason")
        };

        FinishedAt = DateTime.UtcNow;

        if (winnerId.HasValue && reason == GameEndReason.Win)
        {
            var winnerPlayer = Players.FirstOrDefault(p => p.UserId == winnerId.Value);
            if (winnerPlayer != null)
            {
                winnerPlayer.SetAsWinner();
            }
        }
    }

    public bool HasStarted => StartedAt.HasValue;
    public bool IsFinished => Status is GameStatus.Finished or GameStatus.Draw or GameStatus.Forfeited;
    public int TotalMoves => Marks.Count;

    private static void ValidateWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            throw new ArgumentException("Word cannot be empty");

        if (word.Length < 4 || word.Length > 8)
            throw new ArgumentException("Word length must be between 4 and 8 characters");

        if (!System.Text.RegularExpressions.Regex.IsMatch(word, @"^[A-Za-z]+$"))
            throw new ArgumentException("Word can only contain letters");
    }
}
