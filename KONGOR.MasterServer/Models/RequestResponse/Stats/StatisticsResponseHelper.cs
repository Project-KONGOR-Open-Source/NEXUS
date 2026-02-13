namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     Aggregate statistics across all game modes, used as secondary data in show_stats responses.
/// </summary>
public class AggregateStatistics
{
    public int TotalGamesPlayed { get; init; }
    public int TotalGamesPlayedIncludingBots { get; init; }
    public int TotalDisconnections { get; init; }
    public int BotGamesWon { get; init; }

    public int PublicGamesPlayed { get; init; }
    public int PublicDisconnections { get; init; }

    public int RankedGamesPlayed { get; init; }
    public int RankedDisconnections { get; init; }

    public int CasualGamesPlayed { get; init; }
    public int CasualDisconnections { get; init; }

    public int MidWarsGamesPlayed { get; init; }
    public int MidWarsDisconnections { get; init; }

    public int RiftWarsGamesPlayed { get; init; }
    public int RiftWarsDisconnections { get; init; }

    public int CampaignGamesPlayed { get; init; }
    public int CampaignDisconnections { get; init; }
    public int CampaignWins { get; init; }
    public int CampaignLosses { get; init; }

    public int CampaignCasualGamesPlayed { get; init; }
    public int CampaignCasualDisconnections { get; init; }
    public int CampaignCasualWins { get; init; }
    public int CampaignCasualLosses { get; init; }

    /// <summary>
    ///     Creates an aggregate statistics object from a dictionary of account statistics.
    /// </summary>
    public static AggregateStatistics FromStatistics(IReadOnlyDictionary<AccountStatisticsType, AccountStatistics> statistics)
    {
        int GetValue(AccountStatisticsType type, Func<AccountStatistics, int> selector)
            => statistics.TryGetValue(type, out AccountStatistics? stat) ? selector(stat) : 0;

        int publicGames = GetValue(AccountStatisticsType.Public, stat => stat.MatchesPlayed);
        int publicDiscos = GetValue(AccountStatisticsType.Public, stat => stat.MatchesDisconnected);

        int rankedGames = GetValue(AccountStatisticsType.Matchmaking, stat => stat.MatchesPlayed);
        int rankedDiscos = GetValue(AccountStatisticsType.Matchmaking, stat => stat.MatchesDisconnected);

        int casualGames = GetValue(AccountStatisticsType.MatchmakingCasual, stat => stat.MatchesPlayed);
        int casualDiscos = GetValue(AccountStatisticsType.MatchmakingCasual, stat => stat.MatchesDisconnected);

        int midWarsGames = GetValue(AccountStatisticsType.MidWars, stat => stat.MatchesPlayed);
        int midWarsDiscos = GetValue(AccountStatisticsType.MidWars, stat => stat.MatchesDisconnected);

        int riftWarsGames = GetValue(AccountStatisticsType.RiftWars, stat => stat.MatchesPlayed);
        int riftWarsDiscos = GetValue(AccountStatisticsType.RiftWars, stat => stat.MatchesDisconnected);

        int coopGamesWon = GetValue(AccountStatisticsType.Cooperative, stat => stat.MatchesWon);

        int campaignGames = GetValue(AccountStatisticsType.Matchmaking, stat => stat.MatchesPlayed);
        int campaignDiscos = GetValue(AccountStatisticsType.Matchmaking, stat => stat.MatchesDisconnected);
        int campaignWins = GetValue(AccountStatisticsType.Matchmaking, stat => stat.MatchesWon);
        int campaignLosses = GetValue(AccountStatisticsType.Matchmaking, stat => stat.MatchesLost);

        int campaignCasualGames = GetValue(AccountStatisticsType.MatchmakingCasual, stat => stat.MatchesPlayed);
        int campaignCasualDiscos = GetValue(AccountStatisticsType.MatchmakingCasual, stat => stat.MatchesDisconnected);
        int campaignCasualWins = GetValue(AccountStatisticsType.MatchmakingCasual, stat => stat.MatchesWon);
        int campaignCasualLosses = GetValue(AccountStatisticsType.MatchmakingCasual, stat => stat.MatchesLost);

        int totalGames = publicGames + rankedGames + casualGames + midWarsGames + riftWarsGames;
        int totalDiscos = publicDiscos + rankedDiscos + casualDiscos + midWarsDiscos + riftWarsDiscos;

        return new AggregateStatistics
        {
            TotalGamesPlayed = totalGames,
            TotalGamesPlayedIncludingBots = totalGames + coopGamesWon,
            TotalDisconnections = totalDiscos,
            BotGamesWon = coopGamesWon,

            PublicGamesPlayed = publicGames,
            PublicDisconnections = publicDiscos,

            RankedGamesPlayed = rankedGames,
            RankedDisconnections = rankedDiscos,

            CasualGamesPlayed = casualGames,
            CasualDisconnections = casualDiscos,

            MidWarsGamesPlayed = midWarsGames,
            MidWarsDisconnections = midWarsDiscos,

            RiftWarsGamesPlayed = riftWarsGames,
            RiftWarsDisconnections = riftWarsDiscos,

            CampaignGamesPlayed = campaignGames,
            CampaignDisconnections = campaignDiscos,
            CampaignWins = campaignWins,
            CampaignLosses = campaignLosses,

            CampaignCasualGamesPlayed = campaignCasualGames,
            CampaignCasualDisconnections = campaignCasualDiscos,
            CampaignCasualWins = campaignCasualWins,
            CampaignCasualLosses = campaignCasualLosses
        };
    }
}

/// <summary>
///     Helper methods shared across statistics response classes.
/// </summary>
public static class StatisticsResponseHelper
{
    /// <summary>
    ///     Gets the custom icon slot ID from the account's selected store items.
    /// </summary>
    public static string GetCustomIconSlotID(Account account)
    {
        string? customIcon = account.SelectedStoreItems.SingleOrDefault(item => item.StartsWith("ai.custom_icon:"));

        if (customIcon is null)
            return "0";

        string slotID = customIcon.Replace("ai.custom_icon:", string.Empty);

        return slotID;
    }

    /// <summary>
    ///     Gets the owned store items data dictionary, excluding mastery boosts and coupons.
    /// </summary>
    public static Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> GetOwnedStoreItemsData(Account account)
    {
        Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> items = account.User.OwnedStoreItems
            .Where(item => item.StartsWith("ma.").Equals(false) && item.StartsWith("cp.").Equals(false))
            .ToDictionary<string, string, OneOf<StoreItemData, StoreItemDiscountCoupon>>(upgrade => upgrade, upgrade => new StoreItemData());

        // TODO: Add Mastery Boosts And Coupons

        return items;
    }

    /// <summary>
    ///     Calculates the average kills/deaths/assists string for a statistics entry.
    /// </summary>
    public static string CalculateKDA(AccountStatistics statistics)
    {
        int totalMatches = statistics.MatchesPlayed;

        if (totalMatches is 0)
            return "0/0/0";

        double averageKills = (double) statistics.HeroKills / totalMatches;
        double averageDeaths = (double) statistics.HeroDeaths / totalMatches;
        double averageAssists = (double) statistics.HeroAssists / totalMatches;

        return $"{averageKills:F1}/{averageDeaths:F1}/{averageAssists:F1}";
    }
}
