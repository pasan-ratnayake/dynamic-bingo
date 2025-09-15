using Xunit;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;

namespace DynamicBingo.Domain.Tests.Entities;

public class BoardTests
{
    [Fact]
    public void Create_WithSequentialFill_ShouldCreateOrderedLayout()
    {
        var gameId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var boardSize = 5;

        var board = Board.Create(gameId, userId, boardSize, FillMode.Sequential);

        Assert.Equal(gameId, board.GameId);
        Assert.Equal(userId, board.UserId);
        Assert.Equal(FillMode.Sequential, board.FillMode);
        Assert.Equal(25, board.Layout.Length);
        
        for (int i = 0; i < 25; i++)
        {
            Assert.Equal(i + 1, board.Layout[i]);
        }
    }

    [Fact]
    public void Create_WithRandomFill_ShouldCreateValidLayout()
    {
        var gameId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var boardSize = 5;

        var board = Board.Create(gameId, userId, boardSize, FillMode.Random);

        Assert.Equal(25, board.Layout.Length);
        Assert.True(board.Layout.All(n => n >= 1 && n <= 25));
        Assert.Equal(25, board.Layout.Distinct().Count());
    }

    [Fact]
    public void Create_WithManualFill_ShouldCreateEmptyLayout()
    {
        var gameId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var boardSize = 4;

        var board = Board.Create(gameId, userId, boardSize, FillMode.Manual);

        Assert.Equal(16, board.Layout.Length);
        Assert.True(board.Layout.All(n => n == 0));
    }

    [Fact]
    public void SetManualLayout_ShouldUpdateLayout()
    {
        var board = Board.Create(Guid.NewGuid(), Guid.NewGuid(), 4, FillMode.Manual);
        var newLayout = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        board.SetManualLayout(newLayout);

        Assert.Equal(newLayout, board.Layout);
    }

    [Fact]
    public void SetManualLayout_WithInvalidLength_ShouldThrowException()
    {
        var board = Board.Create(Guid.NewGuid(), Guid.NewGuid(), 4, FillMode.Manual);
        var invalidLayout = new int[] { 1, 2, 3 };

        Assert.Throws<ArgumentException>(() => board.SetManualLayout(invalidLayout));
    }

    [Fact]
    public void GetPosition_ShouldReturnCorrectRowAndColumn()
    {
        var board = Board.Create(Guid.NewGuid(), Guid.NewGuid(), 5, FillMode.Sequential);

        var (row, col) = board.GetPosition(6);

        Assert.Equal(1, row);
        Assert.Equal(0, col);
    }

    [Fact]
    public void GetPosition_WithInvalidNumber_ShouldThrowException()
    {
        var board = Board.Create(Guid.NewGuid(), Guid.NewGuid(), 5, FillMode.Sequential);

        Assert.Throws<ArgumentException>(() => board.GetPosition(26));
    }
}
