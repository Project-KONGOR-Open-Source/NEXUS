namespace MERRICK.DatabaseContext.Entities.Statistics;

public class AbilityEvent
{
    public required string HeroName { get; set; }

    public required string AbilityName { get; set; }

    public required int GameTimeSeconds { get; set; }

    public required byte SlotIndex { get; set; }
}