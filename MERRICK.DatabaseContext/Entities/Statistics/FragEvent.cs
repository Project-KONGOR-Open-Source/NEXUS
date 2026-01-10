namespace MERRICK.DatabaseContext.Entities.Statistics;

public class FragEvent
{
    public required int SourceID { get; set; }

    public required int TargetID { get; set; }

    public required int GameTimeSeconds { get; set; }

    public required List<int>? SupporterIDs { get; set; }
}