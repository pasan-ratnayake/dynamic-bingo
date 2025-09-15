using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;

namespace DynamicBingo.Application.Services;

public class GameEngineService
{
    private readonly IGameRepository _gameRepository;
    private readonly IRealtimeTransport _realtimeTransport;
    private readonly ITimeProvider _timeProvider;

    public GameEngineService(
        IGameRepository gameRepository,
        IRealtimeTransport realtimeTransport,
        ITimeProvider timeProvider)
    {
        _gameRepository = gameRepository;
        _realtimeTransport = realtimeTransport;
        _timeProvider = timeProvider;
    }

    public async Task<bool> MarkNumberAsync(Guid gameId, Guid playerId, int number)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null || game.Status != GameStatus.Active)
            return false;

        var currentTurn = game.Turns.Where(t => !t.IsResolved).OrderBy(t => t.Index).FirstOrDefault();
        if (currentTurn == null || currentTurn.PlayerToMoveId != playerId)
            return false;

        if (currentTurn.IsExpired)
        {
            await HandleExpiredTurnAsync(game, currentTurn);
            return false;
        }

        if (game.Marks.Any(m => m.Number == number))
            return false;

        var turnIndex = currentTurn.Index;
        var mark = Mark.Create(gameId, number, playerId, turnIndex);
        game.Marks.Add(mark);

        currentTurn.Resolve(TurnOutcome.Mark, number);

        var (creatorScore, opponentScore) = await CalculateScoresAsync(game);
        
        var creatorPlayer = game.Players.First(p => p.IsCreator);
        var opponentPlayer = game.Players.First(p => !p.IsCreator);

        var creatorPreviousScore = creatorPlayer.Score;
        var opponentPreviousScore = opponentPlayer.Score;

        creatorPlayer.AddScore(creatorScore - creatorPreviousScore);
        opponentPlayer.AddScore(opponentScore - opponentPreviousScore);

        await _realtimeTransport.SendToGroupAsync($"game-{gameId}", "NumberMarked", new
        {
            ByUserId = playerId,
            Number = number,
            TurnIndex = turnIndex,
            Auto = false
        });

        if (creatorScore != creatorPreviousScore)
        {
            var completedLines = await GetCompletedLinesForMarkAsync(game, number, creatorPlayer.UserId);
            await _realtimeTransport.SendToGroupAsync($"game-{gameId}", "ScoreUpdated", new
            {
                UserId = creatorPlayer.UserId,
                NewScore = creatorScore,
                CompletedLines = completedLines
            });
        }

        if (opponentScore != opponentPreviousScore)
        {
            var completedLines = await GetCompletedLinesForMarkAsync(game, number, opponentPlayer.UserId);
            await _realtimeTransport.SendToGroupAsync($"game-{gameId}", "ScoreUpdated", new
            {
                UserId = opponentPlayer.UserId,
                NewScore = opponentScore,
                CompletedLines = completedLines
            });
        }

        if (creatorScore >= 5 || opponentScore >= 5)
        {
            await EndGameAsync(game, creatorScore, opponentScore);
        }
        else
        {
            await StartNextTurnAsync(game);
        }

        await _gameRepository.UpdateAsync(game);
        return true;
    }

    private async Task HandleExpiredTurnAsync(Game game, Turn expiredTurn)
    {
        var player = game.Players.First(p => p.UserId == expiredTurn.PlayerToMoveId);
        player.IncrementIdleCount();

        if (player.IdleCount == 1)
        {
            await AutoMarkRandomNumberAsync(game, expiredTurn, player.UserId);
        }
        else if (player.IdleCount == 2)
        {
            expiredTurn.Resolve(TurnOutcome.Forfeit);
            await _realtimeTransport.SendToGroupAsync($"game-{game.Id}", "PenaltyApplied", new
            {
                UserId = player.UserId,
                Type = "IdleForfeit",
                Details = "Turn forfeited due to inactivity"
            });
            await StartNextTurnAsync(game);
        }
        else
        {
            expiredTurn.Resolve(TurnOutcome.Forfeit);
            player.Forfeit("Game forfeited due to repeated inactivity");
            game.End(GameEndReason.Forfeit, GetOpponentId(game, player.UserId));
            
            await _realtimeTransport.SendToGroupAsync($"game-{game.Id}", "PenaltyApplied", new
            {
                UserId = player.UserId,
                Type = "GameForfeit",
                Details = "Game forfeited due to repeated inactivity"
            });

            await _realtimeTransport.SendToGroupAsync($"game-{game.Id}", "GameEnded", new
            {
                Result = "Win",
                WinnerId = GetOpponentId(game, player.UserId),
                Reason = "Opponent forfeited due to inactivity"
            });
        }

        await _gameRepository.UpdateAsync(game);
    }

    private async Task AutoMarkRandomNumberAsync(Game game, Turn turn, Guid playerId)
    {
        var markedNumbers = game.Marks.Select(m => m.Number).ToHashSet();
        var availableNumbers = Enumerable.Range(1, game.N * game.N).Where(n => !markedNumbers.Contains(n)).ToList();
        
        if (availableNumbers.Count == 0)
        {
            turn.Resolve(TurnOutcome.Forfeit);
            await StartNextTurnAsync(game);
            return;
        }

        var random = new Random();
        var randomNumber = availableNumbers[random.Next(availableNumbers.Count)];
        
        var mark = Mark.Create(game.Id, randomNumber, playerId, turn.Index);
        game.Marks.Add(mark);
        
        turn.Resolve(TurnOutcome.AutoMark, randomNumber);

        await _realtimeTransport.SendToGroupAsync($"game-{game.Id}", "NumberMarked", new
        {
            ByUserId = playerId,
            Number = randomNumber,
            TurnIndex = turn.Index,
            Auto = true
        });

        await _realtimeTransport.SendToGroupAsync($"game-{game.Id}", "PenaltyApplied", new
        {
            UserId = playerId,
            Type = "IdleAutoMark",
            Details = $"Auto-marked number {randomNumber} due to inactivity"
        });

        var (creatorScore, opponentScore) = await CalculateScoresAsync(game);
        var creatorPlayer = game.Players.First(p => p.IsCreator);
        var opponentPlayer = game.Players.First(p => !p.IsCreator);

        var creatorPreviousScore = creatorPlayer.Score;
        var opponentPreviousScore = opponentPlayer.Score;

        creatorPlayer.AddScore(creatorScore - creatorPreviousScore);
        opponentPlayer.AddScore(opponentScore - opponentPreviousScore);

        if (creatorScore >= 5 || opponentScore >= 5)
        {
            await EndGameAsync(game, creatorScore, opponentScore);
        }
        else
        {
            await StartNextTurnAsync(game);
        }
    }

    private async Task StartNextTurnAsync(Game game)
    {
        var lastTurn = game.Turns.OrderBy(t => t.Index).LastOrDefault();
        var nextPlayerToMoveId = GetOpponentId(game, lastTurn?.PlayerToMoveId ?? game.ResolvedStarterId!.Value);
        
        var nextTurn = Turn.Create(game.Id, (lastTurn?.Index ?? -1) + 1, nextPlayerToMoveId, TimeSpan.FromSeconds(30));
        game.Turns.Add(nextTurn);

        await _realtimeTransport.SendToGroupAsync($"game-{game.Id}", "TurnStarted", new
        {
            PlayerId = nextPlayerToMoveId,
            TurnIndex = nextTurn.Index,
            ExpiresAt = nextTurn.ExpiresAt
        });
    }

    private async Task EndGameAsync(Game game, int creatorScore, int opponentScore)
    {
        if (creatorScore >= 5 && opponentScore >= 5)
        {
            game.End(GameEndReason.Draw);
            await _realtimeTransport.SendToGroupAsync($"game-{game.Id}", "GameEnded", new
            {
                Result = "Draw",
                WinnerId = (Guid?)null,
                Reason = "Both players reached 5 points simultaneously"
            });
        }
        else if (creatorScore >= 5)
        {
            var creatorPlayer = game.Players.First(p => p.IsCreator);
            game.End(GameEndReason.Win, creatorPlayer.UserId);
            await _realtimeTransport.SendToGroupAsync($"game-{game.Id}", "GameEnded", new
            {
                Result = "Win",
                WinnerId = creatorPlayer.UserId,
                Reason = "Reached 5 points"
            });
        }
        else if (opponentScore >= 5)
        {
            var opponentPlayer = game.Players.First(p => !p.IsCreator);
            game.End(GameEndReason.Win, opponentPlayer.UserId);
            await _realtimeTransport.SendToGroupAsync($"game-{game.Id}", "GameEnded", new
            {
                Result = "Win",
                WinnerId = opponentPlayer.UserId,
                Reason = "Reached 5 points"
            });
        }
    }

    private async Task<(int creatorScore, int opponentScore)> CalculateScoresAsync(Game game)
    {
        var creatorPlayer = game.Players.First(p => p.IsCreator);
        var opponentPlayer = game.Players.First(p => !p.IsCreator);

        var creatorBoard = game.Boards.First(b => b.UserId == creatorPlayer.UserId);
        var opponentBoard = game.Boards.First(b => b.UserId == opponentPlayer.UserId);

        var markedNumbers = game.Marks.Select(m => m.Number).ToHashSet();

        var creatorScore = CalculateBoardScore(creatorBoard.GetLayout(), markedNumbers, game.N);
        var opponentScore = CalculateBoardScore(opponentBoard.GetLayout(), markedNumbers, game.N);

        return (creatorScore, opponentScore);
    }

    private int CalculateBoardScore(int[,] layout, HashSet<int> markedNumbers, int n)
    {
        var score = 0;

        for (int row = 0; row < n; row++)
        {
            if (IsLineComplete(layout, markedNumbers, n, row, 0, 0, 1))
                score++;
        }

        for (int col = 0; col < n; col++)
        {
            if (IsLineComplete(layout, markedNumbers, n, 0, col, 1, 0))
                score++;
        }

        if (IsLineComplete(layout, markedNumbers, n, 0, 0, 1, 1))
            score++;

        if (IsLineComplete(layout, markedNumbers, n, 0, n - 1, 1, -1))
            score++;

        return score;
    }

    private bool IsLineComplete(int[,] layout, HashSet<int> markedNumbers, int n, int startRow, int startCol, int rowDelta, int colDelta)
    {
        for (int i = 0; i < n; i++)
        {
            var row = startRow + i * rowDelta;
            var col = startCol + i * colDelta;
            var number = layout[row, col];
            
            if (!markedNumbers.Contains(number))
                return false;
        }
        return true;
    }

    private async Task<List<int>> GetCompletedLinesForMarkAsync(Game game, int markedNumber, Guid userId)
    {
        var board = game.Boards.First(b => b.UserId == userId);
        var layout = board.GetLayout();
        var markedNumbers = game.Marks.Select(m => m.Number).ToHashSet();
        
        var completedLines = new List<int>();
        var n = game.N;

        for (int row = 0; row < n; row++)
        {
            for (int col = 0; col < n; col++)
            {
                if (layout[row, col] == markedNumber)
                {
                    if (IsLineComplete(layout, markedNumbers, n, row, 0, 0, 1))
                    {
                        for (int c = 0; c < n; c++)
                            completedLines.Add(layout[row, c]);
                    }

                    if (IsLineComplete(layout, markedNumbers, n, 0, col, 1, 0))
                    {
                        for (int r = 0; r < n; r++)
                            completedLines.Add(layout[r, col]);
                    }

                    if (row == col && IsLineComplete(layout, markedNumbers, n, 0, 0, 1, 1))
                    {
                        for (int i = 0; i < n; i++)
                            completedLines.Add(layout[i, i]);
                    }

                    if (row + col == n - 1 && IsLineComplete(layout, markedNumbers, n, 0, n - 1, 1, -1))
                    {
                        for (int i = 0; i < n; i++)
                            completedLines.Add(layout[i, n - 1 - i]);
                    }
                    break;
                }
            }
        }

        return completedLines.Distinct().ToList();
    }

    private Guid GetOpponentId(Game game, Guid playerId)
    {
        return playerId == game.CreatorId ? game.OpponentId!.Value : game.CreatorId;
    }
}
