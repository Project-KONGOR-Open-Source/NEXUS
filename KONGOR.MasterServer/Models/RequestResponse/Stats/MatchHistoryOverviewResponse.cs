namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     A single match entry in the match history overview response.
///     Keyed as "m0", "m1", "m2", etc. in the serialised PHP array.
/// </summary>
public class MatchHistoryOverviewEntry
{
    [PHPProperty("match_id")]
    public required string MatchID { get; init; }

    [PHPProperty("wins")]
    public required string Wins { get; init; }

    [PHPProperty("team")]
    public required string Team { get; init; }

    [PHPProperty("herokills")]
    public required string HeroKills { get; init; }

    [PHPProperty("deaths")]
    public required string Deaths { get; init; }

    [PHPProperty("heroassists")]
    public required string HeroAssists { get; init; }

    [PHPProperty("hero_id")]
    public required string HeroID { get; init; }

    [PHPProperty("secs")]
    public required string SecondsPlayed { get; init; }

    [PHPProperty("map")]
    public required string Map { get; init; }

    [PHPProperty("mdt")]
    public required string MatchDatetime { get; init; }

    [PHPProperty("hero_cli_name")]
    public required string HeroClientName { get; init; }
}
