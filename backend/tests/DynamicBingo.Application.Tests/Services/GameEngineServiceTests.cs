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
    public async Task CheckForCompletedLines_ShouldDetectRowCompletion()
    {
        var gameId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var game = Game.Create("TEST", playerId, FillMode.Sequential, StarterChoice.Creator);
        
        var marks = new List<Mark>
        {
            new() { GameId = gameId, Number = 1, MarkedByUserId = playerId },
            new() { GameId = gameId, Number = 2, MarkedByUserId = playerId },
            new() { GameId = gameId, Number = 3, MarkedByUserId = playerId },
            new() { GameId = gameId, Number = 4, MarkedByUserId = playerId }
        };

        var completedLines = _gameEngineService.CheckForCompletedLines(game, marks, playerId);

        Assert.Single(completedLines);
        Assert.Equal("row", completedLines[0].Type);
        Assert.Equal(0, completedLines[0].Index);
    }

    [Fact]
    public async Task CheckForCompletedLines_ShouldDetectColumnCompletion()
    {
        var gameId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var game = Game.Create("TEST", playerId, FillMode.Sequential, StarterChoice.Creator);
        
        var marks = new List<Mark>
        {
            new() { GameId = gameId, Number = 1, MarkedByUserId = playerId },
            new() { GameId = gameId, Number = 5, MarkedByUserId = playerId },
            new() { GameId = gameId, Number = 9, MarkedByUserId = playerId },
            new() { GameId = gameId, Number = 13, MarkedByUserId = playerId }
        };

        var completedLines = _gameEngineService.CheckForCompletedLines(game, marks, playerId);

        Assert.Single(completedLines);
        Assert.Equal("column", completedLines[0].Type);
        Assert.Equal(0, completedLines[0].Index);
    }

    [Fact]
    public async Task CheckForCompletedLines_ShouldDetectDiagonalCompletion()
    {
        var gameId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var game = Game.Create("TEST", playerId, FillMode.Sequential, StarterChoice.Creator);
        
        var marks = new List<Mark>
        {
            new() { GameId = gameId, Number = 1, MarkedByUserId = playerId },
            new() { GameId = gameId, Number = 6, MarkedByUserId = playerId },
            new() { GameId = gameId, Number = 11, MarkedByUserId = playerId },
            new() { GameId = gameId, Number = 16, MarkedByUserId = playerId }
        };

        var completedLines = _gameEngineService.CheckForCompletedLines(game, marks, playerId);

        Assert.Single(completedLines);
        Assert.Equal("diagonal", completedLines[0].Type);
        Assert.Equal(0, completedLines[0].Index);
    }
}
