namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class FragEvent
{
    [FromForm(Name = "killer_id")] public required int SourceID { get; set; }

    [FromForm(Name = "target_id")] public required int TargetID { get; set; }

    [FromForm(Name = "secs")] public required int GameTimeSeconds { get; set; }

    [FromForm(Name = "assisters")] public required List<int>? SupporterIDs { get; set; }
}
