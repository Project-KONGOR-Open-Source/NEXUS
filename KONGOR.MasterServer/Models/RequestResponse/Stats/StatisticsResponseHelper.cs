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
    public int PublicSecondsPlayed { get; init; }

    public int RankedGamesPlayed { get; init; }
    public int RankedDisconnections { get; init; }
    public int RankedSecondsPlayed { get; init; }

    public int CasualGamesPlayed { get; init; }
    public int CasualDisconnections { get; init; }
    public int CasualSecondsPlayed { get; init; }

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
        int publicSeconds = GetValue(AccountStatisticsType.Public, stat => stat.HeroStatistics.AggregateTotals().SecondsPlayed);

        int rankedGames = GetValue(AccountStatisticsType.Matchmaking, stat => stat.MatchesPlayed);
        int rankedDiscos = GetValue(AccountStatisticsType.Matchmaking, stat => stat.MatchesDisconnected);
        int rankedSeconds = GetValue(AccountStatisticsType.Matchmaking, stat => stat.HeroStatistics.AggregateTotals().SecondsPlayed);

        int casualGames = GetValue(AccountStatisticsType.MatchmakingCasual, stat => stat.MatchesPlayed);
        int casualDiscos = GetValue(AccountStatisticsType.MatchmakingCasual, stat => stat.MatchesDisconnected);
        int casualSeconds = GetValue(AccountStatisticsType.MatchmakingCasual, stat => stat.HeroStatistics.AggregateTotals().SecondsPlayed);

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
            PublicSecondsPlayed = publicSeconds,

            RankedGamesPlayed = rankedGames,
            RankedDisconnections = rankedDiscos,
            RankedSecondsPlayed = rankedSeconds,

            CasualGamesPlayed = casualGames,
            CasualDisconnections = casualDiscos,
            CasualSecondsPlayed = casualSeconds,

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
    ///     Gets the owned store items data dictionary.
    ///     Mastery boost consumables are excluded, as their counts are surfaced via the match mastery response instead.
    ///     Owned mastery coupons are surfaced as discount coupon data so the client can offer their discount in the store.
    /// </summary>
    public static Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> GetOwnedStoreItemsData(Account account)
    {
        Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> items = [];

        foreach (string ownedItem in account.User.OwnedStoreItems)
        {
            if (ownedItem.StartsWith("ma.", StringComparison.Ordinal))
                continue;

            if (ownedItem.StartsWith("cp.", StringComparison.Ordinal))
            {
                StoreItemDiscountCoupon? coupon = MasteryCouponHelper.BuildDiscountCoupon(ownedItem);

                if (coupon is not null)
                    items[ownedItem] = coupon;

                continue;
            }

            items[ownedItem] = new StoreItemData();
        }

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

    /// <summary>
    ///     Resolves the up-to-five most-played heroes for a statistics entry, ordered by the number of matches played with each hero descending.
    ///     Each entry pairs the hero's icon texture name with the percentage of the account's matches played with that hero and the full hero identifier.
    /// </summary>
    public static IReadOnlyList<FavouriteHero> GetFavouriteHeroes(AccountStatistics statistics)
    {
        int matchesPlayed = statistics.MatchesPlayed;

        return statistics.HeroStatistics.Heroes
            .OrderByDescending(hero => hero.GamesPlayed)
            .Take(5)
            .Select(hero => new FavouriteHero
            (
                ResolveHeroTextureName(hero.HeroIdentifier),
                matchesPlayed is 0 ? 0.0 : Math.Round((double) hero.GamesPlayed / matchesPlayed * 100.0, 2),
                hero.HeroIdentifier
            ))
            .ToList();
    }

    /// <summary>
    ///     Resolves the icon texture name (for example "pyromancer") for a hero identifier (for example "Hero_Pyromancer") by removing the "Hero_" prefix and lower-casing the remainder.
    /// </summary>
    private static string ResolveHeroTextureName(string heroIdentifier)
        => heroIdentifier.StartsWith("Hero_", StringComparison.Ordinal)
            ? heroIdentifier["Hero_".Length..].ToLowerInvariant()
            : heroIdentifier.ToLowerInvariant();

    /// <summary>
    ///     Calculates a per-match average of a total quantity, rounded to two decimal places.
    ///     Returns 0 when no matches have been played.
    /// </summary>
    public static double CalculatePerMatchAverage(int total, int matchesPlayed)
        => matchesPlayed is 0 ? 0.0 : Math.Round((double) total / matchesPlayed, 2);

    /// <summary>
    ///     Calculates a per-minute average of a total quantity over a duration given in seconds, rounded to two decimal places.
    ///     Returns 0 when no time has elapsed.
    /// </summary>
    public static double CalculatePerMinuteAverage(int total, int seconds)
        => seconds is 0 ? 0.0 : Math.Round(total / (seconds / 60.0), 2);
}

/// <summary>
///     A single favourite-hero entry in a show_stats response.
/// </summary>
/// <param name="TextureName">
///     The hero's icon texture name (the hero identifier with its "Hero_" prefix removed and lower-cased).
/// </param>
/// <param name="PlayRatePercentage">
///     The percentage of the account's matches that have been played with this hero.
/// </param>
/// <param name="Identifier">
///     The full hero identifier (for example "Hero_Pyromancer").
/// </param>
public record FavouriteHero(string TextureName, double PlayRatePercentage, string Identifier);
