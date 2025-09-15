using DynamicBingo.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DynamicBingo.Infrastructure.Services;

public class SignalRRealtimeTransport : IRealtimeTransport
{
    private readonly IHubContext<LobbyHub> _lobbyHubContext;
    private readonly IHubContext<GameHub> _gameHubContext;

    public SignalRRealtimeTransport(
        IHubContext<LobbyHub> lobbyHubContext,
        IHubContext<GameHub> gameHubContext)
    {
        _lobbyHubContext = lobbyHubContext;
        _gameHubContext = gameHubContext;
    }

    public async Task SendToUserAsync(Guid userId, string method, object data)
    {
        await _gameHubContext.Clients.User(userId.ToString()).SendAsync(method, data);
    }

    public async Task SendToGroupAsync(string groupName, string method, object data)
    {
        await _gameHubContext.Clients.Group(groupName).SendAsync(method, data);
    }

    public async Task AddToGroupAsync(string connectionId, string groupName)
    {
        await _gameHubContext.Groups.AddToGroupAsync(connectionId, groupName);
    }

    public async Task RemoveFromGroupAsync(string connectionId, string groupName)
    {
        await _gameHubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
    }

    public async Task SendToConnectionAsync(string connectionId, string method, object data)
    {
        await _gameHubContext.Clients.Client(connectionId).SendAsync(method, data);
    }
}

public class LobbyHub : Hub
{
}

public class GameHub : Hub
{
}
