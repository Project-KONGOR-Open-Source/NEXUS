namespace ASPIRE.Common.Configuration.Plinko;

/// <summary>
///     Configuration for the Plinko mini-game.
///     Defines the costs to play a drop, the per-tier probabilities, the visual bucket order sent to the client, and the ticket rewards paid out when a chest tier is exhausted or a ticket tier is rolled.
/// </summary>
public class PlinkoConfiguration
{
    /// <summary>
    ///     The gold coin cost of a single drop when the player pays with gold.
    /// </summary>
    public required int GoldCost { get; init; }

    /// <summary>
    ///     The Plinko ticket cost of a single drop when the player pays with tickets.
    /// </summary>
    public required int TicketCost { get; init; }

    /// <summary>
    ///     The visual order in which tiers appear in the six Plinko buckets on the client UI (left to right).
    ///     Each value is a tier identifier in the range 1 to 6.
    /// </summary>
    public required int[] TierBucketOrder { get; init; }

    /// <summary>
    ///     A comma-separated list of six Unix timestamps that the client uses to invalidate its cached tier layout.
    ///     The values are historical captures and are not required to be accurate timestamps.
    /// </summary>
    public required string LastUpdateTimes { get; init; }

    /// <summary>
    ///     The per-tier drop probabilities, expressed as integer percentages that must sum to 100.
    /// </summary>
    public required PlinkoDropProbabilities DropProbabilities { get; init; }

    /// <summary>
    ///     The ticket rewards paid out when a chest tier (1 to 4) is rolled but the player already owns every product in it.
    /// </summary>
    public required PlinkoExhaustionTicketRewards ExhaustionTicketRewards { get; init; }

    /// <summary>
    ///     The ticket rewards paid out when a ticket tier (5 or 6) is rolled.
    /// </summary>
    public required PlinkoConsolationTickets ConsolationTickets { get; init; }

    /// <summary>
    ///     Validates that the configured drop probabilities sum to exactly 100 percent.
    ///     Throws <see cref="InvalidOperationException"/> if they do not, so that misconfiguration is caught at startup.
    /// </summary>
    public void Validate()
    {
        int sum = DropProbabilities.Tier1
            + DropProbabilities.Tier2
            + DropProbabilities.Tier3
            + DropProbabilities.Tier4
            + DropProbabilities.Tier5
            + DropProbabilities.Tier6;

        if (sum.Equals(100).Equals(false))
            throw new InvalidOperationException($@"Plinko Drop Probabilities Must Sum To 100, But Sum To ""{sum}""");
    }

    /// <summary>
    ///     Returns the exhaustion ticket reward for the given chest tier (1 to 4).
    /// </summary>
    public int GetExhaustionTicketReward(int tierID) => tierID switch
    {
        1 => ExhaustionTicketRewards.Tier1,
        2 => ExhaustionTicketRewards.Tier2,
        3 => ExhaustionTicketRewards.Tier3,
        4 => ExhaustionTicketRewards.Tier4,
        _ => throw new ArgumentOutOfRangeException(nameof(tierID), tierID, @"Exhaustion Rewards Are Only Defined For Chest Tiers 1 To 4")
    };

    /// <summary>
    ///     Returns the consolation ticket reward for the given ticket tier (5 or 6).
    /// </summary>
    public int GetConsolationTicketReward(int tierID) => tierID switch
    {
        5 => ConsolationTickets.Tier5,
        6 => ConsolationTickets.Tier6,
        _ => throw new ArgumentOutOfRangeException(nameof(tierID), tierID, @"Consolation Rewards Are Only Defined For Ticket Tiers 5 And 6")
    };
}

/// <summary>
///     The per-tier drop probabilities in integer percent, all of which must sum to 100.
/// </summary>
public class PlinkoDropProbabilities
{
    public required int Tier1 { get; init; }
    public required int Tier2 { get; init; }
    public required int Tier3 { get; init; }
    public required int Tier4 { get; init; }
    public required int Tier5 { get; init; }
    public required int Tier6 { get; init; }
}

/// <summary>
///     The ticket counts paid out when a chest tier is exhausted for the requesting player.
/// </summary>
public class PlinkoExhaustionTicketRewards
{
    public required int Tier1 { get; init; }
    public required int Tier2 { get; init; }
    public required int Tier3 { get; init; }
    public required int Tier4 { get; init; }
}

/// <summary>
///     The ticket counts paid out when a ticket tier is rolled.
/// </summary>
public class PlinkoConsolationTickets
{
    public required int Tier5 { get; init; }
    public required int Tier6 { get; init; }
}
