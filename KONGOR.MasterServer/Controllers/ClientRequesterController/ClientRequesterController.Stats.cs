namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> GetSimpleStats()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        ShowSimpleStatsResponse response = new()
        {
            NameWithClanTag = account.NameWithClanTag,
            ID = account.ID.ToString(),
            Level = account.User.TotalLevel,
            LevelExperience = account.User.TotalExperience,
            NumberOfAvatarsOwned = account.User.OwnedStoreItems.Count(item => item.StartsWith("aa.")),
            TotalMatchesPlayed = await MerrickContext.PlayerStatistics.CountAsync(stats => stats.AccountID == account.ID), // TODO: Implement Matches Played
            CurrentSeason = 12, // TODO: Set Season
            SimpleSeasonStats = new SimpleSeasonStats() // TODO: Implement Stats
            {
                RankedMatchesWon = 1001 /* ranked */ + 1001 /* ranked casual */,
                RankedMatchesLost = 1002 /* ranked */ + 1002 /* ranked casual */,
                WinStreak = Math.Max(1003 /* ranked */, 1003 /* ranked casual */),
                InPlacementPhase = 0, // TODO: Implement Placement Matches
                LevelsGainedThisSeason = account.User.TotalLevel
            },
            SimpleCasualSeasonStats = new SimpleSeasonStats() // TODO: Implement Stats
            {
                RankedMatchesWon = 1001 /* ranked */ + 1001 /* ranked casual */,
                RankedMatchesLost = 1002 /* ranked */ + 1002 /* ranked casual */,
                WinStreak = Math.Max(1003 /* ranked */, 1003 /* ranked casual */),
                InPlacementPhase = 0, // TODO: Implement Placement Matches
                LevelsGainedThisSeason = account.User.TotalLevel
            },
            MVPAwardsCount = 1004,
            Top4AwardNames = ["awd_masst", "awd_mhdd", "awd_mbdmg", "awd_lgks"], // TODO: Implement Awards
            Top4AwardCounts = [1005, 1006, 1007, 1008], // TODO: Implement Awards
            CustomIconSlotID = SetCustomIconSlotID(account),
            OwnedStoreItems = account.User.OwnedStoreItems,
            SelectedStoreItems = account.SelectedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(account)
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleMatchStats()
    {
        string? cookie = Request.Form["cookie"];
        string? matchID = Request.Form["match_id"];

        Logger.LogInformation("Received Match Stats Request: MatchID={MatchID}, Cookie={Cookie}", matchID, cookie);

        if (cookie is null)

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        if (matchID is null)
        {
            Logger.LogError("Match Stats Request Failed: Missing Match ID");
            return BadRequest(@"Missing Value For Form Parameter ""match_id""");
        }

        MatchStatistics? matchStatistics = await MerrickContext.MatchStatistics.SingleOrDefaultAsync(matchStatistics => matchStatistics.MatchID == int.Parse(matchID));

        if (matchStatistics is null)
        {
            Logger.LogError("Match Stats Request Failed: Match Statistics Not Found For ID {MatchID}", matchID);
            return new NotFoundObjectResult("Match Stats Not Found");
        }

        List<PlayerStatistics> playerStatistics = await MerrickContext.PlayerStatistics.Where(playerStatistics => playerStatistics.MatchID == matchStatistics.MatchID).ToListAsync();

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
        {
            Logger.LogError("Match Stats Request Failed: Session Not Found For Cookie {Cookie}", cookie);
            return new NotFoundObjectResult("Session Not Found");
        }

        Account? account = await MerrickContext.Accounts.SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
        {
            Logger.LogError("Match Stats Request Failed: Account Not Found For Name {AccountName}", accountName);
            return new NotFoundObjectResult("Account Not Found");
        }

        PlayerStatistics? requesterStats = playerStatistics.SingleOrDefault(stats => stats.AccountID == account.ID);

        MatchStatsResponse response = new()
        {
            GoldCoins = account.User.GoldCoins.ToString(),
            SilverCoins = account.User.SilverCoins,
            MatchSummary = [new MatchSummary(matchStatistics, playerStatistics, new MatchStartData
            {
                MatchName = matchStatistics.HostAccountName + "'s Game",
                ServerID = matchStatistics.ServerID,
                HostAccountName = matchStatistics.HostAccountName,
                Map = matchStatistics.Map,
                Version = matchStatistics.Version,
                IsCasual = false, // Defaulting to false as we don't have this in MatchStatistics explicitly yet
                MatchType = 0, // Default
                MatchMode = matchStatistics.GameMode,
                Options = MatchOptions.None // We don't have options persisted in MatchStatistics yet
            })],
            MatchPlayerStatistics = playerStatistics.ToDictionary(stats => stats.AccountID, stats => new MatchPlayerStatistics
            {
                MatchID = stats.MatchID,
                AccountID = stats.AccountID,
                AccountName = stats.AccountName,
                ClanID = stats.ClanID?.ToString() ?? "0",
                HeroID = stats.HeroProductID?.ToString() ?? "0",
                Position = stats.LobbyPosition.ToString(),
                Team = stats.Team.ToString(),
                Level = stats.HeroLevel.ToString(),
                Wins = stats.Win.ToString(),
                Losses = stats.Loss.ToString(),
                Concedes = stats.Conceded.ToString(),
                ConcedeVotes = stats.ConcedeVotes.ToString(),
                Buybacks = stats.Buybacks.ToString(),
                Disconnections = stats.Disconnected.ToString(),
                Kicked = stats.Kicked.ToString(),
                PublicSkill = "1500", // TODO: Store snapshot of PSR
                PublicCount = stats.PublicMatch.ToString(),
                AMMSoloRating = "1500", // TODO: Store snapshot of MMR
                AMMSoloCount = stats.RankedMatch.ToString(),
                AMMTeamRating = "1500", // TODO: Store snapshot of Team MMR
                AMMTeamCount = "0",
                AverageScore = stats.Score.ToString(),
                HeroKills = stats.HeroKills.ToString(),
                HeroDamage = stats.HeroDamage.ToString(),
                HeroExperience = stats.HeroExperience.ToString(),
                HeroKillsGold = stats.GoldFromHeroKills.ToString(),
                HeroAssists = stats.HeroAssists.ToString(),
                Deaths = stats.HeroDeaths.ToString(),
                GoldLostToDeath = stats.GoldLostToDeath.ToString(),
                SecondsDead = stats.SecondsDead.ToString(),
                TeamCreepKills = stats.TeamCreepKills.ToString(),
                TeamCreepDamage = stats.TeamCreepDamage.ToString(),
                TeamCreepExperience = stats.TeamCreepExperience.ToString(),
                TeamCreepGold = stats.TeamCreepGold.ToString(),
                NeutralCreepKills = stats.NeutralCreepKills.ToString(),
                NeutralCreepDamage = stats.NeutralCreepDamage.ToString(),
                NeutralCreepExperience = stats.NeutralCreepExperience.ToString(),
                NeutralCreepGold = stats.NeutralCreepGold.ToString(),
                BuildingDamage = stats.BuildingDamage.ToString(),
                BuildingExperience = stats.ExperienceFromBuildings.ToString(),
                BuildingsRazed = stats.BuildingsRazed.ToString(),
                BuildingGold = stats.GoldFromBuildings.ToString(),
                Denies = stats.Denies.ToString(),
                ExperienceDenied = stats.ExperienceDenied.ToString(),
                Gold = stats.Gold.ToString(),
                GoldSpent = stats.GoldSpent.ToString(),
                Experience = stats.Experience.ToString(),
                Actions = stats.Actions.ToString(),
                Seconds = stats.SecondsPlayed.ToString(),
                Consumables = stats.ConsumablesPurchased.ToString(),
                Wards = stats.WardsPlaced.ToString(),
                TimeEarningExperience = stats.TimeEarningExperience.ToString(),
                FirstBlood = stats.FirstBlood.ToString(),
                DoubleKill = stats.DoubleKill.ToString(),
                TripleKill = stats.TripleKill.ToString(),
                QuadKill = stats.QuadKill.ToString(),
                Annihilation = stats.Annihilation.ToString(),
                KillStreak3 = stats.KillStreak03.ToString(),
                KillStreak4 = stats.KillStreak04.ToString(),
                KillStreak5 = stats.KillStreak05.ToString(),
                KillStreak6 = stats.KillStreak06.ToString(),
                KillStreak7 = stats.KillStreak07.ToString(),
                KillStreak8 = stats.KillStreak08.ToString(),
                KillStreak9 = stats.KillStreak09.ToString(),
                KillStreak10 = stats.KillStreak10.ToString(),
                KillStreak15 = stats.KillStreak15.ToString(),
                Smackdown = stats.Smackdown.ToString(),
                Humiliation = stats.Humiliation.ToString(),
                Nemesis = stats.Nemesis.ToString(),
                Retribution = stats.Retribution.ToString(),
                UsedToken = stats.UsedToken.ToString(),
                HeroIdentifier = stats.HeroProductID?.ToString() ?? "0", // TODO: Map to Hero_Name
                ClanTag = stats.ClanTag ?? string.Empty,
                AlternativeAvatarName = stats.AlternativeAvatarName ?? string.Empty,
                SeasonProgress = new SeasonProgress
                {
                    MatchID = stats.MatchID,
                    AccountID = stats.AccountID,
                    IsCasual = "0",
                    MMRBefore = "1500.00",
                    MMRAfter = "1505.00",
                    MedalBefore = "11",
                    MedalAfter = "11",
                    Season = "12",
                    PlacementMatches = 0,
                    PlacementWins = "0"
                }
            }),
            MatchPlayerInventories = playerStatistics.ToDictionary(stats => stats.AccountID, stats => new MatchPlayerInventory
            {
                AccountID = stats.AccountID,
                MatchID = stats.MatchID,
                Slot1 = stats.Inventory?.ElementAtOrDefault(0) ?? string.Empty,
                Slot2 = stats.Inventory?.ElementAtOrDefault(1) ?? string.Empty,
                Slot3 = stats.Inventory?.ElementAtOrDefault(2) ?? string.Empty,
                Slot4 = stats.Inventory?.ElementAtOrDefault(3) ?? string.Empty,
                Slot5 = stats.Inventory?.ElementAtOrDefault(4) ?? string.Empty,
                Slot6 = stats.Inventory?.ElementAtOrDefault(5) ?? string.Empty
            }),
            MatchMastery = new MatchMastery((requesterStats?.HeroProductID.ToString() ?? "Hero_Legionnaire"), 0, 0, 0) // TODO: Implement Mastery
            {
                HeroIdentifier = (requesterStats?.HeroProductID.ToString() ?? "Hero_Legionnaire"),
                CurrentMasteryExperience = 0,
                MatchMasteryExperience = 0,
                MasteryExperienceBonus = 0,
                MasteryExperienceBoost = 0,
                MasteryExperienceSuperBoost = 0,
                MasteryExperienceMaximumLevelHeroesCount = 0,
                MasteryExperienceHeroesBonus = 0,
                MasteryExperienceToBoost = 0,
                MasteryExperienceEventBonus = 0,
                MasteryExperienceCanBoost = true,
                MasteryExperienceCanSuperBoost = true,
                MasteryExperienceBoostProductIdentifier = 3609,
                MasteryExperienceSuperBoostProductIdentifier = 4605,
                MasteryExperienceBoostProductCount = 0,
                MasteryExperienceSuperBoostProductCount = 0
            },
            OwnedStoreItems = account.User.OwnedStoreItems,
            SelectedStoreItems = account.SelectedStoreItems,
            CustomIconSlotID = SetCustomIconSlotID(account),
            CampaignReward = new CampaignReward
            {
                PreviousCampaignLevel = 5,
                CurrentCampaignLevel = 6,
                NextLevel = 0,
                RequireRank = 0,
                NeedMorePlay = 0,
                PercentageBefore = "0.92",
                Percentage = "1.00"
            }
        };

        Logger.LogInformation("Successfully Retrieved Match Stats For Match ID {MatchID} Requested By {AccountName}", matchID, accountName);

        return Ok(PhpSerialization.Serialize(response));
    }

    private static string SetCustomIconSlotID(Account account)
        => account.SelectedStoreItems.Any(item => item.StartsWith("ai.custom_icon"))
            ? account.SelectedStoreItems.Single(item => item.StartsWith("ai.custom_icon")).Replace("ai.custom_icon:", string.Empty) : "0";

    private static Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> SetOwnedStoreItemsData(Account account)
    {
        Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> items = account.User.OwnedStoreItems
            .Where(item => item.StartsWith("ma.").Equals(false) && item.StartsWith("cp.").Equals(false))
            .ToDictionary<string, string, OneOf<StoreItemData, StoreItemDiscountCoupon>>(upgrade => upgrade, upgrade => new StoreItemData());

        // TODO: Add Mastery Boosts And Coupons

        return items;
    }
}
