using Xunit;
using Moq;
using DynamicBingo.Application.Services;
using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;

namespace DynamicBingo.Application.Tests.Services;

public class GameEngineServiceTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IRealtimeTransport> _realtimeTransportMock;
    private readonly Mock<ITimeProvider> _timeProviderMock;
    private readonly GameEngineService _gameEngineService;

    public GameEngineServiceTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _realtimeTransportMock = new Mock<IRealtimeTransport>();
        _timeProviderMock = new Mock<ITimeProvider>();
        _gameEngineService = new GameEngineService(
            _gameRepositoryMock.Object,
            _realtimeTransportMock.Object,
            _timeProviderMock.Object);
    }

    [Fact]
    public async Task MarkNumberAsync_WithValidMove_ShouldReturnTrue()
    {
        var gameId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var opponentId = Guid.NewGuid();
        var game = Game.Create("BINGO", playerId, FillMode.Sequential, StarterChoice.Creator);
        game.AddOpponent(opponentId);
        game.Start();

        _gameRepositoryMock.Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);
        _timeProviderMock.Setup(x => x.UtcNow)
            .Returns(DateTime.UtcNow);

        var result = await _gameEngineService.MarkNumberAsync(gameId, playerId, 1);

        Assert.True(result);
        _gameRepositoryMock.Verify(x => x.UpdateAsync(game), Times.Once);
    }

    [Fact]
    public async Task MarkNumberAsync_WithInvalidGame_ShouldReturnFalse()
    {
        var gameId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        _gameRepositoryMock.Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync((Game?)null);

        var result = await _gameEngineService.MarkNumberAsync(gameId, playerId, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task MarkNumberAsync_WithFinishedGame_ShouldReturnFalse()
    {
        var gameId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var game = Game.Create("BINGO", playerId, FillMode.Sequential, StarterChoice.Creator);
        game.End(GameEndReason.Win);

        _gameRepositoryMock.Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        var result = await _gameEngineService.MarkNumberAsync(gameId, playerId, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task MarkNumberAsync_ShouldCalculateScoreCorrectly()
    {
        var gameId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var opponentId = Guid.NewGuid();
        var game = Game.Create("TEST", creatorId, FillMode.Sequential, StarterChoice.Creator);
        game.AddOpponent(opponentId);
        game.Start();

        var creatorPlayer = GamePlayer.Create(gameId, creatorId, true);
        var opponentPlayer = GamePlayer.Create(gameId, opponentId, false);
        game.Players.Add(creatorPlayer);
        game.Players.Add(opponentPlayer);

        var creatorBoard = Board.Create(gameId, creatorId, FillMode.Sequential, 4);
        var opponentBoard = Board.Create(gameId, opponentId, FillMode.Sequential, 4);
        game.Boards.Add(creatorBoard);
        game.Boards.Add(opponentBoard);

        var turn = Turn.Create(gameId, 0, creatorId, TimeSpan.FromSeconds(30));
        game.Turns.Add(turn);

        _gameRepositoryMock.Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);
        _timeProviderMock.Setup(x => x.UtcNow)
            .Returns(DateTime.UtcNow);

        var result = await _gameEngineService.MarkNumberAsync(gameId, creatorId, 1);

        Assert.True(result);
        Assert.Single(game.Marks);
        Assert.Equal(1, game.Marks.First().Number);
        _gameRepositoryMock.Verify(x => x.UpdateAsync(game), Times.Once);
    }
}
