namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     The match history overview response, containing a variable number of match entries alongside fixed metadata.
/// </summary>
/// <remarks>
///     <para>
///         The game client expects this response as a flat PHP associative array with dynamic string keys ("m0", "m1", ...), a fixed string key ("vested_threshold"), and a fixed integer key (0), all at the same level with different value types.
///     </para>
///     <para>
///         PhpSerializerNET's class serialisation path counts properties at reflection time and writes the array header (a:N:{...}) before any filters run.
///         This means a class-based model always produces a compile-time-fixed entry count, which does not work when the number of "m{N}" entries is only known at runtime.
///         The library's <see cref="IDictionary"/> serialisation path counts actual entries, so this response model builds an <see cref="OrderedDictionary"/> internally and delegates to <see cref="PhpSerialization.Serialize"/> via <see cref="Serialise"/>.
///     </para>
/// </remarks>
public sealed class MatchHistoryOverviewResponse
{
    /// <summary>
    ///     The match entries, serialised as "m0", "m1", "m2", etc. in the PHP array.
    /// </summary>
    public required List<MatchHistoryOverviewEntry> Entries { get; init; }

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    /// </summary>
    public int VestedThreshold => 5;

    /// <summary>
    ///     Unknown.
    ///     <br/>
    ///     Seems to be set to <see langword="true"/> on a successful response, or to <see langword="false"/> if an error occurs.
    /// </summary>
    public bool Zero => true;

    /// <summary>
    ///     Serialises this response as a flat PHP associative array with "m{N}" keys for each entry, a "vested_threshold" string key, and a 0 integer key.
    /// </summary>
    public string Serialise()
    {
        OrderedDictionary result = new ();

        for (int index = 0; index < Entries.Count; index++)
            result.Add($"m{index}", Entries[index]);

        result.Add("vested_threshold", VestedThreshold);
        result.Add(0, Zero);

        return PhpSerialization.Serialize(result);
    }
}

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
