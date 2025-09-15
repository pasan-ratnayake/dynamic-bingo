using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;

namespace DynamicBingo.WebApi.Hubs;

[Authorize]
public class LobbyHub : Hub
{
    private readonly IUserRepository _userRepository;
    private readonly IOpenChallengeRepository _challengeRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IGameRepository _gameRepository;

    public LobbyHub(
        IUserRepository userRepository,
        IOpenChallengeRepository challengeRepository,
        IFriendshipRepository friendshipRepository,
        IGameRepository gameRepository)
    {
        _userRepository = userRepository;
        _challengeRepository = challengeRepository;
        _friendshipRepository = friendshipRepository;
        _gameRepository = gameRepository;
    }

    public async Task CreateOpenChallenge(CreateChallengeRequest request)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        var challenge = OpenChallenge.Create(
            Guid.Parse(userId),
            Enum.Parse<ChallengeVisibility>(request.Visibility),
            request.Word,
            Enum.Parse<FillMode>(request.FillMode),
            Enum.Parse<StarterChoice>(request.StarterChoice)
        );

        await _challengeRepository.CreateAsync(challenge);
        await Clients.All.SendAsync("ChallengeCreated", challenge);
    }

    public async Task CancelOpenChallenge(string challengeId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        var challenge = await _challengeRepository.GetByIdAsync(Guid.Parse(challengeId));
        if (challenge?.CreatorId == Guid.Parse(userId))
        {
            challenge.Cancel();
            await _challengeRepository.UpdateAsync(challenge);
            await Clients.All.SendAsync("ChallengeCancelled", challengeId);
        }
    }

    public async Task<AcceptChallengeResponse> AcceptChallenge(string challengeId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return new AcceptChallengeResponse { GameId = "" };

        var challenge = await _challengeRepository.GetByIdAsync(Guid.Parse(challengeId));
        if (challenge?.IsActive != true || challenge.CreatorId == Guid.Parse(userId))
            return new AcceptChallengeResponse { GameId = "" };

        var game = challenge.AcceptChallenge(Guid.Parse(userId));

        await _gameRepository.CreateAsync(game);

        challenge.Cancel();
        await _challengeRepository.UpdateAsync(challenge);

        var response = new AcceptChallengeResponse { GameId = game.Id.ToString() };
        await Clients.All.SendAsync("ChallengeAccepted", new { challengeId, gameId = game.Id.ToString() });

        return response;
    }

    public async Task SendFriendRequest(string targetUserId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId) || userId == targetUserId) return;

        var existingFriendship = await _friendshipRepository.GetFriendshipsForUserAsync(Guid.Parse(userId));
        if (existingFriendship?.Any(f => f.UserAId == Guid.Parse(targetUserId) || f.UserBId == Guid.Parse(targetUserId)) == true) return;

        var friendship = Friendship.Create(Guid.Parse(userId), Guid.Parse(targetUserId));

        await _friendshipRepository.CreateAsync(friendship);
        await Clients.User(targetUserId).SendAsync("FriendRequestReceived", friendship);
    }

    public async Task RespondFriendRequest(string requestId, bool accept)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        var friendship = await _friendshipRepository.GetByIdAsync(Guid.Parse(requestId));
        if (friendship?.UserBId != Guid.Parse(userId) || friendship.Status != FriendshipStatus.Pending)
            return;

        if (accept)
        {
            friendship.Accept();
        }
        else
        {
            friendship.Block();
        }

        await _friendshipRepository.UpdateAsync(friendship);
        await Clients.Users(new[] { friendship.UserAId.ToString(), friendship.UserBId.ToString() })
            .SendAsync("FriendRequestUpdated", friendship);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            var users = await _userRepository.GetOnlineUsersAsync();
            await Clients.All.SendAsync("LobbyUsersUpdated", users);
            await Clients.All.SendAsync("PresenceUpdated", new { userId, status = "Available" });
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            var users = await _userRepository.GetOnlineUsersAsync();
            await Clients.All.SendAsync("LobbyUsersUpdated", users);
        }
        await base.OnDisconnectedAsync(exception);
    }
}

public class CreateChallengeRequest
{
    public string Visibility { get; set; } = "";
    public string Word { get; set; } = "";
    public string FillMode { get; set; } = "";
    public string StarterChoice { get; set; } = "";
}

public class AcceptChallengeResponse
{
    public string GameId { get; set; } = "";
}
