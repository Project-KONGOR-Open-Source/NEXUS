using System.Globalization;

using MERRICK.DatabaseContext.Extensions;

namespace KONGOR.MasterServer.Services.Requester;

public static class ClientRequestHelper
{
    public static string? GetCookie(HttpRequest request)
    {
        if (request.Form.ContainsKey("cookie"))
        {
            return request.Form["cookie"];
        }

        if (request.Query.ContainsKey("cookie"))
        {
            return request.Query["cookie"];
        }

        return null;
    }

    public static ShowSimpleStatsResponse CreateShowSimpleStatsResponse(Account account, PlayerStatisticsAggregatedDTO stats)
    {
        // Calculate awards
        // Keys derived from MatchStats.cs (Standard Submission Keys)
        // Enforce specific order for awards: Smackdowns, Annihilations, Assists, Kills
        // This ensures all users see the same awards in the same order, matching the requested design.
        List<KeyValuePair<string, int>> top4 = new List<KeyValuePair<string, int>>
        {
            new("awd_msd", stats.Smackdowns),
            new("awd_mann", stats.Annihilations),
            new("awd_masst", stats.Assists),
            new("awd_mkill", stats.Kills)
        };

        // Populate from account statistics.
        return new ShowSimpleStatsResponse
        {
            NameWithClanTag = account.GetNameWithClanTag(),
            ID = account.ID,
            Level = account.User.TotalLevel,
            LevelExperience = account.User.TotalExperience,
            NumberOfAvatarsOwned = account.User.OwnedStoreItems.Count(i => i.StartsWith("aa.", StringComparison.OrdinalIgnoreCase)),
            NumberOfHeroesOwned = account.User.OwnedStoreItems.Count(i => i.StartsWith("h.", StringComparison.OrdinalIgnoreCase)),
            TotalMatchesPlayed = stats.TotalMatches,
            CurrentSeason = 0,
            SeasonLevel = 0,
            CreepLevel = 0,
            SimpleSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = stats.RankedWins,
                RankedMatchesLost = stats.RankedLosses,
                WinStreak = 0, // Requires match history analysis, leaving 0
                InPlacementPhase = 0,
                LevelsGainedThisSeason = 0
            },
            SimpleCasualSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = stats.CasualWins,
                RankedMatchesLost = stats.CasualLosses,
                WinStreak = 0,
                InPlacementPhase = 0,
                LevelsGainedThisSeason = 0
            },
            MVPAwardsCount = stats.MVP,
            Top4AwardNames = top4.Select(x => x.Key).ToList(),
            Top4AwardCounts = top4.Select(x => x.Value).ToList(),
            CustomIconSlotID = GetCustomIconSlotID(account),
            OwnedStoreItems = account.User.OwnedStoreItems,
            SelectedStoreItems = account.SelectedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(account),
            DiceTokens = "1", // Hardcoded string for parity
            ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public static string SetCustomIconSlotID(Account account)
    {
        return GetCustomIconSlotID(account);
    }

    private static string GetCustomIconSlotID(Account account)
    {
        // The client expects the 1-BASED INDEX of the equipped icon in the selected_upgrades list.
        // If no icon is equipped, it expects "0".
        // Note: For ID, strict typing might require it to be a string or int depending on context,
        // but here it returns string which fits 'CustomIconSlotID'.
        if (account.SelectedStoreItems == null) return "0";

        int index = account.SelectedStoreItems.FindIndex(i => i.StartsWith("ai.", StringComparison.OrdinalIgnoreCase));
        return index >= 0 ? (index + 1).ToString(CultureInfo.InvariantCulture) : "0";
    }

    public static Dictionary<string, OneOf<object, Dictionary<string, object>>> SetOwnedStoreItemsData(Account account)
    {
        Dictionary<string, OneOf<object, Dictionary<string, object>>> data = new();
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long forever = now + 999999999;

        if (account.User.OwnedStoreItems != null)
        {
            foreach (string item in account.User.OwnedStoreItems)
            {
                data[item] = new StoreItemData
                {
                    Data = "",
                    AvailableFrom = now.ToString(),
                    AvailableUntil = forever.ToString(), // Log shows 33325185006
                    Used = 0,
                    Score = "0",
                    ExpirationDate = "0",
                    Permanent = "1"
                };
            }
        }
        return data;
    }
}
