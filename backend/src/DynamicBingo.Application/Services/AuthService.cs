using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace DynamicBingo.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthMagicLinkRepository _magicLinkRepository;
    private readonly IEmailSender _emailSender;
    private readonly ITimeProvider _timeProvider;

    public AuthService(
        IUserRepository userRepository,
        IAuthMagicLinkRepository magicLinkRepository,
        IEmailSender emailSender,
        ITimeProvider timeProvider)
    {
        _userRepository = userRepository;
        _magicLinkRepository = magicLinkRepository;
        _emailSender = emailSender;
        _timeProvider = timeProvider;
    }

    public async Task<bool> SendMagicLinkAsync(string email, string? ipAddress = null, string? userAgent = null)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            user = User.CreateRegistered(email, GenerateDefaultDisplayName(email));
            user = await _userRepository.CreateAsync(user);
        }

        if (user.IsDeleted)
            return false;

        var token = GenerateSecureToken();
        var tokenHash = HashToken(token);
        
        var magicLink = AuthMagicLink.Create(user.Id, tokenHash, TimeSpan.FromHours(1), ipAddress, userAgent);
        await _magicLinkRepository.CreateAsync(magicLink);

        var magicLinkUrl = $"https://your-domain.com/auth/magic-link?token={token}";
        await _emailSender.SendMagicLinkAsync(email, magicLinkUrl);

        return true;
    }

    public async Task<User?> ConsumeMagicLinkAsync(string token)
    {
        var tokenHash = HashToken(token);
        var magicLink = await _magicLinkRepository.GetByTokenHashAsync(tokenHash);
        
        if (magicLink == null || !magicLink.IsValid)
            return null;

        magicLink.MarkAsUsed();
        await _magicLinkRepository.UpdateAsync(magicLink);

        var user = await _userRepository.GetByIdAsync(magicLink.UserId);
        if (user != null && !user.IsDeleted)
        {
            user.UpdateLastActive();
            await _userRepository.UpdateAsync(user);
        }

        return user;
    }

    public async Task<User> CreateGuestAsync(string displayName)
    {
        var user = User.CreateGuest(displayName);
        return await _userRepository.CreateAsync(user);
    }

    public async Task<(User User, string Token)> CreateGuestWithTokenAsync(string displayName)
    {
        var user = User.CreateGuest(displayName);
        var createdUser = await _userRepository.CreateAsync(user);
        var token = GenerateAccessToken(createdUser);
        return (createdUser, token);
    }

    private string GenerateAccessToken(User user)
    {
        var tokenData = $"{user.Id}:{user.DisplayName}:{DateTime.UtcNow:O}";
        var bytes = Encoding.UTF8.GetBytes(tokenData);
        return Convert.ToBase64String(bytes);
    }

    public async Task<bool> ConvertGuestToRegisteredAsync(Guid guestId, string email, string? ipAddress = null, string? userAgent = null)
    {
        var user = await _userRepository.GetByIdAsync(guestId);
        if (user == null || !user.IsGuest || user.IsDeleted)
            return false;

        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
            return false;

        var token = GenerateSecureToken();
        var tokenHash = HashToken(token);
        
        var magicLink = AuthMagicLink.Create(user.Id, tokenHash, TimeSpan.FromHours(1), ipAddress, userAgent);
        await _magicLinkRepository.CreateAsync(magicLink);

        var magicLinkUrl = $"https://your-domain.com/auth/convert-guest?token={token}&email={Uri.EscapeDataString(email)}";
        await _emailSender.SendMagicLinkAsync(email, magicLinkUrl);

        return true;
    }

    public async Task<User?> CompleteGuestConversionAsync(string token, string email)
    {
        var tokenHash = HashToken(token);
        var magicLink = await _magicLinkRepository.GetByTokenHashAsync(tokenHash);
        
        if (magicLink == null || !magicLink.IsValid)
            return null;

        var user = await _userRepository.GetByIdAsync(magicLink.UserId);
        if (user == null || !user.IsGuest || user.IsDeleted)
            return null;

        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
            return null;

        user.ConvertToRegistered(email);
        magicLink.MarkAsUsed();

        await _userRepository.UpdateAsync(user);
        await _magicLinkRepository.UpdateAsync(magicLink);

        return user;
    }

    public async Task CleanupExpiredMagicLinksAsync()
    {
        await _magicLinkRepository.DeleteExpiredLinksAsync();
    }

    public async Task CleanupGuestAccountsAsync()
    {
        var guestsToCleanup = await _userRepository.GetGuestsForCleanupAsync();
        
        foreach (var guest in guestsToCleanup)
        {
            await _userRepository.DeleteAsync(guest.Id);
        }
    }

    private string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private string GenerateDefaultDisplayName(string email)
    {
        var username = email.Split('@')[0];
        return username.Length > 30 ? username.Substring(0, 30) : username;
    }
}
