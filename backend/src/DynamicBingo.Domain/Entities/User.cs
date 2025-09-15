using DynamicBingo.Domain.ValueObjects;

namespace DynamicBingo.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string? Email { get; private set; }
    public string DisplayName { get; private set; }
    public bool IsGuest { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastActiveAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public ICollection<Friendship> InitiatedFriendships { get; private set; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedFriendships { get; private set; } = new List<Friendship>();
    public ICollection<Game> CreatedGames { get; private set; } = new List<Game>();
    public ICollection<GamePlayer> GamePlayers { get; private set; } = new List<GamePlayer>();
    public ICollection<AuthMagicLink> MagicLinks { get; private set; } = new List<AuthMagicLink>();
    public ICollection<Ban> Bans { get; private set; } = new List<Ban>();

    private User() { }

    public static User CreateRegistered(string email, string displayName)
    {
        ValidateDisplayName(displayName);
        ValidateEmail(email);

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = displayName,
            IsGuest = false,
            CreatedAt = DateTime.UtcNow,
            LastActiveAt = DateTime.UtcNow
        };
    }

    public static User CreateGuest(string displayName)
    {
        ValidateDisplayName(displayName);

        return new User
        {
            Id = Guid.NewGuid(),
            Email = null,
            DisplayName = displayName,
            IsGuest = true,
            CreatedAt = DateTime.UtcNow,
            LastActiveAt = DateTime.UtcNow
        };
    }

    public void ConvertToRegistered(string email)
    {
        if (!IsGuest)
            throw new InvalidOperationException("Only guest users can be converted to registered users");

        ValidateEmail(email);
        Email = email;
        IsGuest = false;
    }

    public void UpdateDisplayName(string displayName)
    {
        ValidateDisplayName(displayName);
        DisplayName = displayName;
    }

    public void UpdateEmail(string email)
    {
        if (IsGuest)
            throw new InvalidOperationException("Guest users cannot have email addresses");

        ValidateEmail(email);
        Email = email;
    }

    public void UpdateLastActive()
    {
        LastActiveAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
    }

    public bool IsDeleted => DeletedAt.HasValue;

    public bool ShouldBeCleanedUp()
    {
        if (!IsGuest) return false;
        
        var inactiveFor = DateTime.UtcNow - LastActiveAt;
        var totalAge = DateTime.UtcNow - CreatedAt;
        
        return inactiveFor > TimeSpan.FromHours(24) || totalAge > TimeSpan.FromDays(30);
    }

    private static void ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty");

        if (displayName.Length > 30)
            throw new ArgumentException("Display name cannot exceed 30 characters");

        if (!System.Text.RegularExpressions.Regex.IsMatch(displayName, @"^[A-Za-z0-9_\-\$\@\^.\(\)\[\]\{\}!]{1,30}$"))
            throw new ArgumentException("Display name contains invalid characters");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty");

        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Invalid email format");
    }
}
