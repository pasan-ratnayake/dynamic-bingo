using DynamicBingo.Application.Interfaces;

namespace DynamicBingo.Infrastructure.Services;

public class TimeProvider : ITimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
