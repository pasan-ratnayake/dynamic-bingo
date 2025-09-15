using Xunit;
using Moq;
using DynamicBingo.Application.Services;
using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;

namespace DynamicBingo.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuthMagicLinkRepository> _magicLinkRepositoryMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<ITimeProvider> _timeProviderMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _magicLinkRepositoryMock = new Mock<IAuthMagicLinkRepository>();
        _emailSenderMock = new Mock<IEmailSender>();
        _timeProviderMock = new Mock<ITimeProvider>();
        _authService = new AuthService(
            _userRepositoryMock.Object,
            _magicLinkRepositoryMock.Object,
            _emailSenderMock.Object,
            _timeProviderMock.Object);
    }

    [Fact]
    public async Task CreateGuestAsync_ShouldCreateGuestUser()
    {
        var displayName = "TestGuest";
        _timeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        var result = await _authService.CreateGuestAsync(displayName);

        Assert.NotNull(result.User);
        Assert.True(result.User.IsGuest);
        Assert.Equal(displayName, result.User.DisplayName);
        Assert.NotNull(result.Token);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task SendMagicLinkAsync_WithNewEmail_ShouldCreateUserAndSendLink()
    {
        var email = "test@example.com";
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);
        _timeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        await _authService.SendMagicLinkAsync(email);

        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        _magicLinkRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<AuthMagicLink>()), Times.Once);
        _emailSenderMock.Verify(x => x.SendMagicLinkAsync(email, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SendMagicLinkAsync_WithExistingEmail_ShouldSendLinkOnly()
    {
        var email = "test@example.com";
        var existingUser = User.CreateRegistered(email, "TestUser");
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);
        _timeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        await _authService.SendMagicLinkAsync(email);

        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        _magicLinkRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<AuthMagicLink>()), Times.Once);
        _emailSenderMock.Verify(x => x.SendMagicLinkAsync(email, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ConsumeMagicLinkAsync_WithValidToken_ShouldReturnUser()
    {
        var token = "valid-token";
        var user = User.CreateRegistered("test@example.com", "TestUser");
        var magicLink = AuthMagicLink.Create(user.Id, token, DateTime.UtcNow.AddHours(1));
        
        _magicLinkRepositoryMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(magicLink);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _timeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        var result = await _authService.ConsumeMagicLinkAsync(token);

        Assert.NotNull(result.User);
        Assert.Equal(user.Id, result.User.Id);
        Assert.NotNull(result.Token);
        _magicLinkRepositoryMock.Verify(x => x.UpdateAsync(magicLink), Times.Once);
    }

    [Fact]
    public async Task ConsumeMagicLinkAsync_WithExpiredToken_ShouldReturnNull()
    {
        var token = "expired-token";
        var user = User.CreateRegistered("test@example.com", "TestUser");
        var magicLink = AuthMagicLink.Create(user.Id, token, DateTime.UtcNow.AddHours(-1));
        
        _magicLinkRepositoryMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(magicLink);
        _timeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        var result = await _authService.ConsumeMagicLinkAsync(token);

        Assert.Null(result);
    }

    [Fact]
    public async Task ConvertGuestToRegisteredAsync_ShouldUpdateUserAndSendMagicLink()
    {
        var guestId = Guid.NewGuid();
        var email = "test@example.com";
        var guest = User.CreateGuest("TestGuest");
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(guestId))
            .ReturnsAsync(guest);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);
        _timeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        await _authService.ConvertGuestToRegisteredAsync(guestId, email);

        Assert.False(guest.IsGuest);
        Assert.Equal(email, guest.Email);
        _userRepositoryMock.Verify(x => x.UpdateAsync(guest), Times.Once);
        _magicLinkRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<AuthMagicLink>()), Times.Once);
        _emailSenderMock.Verify(x => x.SendMagicLinkAsync(email, It.IsAny<string>()), Times.Once);
    }
}
