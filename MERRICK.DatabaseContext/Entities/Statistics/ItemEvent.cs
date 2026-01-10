namespace MERRICK.DatabaseContext.Entities.Statistics;

public class ItemEvent
{
    public required string ItemName { get; set; }

    public required int GameTimeSeconds { get; set; }

    public required byte EventType { get; set; }
}