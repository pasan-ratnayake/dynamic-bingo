using DynamicBingo.Domain.Enums;

namespace DynamicBingo.Domain.Entities;

public class OpenChallenge
{
    public Guid Id { get; private set; }
    public Guid CreatorId { get; private set; }
    public ChallengeVisibility Visibility { get; private set; }
    public string Word { get; private set; }
    public FillMode FillMode { get; private set; }
    public StarterChoice StarterChoice { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    public User Creator { get; private set; } = null!;

    private OpenChallenge() { }

    public static OpenChallenge Create(Guid creatorId, ChallengeVisibility visibility, string word, FillMode fillMode, StarterChoice starterChoice)
    {
        ValidateWord(word);

        return new OpenChallenge
        {
            Id = Guid.NewGuid(),
            CreatorId = creatorId,
            Visibility = visibility,
            Word = word.ToUpper(),
            FillMode = fillMode,
            StarterChoice = starterChoice,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void Cancel()
    {
        if (!IsActive)
            throw new InvalidOperationException("Challenge is already inactive");

        IsActive = false;
    }

    public Game AcceptChallenge(Guid opponentId)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot accept inactive challenge");

        if (opponentId == CreatorId)
            throw new InvalidOperationException("Creator cannot accept their own challenge");

        IsActive = false;

        return Game.Create(Word, CreatorId, FillMode, StarterChoice);
    }

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
