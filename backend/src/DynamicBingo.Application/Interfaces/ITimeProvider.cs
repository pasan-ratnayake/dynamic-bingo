namespace DynamicBingo.Application.Interfaces;

public interface ITimeProvider
{
    DateTime UtcNow { get; }
}
