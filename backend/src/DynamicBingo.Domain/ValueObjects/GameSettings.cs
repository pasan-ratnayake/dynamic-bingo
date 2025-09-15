using DynamicBingo.Domain.Enums;

namespace DynamicBingo.Domain.ValueObjects;

public class GameSettings
{
    public FillMode FillMode { get; set; }
    public StarterChoice StarterChoice { get; set; }
}
