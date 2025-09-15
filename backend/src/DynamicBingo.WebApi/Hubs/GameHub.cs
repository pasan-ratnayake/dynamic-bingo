using Microsoft.AspNetCore.SignalR;
using DynamicBingo.Application.Interfaces;
using DynamicBingo.Application.Services;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;

namespace DynamicBingo.WebApi.Hubs;

public class GameHub : Hub
{
    private readonly IGameRepository _gameRepository;
    private readonly GameEngineService _gameEngine;
    private readonly ITimeProvider _timeProvider;

    public GameHub(
        IGameRepository gameRepository,
        GameEngineService gameEngine,
        ITimeProvider timeProvider)
    {
        _gameRepository = gameRepository;
        _gameEngine = gameEngine;
        _timeProvider = timeProvider;
    }

    public async Task JoinGame(string gameId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"Game_{gameId}");

        var game = await _gameRepository.GetByIdAsync(Guid.Parse(gameId));
        if (game == null) return;

        await Clients.Caller.SendAsync("GameState", new { game });

        if (game.Status == GameStatus.Pending && game.OpponentId.HasValue)
        {
            game.Start();
            await _gameRepository.UpdateAsync(game);
            await Clients.Group($"Game_{gameId}").SendAsync("GameState", new { game });
        }
    }

    public async Task SubmitManualBoard(string gameId, int[] layout)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        var game = await _gameRepository.GetByIdAsync(Guid.Parse(gameId));
        if (game?.Status == GameStatus.Pending)
        {
            await _gameRepository.UpdateAsync(game);
        }
    }

    public async Task MarkNumber(string gameId, int number)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        try
        {
            var success = await _gameEngine.MarkNumberAsync(Guid.Parse(gameId), Guid.Parse(userId), number);
            
            if (success)
            {
                await Clients.Group($"Game_{gameId}").SendAsync("NumberMarked", new
                {
                    byUserId = userId,
                    number,
                    turnIndex = 0,
                    auto = false
                });
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task OfferRematch(string gameId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        await Clients.Group($"Game_{gameId}").SendAsync("RematchOffered", new { byUserId = userId });
    }

    public async Task AcceptRematch(string gameId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        var originalGame = await _gameRepository.GetByIdAsync(Guid.Parse(gameId));
        if (originalGame?.IsFinished == true)
        {
            var newGame = Game.Create(originalGame.Word, originalGame.CreatorId, FillMode.Random, originalGame.StarterChoice);
            if (originalGame.OpponentId.HasValue)
            {
                newGame.AddOpponent(originalGame.OpponentId.Value);
            }
            await _gameRepository.CreateAsync(newGame);
            await Clients.Group($"Game_{gameId}").SendAsync("RematchReady", new { newGameId = newGame.Id.ToString() });
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
