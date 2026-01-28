namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class AbilityEvent
{
    [FromForm(Name = "hero_cli_name")] public required string HeroName { get; set; }

    [FromForm(Name = "ability_cli_name")] public required string AbilityName { get; set; }

    [FromForm(Name = "secs")] public required int GameTimeSeconds { get; set; }

    [FromForm(Name = "slot")] public required byte SlotIndex { get; set; }
}
