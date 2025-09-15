using Xunit;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;

namespace DynamicBingo.Domain.Tests.Entities;

public class GameTests
{
    [Fact]
    public void Create_ShouldCreateGameWithCorrectProperties()
    {
        var creatorId = Guid.NewGuid();
        var word = "BINGO";
        var fillMode = FillMode.Random;
        var starterChoice = StarterChoice.Creator;

        var game = Game.Create(word, creatorId, fillMode, starterChoice);

        Assert.Equal(word.ToUpper(), game.Word);
        Assert.Equal(creatorId, game.CreatorId);
        Assert.Equal(starterChoice, game.StarterChoice);
        Assert.Equal(GameStatus.Pending, game.Status);
        Assert.Null(game.OpponentId);
        Assert.False(game.IsFinished);
        Assert.Equal(word.Length, game.N);
    }

    [Fact]
    public void AddOpponent_ShouldSetOpponentId()
    {
        var game = Game.Create("BINGO", Guid.NewGuid(), FillMode.Random, StarterChoice.Creator);
        var opponentId = Guid.NewGuid();

        game.AddOpponent(opponentId);

        Assert.Equal(opponentId, game.OpponentId);
    }

    [Fact]
    public void Start_ShouldChangeStatusToActive()
    {
        var game = Game.Create("BINGO", Guid.NewGuid(), FillMode.Random, StarterChoice.Creator);
        game.AddOpponent(Guid.NewGuid());

        game.Start();

        Assert.Equal(GameStatus.Active, game.Status);
        Assert.NotNull(game.StartedAt);
    }

    [Fact]
    public void End_ShouldChangeStatusToFinished()
    {
        var game = Game.Create("BINGO", Guid.NewGuid(), FillMode.Random, StarterChoice.Creator);
        game.AddOpponent(Guid.NewGuid());
        game.Start();

        game.End(GameEndReason.Win);

        Assert.Equal(GameStatus.Finished, game.Status);
        Assert.NotNull(game.FinishedAt);
        Assert.True(game.IsFinished);
    }

    [Theory]
    [InlineData("TEST", 4)]
    [InlineData("BINGO", 5)]
    [InlineData("DYNAMIC", 7)]
    [InlineData("BINGOMAX", 8)]
    public void N_ShouldEqualWordLength(string word, int expectedSize)
    {
        var game = Game.Create(word, Guid.NewGuid(), FillMode.Random, StarterChoice.Creator);

        Assert.Equal(expectedSize, game.N);
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("TOOLONGWORD")]
    public void Create_WithInvalidWordLength_ShouldThrowException(string word)
    {
        Assert.Throws<ArgumentException>(() => 
            Game.Create(word, Guid.NewGuid(), FillMode.Random, StarterChoice.Creator));
    }
}
