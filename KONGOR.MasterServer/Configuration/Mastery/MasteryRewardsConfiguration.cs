namespace KONGOR.MasterServer.Configuration.Mastery;

public class MasteryRewardsConfiguration
{
    public required List<MasteryReward> MasteryRewards { get; set; }
}

public class MasteryReward
{
    public required int RequiredLevel { get; set; }

    public required int ProductIdentifier { get; set; }

    public required string ProductName { get; set; }

    public required string ProductCode { get; set; }

    public required string ProductLocalResource { get; set; }

    public required int ProductQuantity { get; set; }

    public required int GoldCoins { get; set; }

    public required int SilverCoins { get; set; }

    public required int PlinkoTickets { get; set; }
}