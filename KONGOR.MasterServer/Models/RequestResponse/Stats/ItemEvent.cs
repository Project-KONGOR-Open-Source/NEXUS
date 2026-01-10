namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class ItemEvent
{
    [FromForm(Name = "account_id")] public required int AccountID { get; set; }

    [FromForm(Name = "cli_name")] public required string ItemName { get; set; }

    [FromForm(Name = "secs")] public required int GameTimeSeconds { get; set; }

    [FromForm(Name = "action")] public required byte EventType { get; set; }
}