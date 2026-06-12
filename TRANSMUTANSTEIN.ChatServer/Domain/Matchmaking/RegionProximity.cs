namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

/// <summary>
///     Approximates the proximity between region aggregates (see <see cref="GameRegions"/>) using representative UTC offsets, with distances measured circularly around the globe.
///     Used to prefer the closest available aggregate when no match server is idle in any of a match's requested regions.
/// </summary>
internal static class RegionProximity
{
    private const double HoursAroundTheGlobe = 24.0;

    /// <summary>
    ///     The distance assigned when no offset comparison is possible (an unmapped match region against a different aggregate), which equals the maximum circular distance, so such servers are only selected as a last resort.
    /// </summary>
    public const double UnknownRegionDistance = HoursAroundTheGlobe / 2.0;

    /// <summary>
    ///     Representative UTC offsets per region aggregate.
    /// </summary>
    private static readonly Dictionary<string, double> AggregateUTCOffsets = new (StringComparer.OrdinalIgnoreCase)
    {
        ["US"]  = -6.5,
        ["LAT"] = -4.5,
        ["EU"]  =  1.5,
        ["SEA"] =  8.5
    };

    /// <summary>
    ///     Gets the circular distance between a server's location and the closest of the given regions, comparing at the aggregate level.
    ///     Returns zero when the regions include the <see cref="GameRegions.Wildcard"/>, when the server's location normalises to the wildcard (which includes unknown locations), or when the server's aggregate is one of the regions' aggregates.
    /// </summary>
    public static double GetDistance(string[] regions, string serverLocation)
    {
        string serverAggregate = GameRegions.NormaliseServerLocation(serverLocation);

        // A Server In The Wildcard Region (Including One With An Unknown Location) Is Available To Every Match
        if (serverAggregate.Equals(GameRegions.Wildcard, StringComparison.OrdinalIgnoreCase))
            return 0.0;

        if (regions.Contains(GameRegions.Wildcard, StringComparer.OrdinalIgnoreCase))
            return 0.0;

        double serverOffset = AggregateUTCOffsets[serverAggregate];

        double closestDistance = UnknownRegionDistance;

        foreach (string region in regions)
        {
            string regionAggregate = GameRegions.GetAggregate(region);

            if (regionAggregate.Equals(serverAggregate, StringComparison.OrdinalIgnoreCase))
                return 0.0;

            if (AggregateUTCOffsets.TryGetValue(regionAggregate, out double regionOffset) is false)
                continue;

            double offsetDifference = Math.Abs(regionOffset - serverOffset);
            double circularDistance = Math.Min(offsetDifference, HoursAroundTheGlobe - offsetDifference);

            closestDistance = Math.Min(closestDistance, circularDistance);
        }

        return closestDistance;
    }
}
