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
        var n = 5;

        var board = Board.Create(gameId, userId, FillMode.Sequential, n);

        Assert.Equal(gameId, board.GameId);
        Assert.Equal(userId, board.UserId);
        Assert.Equal(FillMode.Sequential, board.FillMode);
        
        var layout = board.GetLayout();
        Assert.Equal(n, layout.GetLength(0));
        Assert.Equal(n, layout.GetLength(1));
        
        var number = 1;
        for (int row = 0; row < n; row++)
        {
            for (int col = 0; col < n; col++)
            {
                Assert.Equal(number++, layout[row, col]);
            }
        }
    }

    [Fact]
    public void Create_WithRandomFill_ShouldCreateValidLayout()
    {
        var gameId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var n = 5;

        var board = Board.Create(gameId, userId, FillMode.Random, n);

        var layout = board.GetLayout();
        Assert.Equal(n, layout.GetLength(0));
        Assert.Equal(n, layout.GetLength(1));
        
        var numbers = new HashSet<int>();
        for (int row = 0; row < n; row++)
        {
            for (int col = 0; col < n; col++)
            {
                var number = layout[row, col];
                Assert.True(number >= 1 && number <= n * n);
                Assert.True(numbers.Add(number), $"Duplicate number {number} found");
            }
        }
        Assert.Equal(n * n, numbers.Count);
    }

    [Fact]
    public void Create_WithManualFill_ShouldCreateEmptyLayout()
    {
        var gameId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var n = 4;

        var board = Board.Create(gameId, userId, FillMode.Manual, n);

        var layout = board.GetLayout();
        Assert.Equal(n, layout.GetLength(0));
        Assert.Equal(n, layout.GetLength(1));
        
        for (int row = 0; row < n; row++)
        {
            for (int col = 0; col < n; col++)
            {
                Assert.Equal(0, layout[row, col]);
            }
        }
    }

    [Fact]
    public void SetManualLayout_ShouldUpdateLayout()
    {
        var board = Board.Create(Guid.NewGuid(), Guid.NewGuid(), FillMode.Manual, 4);
        var newLayout = new int[4, 4];
        var number = 1;
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                newLayout[row, col] = number++;
            }
        }

        board.SetManualLayout(newLayout);

        var layout = board.GetLayout();
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                Assert.Equal(newLayout[row, col], layout[row, col]);
            }
        }
    }

    [Fact]
    public void SetManualLayout_WithInvalidDimensions_ShouldThrowException()
    {
        var board = Board.Create(Guid.NewGuid(), Guid.NewGuid(), FillMode.Manual, 4);
        var invalidLayout = new int[3, 4]; // Wrong dimensions

        Assert.Throws<ArgumentException>(() => board.SetManualLayout(invalidLayout));
    }

    [Fact]
    public void GetLayout_ShouldReturnCorrectLayout()
    {
        var board = Board.Create(Guid.NewGuid(), Guid.NewGuid(), FillMode.Sequential, 3);

        var layout = board.GetLayout();

        Assert.Equal(3, layout.GetLength(0));
        Assert.Equal(3, layout.GetLength(1));
        Assert.Equal(1, layout[0, 0]);
        Assert.Equal(5, layout[1, 1]);
        Assert.Equal(9, layout[2, 2]);
    }
}
