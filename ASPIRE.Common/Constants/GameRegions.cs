namespace ASPIRE.Common.Constants;

/// <summary>
///     The shared directory of game regions, used by matchmaking and by the public server list.
///     Specific regions belong to logical aggregates, and region matching operates at the aggregate level, so that for example a "USE" group matches a "USW" group through their shared "US" aggregate.
/// </summary>
public static class GameRegions
{
    /// <summary>
    ///     The wildcard region.
    ///     As a group preference it accepts every region, and as a server location it makes the server available to every match.
    /// </summary>
    public const string Wildcard = "NEWERTH";

    /// <summary>
    ///     Maps each region the client can send to its logical aggregate.
    ///     Aggregate names map to themselves, so servers may advertise either a specific region or an aggregate.
    /// </summary>
    private static readonly Dictionary<string, string> RegionAggregates = new (StringComparer.OrdinalIgnoreCase)
    {
        ["US"]  = "US",
        ["USE"] = "US",
        ["USW"] = "US",

        ["LAT"] = "LAT",
        ["BR"]  = "LAT",

        ["EU"]  = "EU",
        ["RU"]  = "EU",
        ["TR"]  = "EU",

        ["SEA"] = "SEA",
        ["SG"]  = "SEA",
        ["MY"]  = "SEA",
        ["PH"]  = "SEA",
        ["TH"]  = "SEA",
        ["ID"]  = "SEA",
        ["CN"]  = "SEA",
        ["KR"]  = "SEA",
        ["AU"]  = "SEA"
    };

    /// <summary>
    ///     Gets the logical aggregate for a region.
    ///     An unmapped region aggregates to itself, so an unknown code only ever matches itself.
    /// </summary>
    public static string GetAggregate(string region)
        => RegionAggregates.TryGetValue(region, out string? aggregate) ? aggregate : region;

    /// <summary>
    ///     Normalises a server's advertised location to its aggregate.
    ///     An unknown location normalises to the <see cref="Wildcard"/> region, making the server available to every match.
    /// </summary>
    public static string NormaliseServerLocation(string location)
        => RegionAggregates.TryGetValue(location, out string? aggregate) ? aggregate : Wildcard;
}
