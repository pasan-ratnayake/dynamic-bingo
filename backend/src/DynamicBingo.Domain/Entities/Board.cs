using DynamicBingo.Domain.Enums;

namespace DynamicBingo.Domain.Entities;

public class Board
{
    public Guid Id { get; private set; }
    public Guid GameId { get; private set; }
    public Guid UserId { get; private set; }
    public string LayoutJson { get; private set; }
    public FillMode FillMode { get; private set; }

    public Game Game { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private Board() { }

    public static Board Create(Guid gameId, Guid userId, FillMode fillMode, int n)
    {
        var layout = fillMode switch
        {
            FillMode.Sequential => GenerateSequentialLayout(n),
            FillMode.Random => GenerateRandomLayout(n),
            FillMode.Manual => GenerateEmptyLayout(n),
            _ => throw new ArgumentException("Invalid fill mode")
        };

        return new Board
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            UserId = userId,
            LayoutJson = System.Text.Json.JsonSerializer.Serialize(layout),
            FillMode = fillMode
        };
    }

    public void SetManualLayout(int[,] layout)
    {
        if (FillMode != FillMode.Manual)
            throw new InvalidOperationException("Can only set manual layout for manual fill mode");

        var n = (int)Math.Sqrt(layout.Length);
        if (layout.GetLength(0) != n || layout.GetLength(1) != n)
            throw new ArgumentException("Layout must be square");

        ValidateLayout(layout, n);
        LayoutJson = System.Text.Json.JsonSerializer.Serialize(layout);
    }

    public int[,] GetLayout()
    {
        return System.Text.Json.JsonSerializer.Deserialize<int[,]>(LayoutJson)!;
    }

    private static int[,] GenerateSequentialLayout(int n)
    {
        var layout = new int[n, n];
        var number = 1;

        for (int row = 0; row < n; row++)
        {
            for (int col = 0; col < n; col++)
            {
                layout[row, col] = number++;
            }
        }

        return layout;
    }

    private static int[,] GenerateRandomLayout(int n)
    {
        var numbers = Enumerable.Range(1, n * n).ToList();
        var random = new Random();
        var layout = new int[n, n];

        for (int row = 0; row < n; row++)
        {
            for (int col = 0; col < n; col++)
            {
                var index = random.Next(numbers.Count);
                layout[row, col] = numbers[index];
                numbers.RemoveAt(index);
            }
        }

        return layout;
    }

    private static int[,] GenerateEmptyLayout(int n)
    {
        return new int[n, n];
    }

    private static void ValidateLayout(int[,] layout, int n)
    {
        var expectedNumbers = Enumerable.Range(1, n * n).ToHashSet();
        var actualNumbers = new HashSet<int>();

        for (int row = 0; row < n; row++)
        {
            for (int col = 0; col < n; col++)
            {
                var number = layout[row, col];
                if (number < 1 || number > n * n)
                    throw new ArgumentException($"Invalid number {number} in layout");

                if (!actualNumbers.Add(number))
                    throw new ArgumentException($"Duplicate number {number} in layout");
            }
        }

        if (!expectedNumbers.SetEquals(actualNumbers))
            throw new ArgumentException("Layout must contain all numbers from 1 to N²");
    }
}
