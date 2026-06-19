namespace KONGOR.MasterServer.Configuration.Mastery;

public class MasteryRewardsConfiguration
{
    public required List<MasteryReward> MasteryRewards { get; set; }
}

/// <summary>
///     Represents a mastery tier reward.
/// </summary>
public partial class MasteryReward
{
    public required int RequiredLevel { get; set; }

    public required int ProductIdentifier { get; set; }

    public required int ProductQuantity { get; set; }

    public required int GoldCoins { get; set; }

    public required int SilverCoins { get; set; }

    public required int PlinkoTickets { get; set; }
}

/// <summary>
///     These properties are <see langword="null"/> for reward tiers that only grant currency (Gold Coins, Silver Coins, or Plinko Tickets) rather than a product.
/// </summary>
public partial class MasteryReward
{
    public required string? ProductName { get; set; }

    public required string? ProductCode { get; set; }

    public required string? ProductLocalResource { get; set; }
}
