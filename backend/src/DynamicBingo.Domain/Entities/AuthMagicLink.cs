namespace DynamicBingo.Domain.Entities;

public class AuthMagicLink
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public User User { get; private set; } = null!;

    private AuthMagicLink() { }

    public static AuthMagicLink Create(Guid userId, string tokenHash, TimeSpan validFor, string? ipAddress = null, string? userAgent = null)
    {
        return new AuthMagicLink
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.Add(validFor),
            CreatedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
    }

    public void MarkAsUsed()
    {
        if (UsedAt.HasValue)
            throw new InvalidOperationException("Magic link has already been used");

        if (IsExpired)
            throw new InvalidOperationException("Magic link has expired");

        UsedAt = DateTime.UtcNow;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsUsed => UsedAt.HasValue;
    public bool IsValid => !IsExpired && !IsUsed;
}
