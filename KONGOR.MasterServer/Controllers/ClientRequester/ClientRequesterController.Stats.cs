namespace KONGOR.MasterServer.Controllers.ClientRequester;

public partial class ClientRequesterController
{
    private async Task<IActionResult> GetPlayerAwardSummary()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        List<AccountStatistics> allAccountStatistics = await MerrickContext.AccountStatistics
            .Where(statistics => statistics.AccountID == account.ID)
            .ToListAsync();

        AwardStatisticsSummary aggregatedAwards = new ();

        foreach (AccountStatistics statistics in allAccountStatistics)
        {
            aggregatedAwards.MVPAwards += statistics.AwardStatistics.MVPAwards;
            aggregatedAwards.AnnihilationAwards += statistics.AwardStatistics.AnnihilationAwards;
            aggregatedAwards.QuadKillAwards += statistics.AwardStatistics.QuadKillAwards;
            aggregatedAwards.LongestKillStreakAwards += statistics.AwardStatistics.LongestKillStreakAwards;
            aggregatedAwards.SmackdownAwards += statistics.AwardStatistics.SmackdownAwards;
            aggregatedAwards.MostKillsAwards += statistics.AwardStatistics.MostKillsAwards;
            aggregatedAwards.MostAssistsAwards += statistics.AwardStatistics.MostAssistsAwards;
            aggregatedAwards.LeastDeathsAwards += statistics.AwardStatistics.LeastDeathsAwards;
            aggregatedAwards.MostBuildingDamageAwards += statistics.AwardStatistics.MostBuildingDamageAwards;
            aggregatedAwards.MostWardsDestroyedAwards += statistics.AwardStatistics.MostWardsDestroyedAwards;
            aggregatedAwards.MostHeroDamageDealtAwards += statistics.AwardStatistics.MostHeroDamageDealtAwards;
            aggregatedAwards.HighestCreepScoreAwards += statistics.AwardStatistics.HighestCreepScoreAwards;
        }

        GetPlayerAwardSummaryResponse response = new ()
        {
            AccountID = account.ID.ToString(),

            MVPAwards = aggregatedAwards.MVPAwards.ToString(),
            AnnihilationAwards = aggregatedAwards.AnnihilationAwards.ToString(),
            QuadKillAwards = aggregatedAwards.QuadKillAwards.ToString(),
            LongestKillStreakAwards = aggregatedAwards.LongestKillStreakAwards.ToString(),
            SmackdownAwards = aggregatedAwards.SmackdownAwards.ToString(),
            MostKillsAwards = aggregatedAwards.MostKillsAwards.ToString(),
            MostAssistsAwards = aggregatedAwards.MostAssistsAwards.ToString(),
            LeastDeathsAwards = aggregatedAwards.LeastDeathsAwards.ToString(),
            MostBuildingDamageAwards = aggregatedAwards.MostBuildingDamageAwards.ToString(),
            MostWardsDestroyedAwards = aggregatedAwards.MostWardsDestroyedAwards.ToString(),
            MostHeroDamageDealtAwards = aggregatedAwards.MostHeroDamageDealtAwards.ToString(),
            HighestCreepScoreAwards = aggregatedAwards.HighestCreepScoreAwards.ToString()
        };

        // TODO: Most Wards Destroyed Awards Seems To Be Missing From The Client UI, Find Out Why

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> GetSeasons()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        int[] seasons = [ SeasonInformation.CurrentSeasonIndex ];

        GetSeasonsResponse response = new ()
        {
            AllSeasons = string.Join("|", seasons.Select(season => $"{season},0|{season},1"))
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Returns a paginated overview of recent match history for the specified account.
    ///     Supports different table types: "player" (public matches), "campaign" and "campaign_casual" (ranked/casual matchmaking).
    ///     Each entry contains the match ID, outcome, team, hero information, duration, map, and datetime.
    /// </summary>
    private async Task<IActionResult> GetMatchHistoryOverview()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        string? table = Request.Form["table"];

        if (table is null)
            return BadRequest(@"Missing Value For Form Parameter ""table""");

        int limit = int.TryParse(Request.Form["num"], out int parsedLimit) ? parsedLimit : 100;

        // Retrieve The Most Recent Match Entries For The Account, Joined With Match Statistics For Map And Datetime
        List<(MatchParticipantStatistics Participant, MatchStatistics Match)> matchEntries = await MerrickContext.MatchParticipantStatistics
            .Where(participant => participant.AccountID == account.ID)
            .Join(MerrickContext.MatchStatistics, participant => participant.MatchID, match => match.MatchID, (participant, match) => new { Participant = participant, Match = match })
            .OrderByDescending(entry => entry.Match.TimestampRecorded)
            .Take(limit)
            .Select(entry => new ValueTuple<MatchParticipantStatistics, MatchStatistics>(entry.Participant, entry.Match))
            .ToListAsync();

        List<MatchHistoryOverviewEntry> entries = [];

        // Build The Response Entries
        for (int index = 0; index < matchEntries.Count; index++)
        {
            (MatchParticipantStatistics participant, MatchStatistics match) = matchEntries[index];

            entries.Add(new MatchHistoryOverviewEntry
            {
                MatchID = match.MatchID.ToString(),
                Wins = participant.Win.ToString(),
                Team = participant.Team.ToString(),
                HeroKills = participant.HeroKills.ToString(),
                Deaths = participant.HeroDeaths.ToString(),
                HeroAssists = participant.HeroAssists.ToString(),
                HeroID = (participant.HeroProductID ?? 0).ToString(),
                SecondsPlayed = participant.SecondsPlayed.ToString(),
                Map = match.Map,
                MatchDatetime = match.TimestampRecorded.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                HeroClientName = participant.HeroIdentifier
            });
        }

        MatchHistoryOverviewResponse response = new () { Entries = entries };

        return Ok(response.Serialise());
    }

    private async Task<IActionResult> GetSimpleStatistics()
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

        List<AccountStatistics> allAccountStatistics = await MerrickContext.AccountStatistics
            .Where(statistics => statistics.AccountID == account.ID)
            .ToListAsync();

        Dictionary<AccountStatisticsType, AccountStatistics> statisticsByType = allAccountStatistics.ToDictionary(statistics => statistics.Type);

        AggregateStatistics aggregates = AggregateStatistics.FromStatistics(statisticsByType);

        // Aggregate Award Counts Across All Game Modes
        AwardStatisticsSummary aggregatedAwards = new ();

        foreach (AccountStatistics statistics in allAccountStatistics)
        {
            aggregatedAwards.MVPAwards += statistics.AwardStatistics.MVPAwards;
            aggregatedAwards.AnnihilationAwards += statistics.AwardStatistics.AnnihilationAwards;
            aggregatedAwards.QuadKillAwards += statistics.AwardStatistics.QuadKillAwards;
            aggregatedAwards.LongestKillStreakAwards += statistics.AwardStatistics.LongestKillStreakAwards;
            aggregatedAwards.SmackdownAwards += statistics.AwardStatistics.SmackdownAwards;
            aggregatedAwards.MostKillsAwards += statistics.AwardStatistics.MostKillsAwards;
            aggregatedAwards.MostAssistsAwards += statistics.AwardStatistics.MostAssistsAwards;
            aggregatedAwards.LeastDeathsAwards += statistics.AwardStatistics.LeastDeathsAwards;
            aggregatedAwards.MostBuildingDamageAwards += statistics.AwardStatistics.MostBuildingDamageAwards;
            aggregatedAwards.MostWardsDestroyedAwards += statistics.AwardStatistics.MostWardsDestroyedAwards;
            aggregatedAwards.MostHeroDamageDealtAwards += statistics.AwardStatistics.MostHeroDamageDealtAwards;
            aggregatedAwards.HighestCreepScoreAwards += statistics.AwardStatistics.HighestCreepScoreAwards;
        }

        // Determine Top 4 Awards By Count
        List<(string Name, int Count)> allAwards =
        [
            ("awd_masst", aggregatedAwards.MostAssistsAwards),
            ("awd_mhdd", aggregatedAwards.MostHeroDamageDealtAwards),
            ("awd_mbdmg", aggregatedAwards.MostBuildingDamageAwards),
            ("awd_lgks", aggregatedAwards.LongestKillStreakAwards),
            ("awd_mkills", aggregatedAwards.MostKillsAwards),
            ("awd_ldths", aggregatedAwards.LeastDeathsAwards),
            ("awd_mqk", aggregatedAwards.QuadKillAwards),
            ("awd_smkd", aggregatedAwards.SmackdownAwards),
            ("awd_annih", aggregatedAwards.AnnihilationAwards),
            ("awd_mwk", aggregatedAwards.MostWardsDestroyedAwards),
            ("awd_hcs", aggregatedAwards.HighestCreepScoreAwards)
        ];

        List<(string Name, int Count)> top4Awards = [.. allAwards.OrderByDescending(award => award.Count).Take(4)];

        // Build Season Statistics From Ranked And Casual Matchmaking
        int rankedWins = statisticsByType.TryGetValue(AccountStatisticsType.Matchmaking, out AccountStatistics? rankedStatistics) ? rankedStatistics.MatchesWon : 0;
        int rankedLosses = statisticsByType.TryGetValue(AccountStatisticsType.Matchmaking, out _) ? rankedStatistics?.MatchesLost ?? 0 : 0;

        int casualWins = statisticsByType.TryGetValue(AccountStatisticsType.MatchmakingCasual, out AccountStatistics? casualStatistics) ? casualStatistics.MatchesWon : 0;
        int casualLosses = statisticsByType.TryGetValue(AccountStatisticsType.MatchmakingCasual, out _) ? casualStatistics?.MatchesLost ?? 0 : 0;

        ShowSimpleStatsResponse response = new ()
        {
            NameWithClanTag = account.NameWithClanTag,
            ID = account.ID.ToString(),
            Level = account.User.TotalLevel,
            LevelExperience = account.User.TotalExperience,
            NumberOfAvatarsOwned = account.User.OwnedStoreItems.Count(item => item.StartsWith("aa.")),
            TotalMatchesPlayed = aggregates.TotalGamesPlayed,
            CurrentSeason = SeasonInformation.CurrentSeasonIndex,
            SimpleSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = rankedWins,
                RankedMatchesLost = rankedLosses,
                WinStreak = 0, // TODO: Implement Win Streak Tracking
                InPlacementPhase = (rankedStatistics?.IsInPlacementPhase ?? false) ? 1 : 0,
                LevelsGainedThisSeason = account.User.TotalLevel
            },
            SimpleCasualSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = casualWins,
                RankedMatchesLost = casualLosses,
                WinStreak = 0, // TODO: Implement Win Streak Tracking
                InPlacementPhase = (casualStatistics?.IsInPlacementPhase ?? false) ? 1 : 0,
                LevelsGainedThisSeason = account.User.TotalLevel
            },
            MVPAwardsCount = aggregatedAwards.MVPAwards,
            Top4AwardNames = [.. top4Awards.Select(award => award.Name)],
            Top4AwardCounts = [.. top4Awards.Select(award => award.Count)],
            CustomIconSlotID = SetCustomIconSlotID(account),
            OwnedStoreItems = account.User.OwnedStoreItems,
            SelectedStoreItems = account.SelectedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(account)
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> GetStatistics()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User).ThenInclude(user => user.Accounts)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        string? table = Request.Form["table"];

        if (table is null)
            return BadRequest(@"Missing Value For Form Parameter ""table""");

        List<AccountStatistics> allAccountStatistics = await MerrickContext.AccountStatistics
            .Where(statistics => statistics.AccountID == account.ID).ToListAsync();

        Dictionary<AccountStatisticsType, AccountStatistics> statisticsByType = allAccountStatistics.ToDictionary(statistics => statistics.Type);

        AggregateStatistics aggregates = AggregateStatistics.FromStatistics(statisticsByType);

        if (table is "player")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.Public];

            PlayerStatisticsResponse response = new (account, statistics, aggregates);

            return Ok(PhpSerialization.Serialize(response));
        }

        if (table is "ranked")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.Matchmaking];

            RankedStatisticsResponse response = new (account, statistics, aggregates);

            return Ok(PhpSerialization.Serialize(response));
        }

        if (table is "casual")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.MatchmakingCasual];

            CasualStatisticsResponse response = new (account, statistics, aggregates);

            return Ok(PhpSerialization.Serialize(response));
        }

        if (table is "campaign")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.Matchmaking];

            CampaignStatisticsResponse response = new (account, statistics, aggregates);

            return Ok(PhpSerialization.Serialize(response));
        }

        if (table is "campaign_casual")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.MatchmakingCasual];

            CampaignCasualStatisticsResponse response = new (account, statistics, aggregates);

            return Ok(PhpSerialization.Serialize(response));
        }

        if (table is "mastery")
        {
            ShowMasteryStatisticsResponse response = new (account);

            // The Mastery And Mastery Rewards Rows Are Created During Statistics Submission; Transient All-Zero Rows Are Used As A Fallback So That Reads Never Write To The Database
            Mastery mastery = await MerrickContext.Masteries.SingleOrDefaultAsync(record => record.AccountID == account.ID)
                ?? new Mastery { Account = account };

            MasteryRewards rewards = await MerrickContext.MasteryRewards.SingleOrDefaultAsync(record => record.AccountID == account.ID)
                ?? new MasteryRewards { Account = account };

            // Every Hero Is Reported (Including Those With No Experience) So The Client Renders The Full Mastery Grid
            response.MasteryInfo = Heroes.AllHeroIdentifiers()
                .Select(identifier => new HeroMasteryInfo { HeroName = identifier, Experience = mastery.GetHeroExperienceByHeroIdentifier(identifier) }).ToList();

            response.MasteryRewards = JSONConfiguration.MasteryRewardsConfiguration.MasteryRewards
                .Select(configuredReward => new MasteryRewardTier
                {
                    Level = configuredReward.RequiredLevel,
                    AlreadyClaimed = rewards.HasObtained(configuredReward.RequiredLevel),
                    Reward = new global::KONGOR.MasterServer.Models.RequestResponse.Stats.MasteryReward
                    {
                        ProductID = configuredReward.ProductIdentifier,
                        ProductName = configuredReward.ProductName ?? string.Empty,
                        ProductLocalContent = configuredReward.ProductLocalResource ?? string.Empty,
                        Quantity = configuredReward.ProductQuantity,
                        GoldCoins = configuredReward.GoldCoins,
                        SilverCoins = configuredReward.SilverCoins,
                        GameTokens = configuredReward.PlinkoTickets
                    }
                }).ToList();

            return Ok(PhpSerialization.Serialize(response));
        }

        if (table is "campaign_history" or "history")
        {
            // The "is_casual" Parameter Determines Whether To Return Campaign Normal Or Campaign Casual Statistics
            bool isCasual = Request.Form["is_casual"].ToString() is "1";

            if (isCasual)
            {
                AccountStatistics statistics = statisticsByType[AccountStatisticsType.MatchmakingCasual];

                CampaignCasualStatisticsResponse response = new (account, statistics, aggregates);

                return Ok(PhpSerialization.Serialize(response));
            }

            else
            {
                AccountStatistics statistics = statisticsByType[AccountStatisticsType.Matchmaking];

                CampaignStatisticsResponse response = new (account, statistics, aggregates);

                return Ok(PhpSerialization.Serialize(response));
            }
        }

        throw new ArgumentOutOfRangeException(nameof(table), table, $@"Unsupported Value For Form Parameter ""table"": ""{table}""");
    }

    private async Task<IActionResult> GetHeroStatistics()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        Dictionary<AccountStatisticsType, AccountStatistics> statisticsByType = await MerrickContext.AccountStatistics
            .Where(statistics => statistics.AccountID == account.ID)
            .ToDictionaryAsync(statistics => statistics.Type);

        List<RankedHeroStatistics> rankedStats = [];

        // Build Ranked Hero Statistics
        if (statisticsByType.TryGetValue(AccountStatisticsType.Matchmaking, out AccountStatistics? matchmakingStatistics))
        {
            rankedStats = [.. matchmakingStatistics.HeroStatistics.Heroes.Select(heroStats => new RankedHeroStatistics
            {
                HeroIdentifier = heroStats.HeroIdentifier,
                TimesUsed = heroStats.GamesPlayed.ToString(),
                Wins = heroStats.Wins.ToString(),
                Losses = heroStats.Losses.ToString(),
                HeroKills = heroStats.HeroKills.ToString(),
                Deaths = heroStats.HeroDeaths.ToString(),
                HeroAssists = heroStats.HeroAssists.ToString(),
                TeamCreepKills = heroStats.TeamCreepKills.ToString(),
                Denies = heroStats.Denies.ToString(),
                Experience = heroStats.Experience.ToString(),
                Gold = heroStats.Gold.ToString(),
                Actions = heroStats.Actions.ToString(),
                TimeEarningExperience = heroStats.TimeEarningExperience.ToString()
            })];
        }

        List<CasualHeroStatistics> casualStats = [];

        // Build Casual Hero Statistics
        if (statisticsByType.TryGetValue(AccountStatisticsType.MatchmakingCasual, out AccountStatistics? casualStatistics))
        {
            casualStats = [.. casualStatistics.HeroStatistics.Heroes.Select(heroStats => new CasualHeroStatistics
            {
                HeroIdentifier = heroStats.HeroIdentifier,
                TimesUsed = heroStats.GamesPlayed.ToString(),
                Wins = heroStats.Wins.ToString(),
                Losses = heroStats.Losses.ToString(),
                HeroKills = heroStats.HeroKills.ToString(),
                Deaths = heroStats.HeroDeaths.ToString(),
                HeroAssists = heroStats.HeroAssists.ToString(),
                TeamCreepKills = heroStats.TeamCreepKills.ToString(),
                Denies = heroStats.Denies.ToString(),
                Experience = heroStats.Experience.ToString(),
                Gold = heroStats.Gold.ToString(),
                Actions = heroStats.Actions.ToString(),
                TimeEarningExperience = heroStats.TimeEarningExperience.ToString()
            })];
        }

        List<CampaignHeroStatistics> campaignStats = [];

        // Build Campaign Normal Hero Statistics
        if (statisticsByType.TryGetValue(AccountStatisticsType.Matchmaking, out AccountStatistics? campaignStatisticsSource))
        {
            campaignStats = [.. campaignStatisticsSource.HeroStatistics.Heroes.Select(heroStats => new CampaignHeroStatistics
            {
                HeroIdentifier = heroStats.HeroIdentifier,
                TimesUsed = heroStats.GamesPlayed.ToString(),
                Wins = heroStats.Wins.ToString(),
                Losses = heroStats.Losses.ToString(),
                HeroKills = heroStats.HeroKills.ToString(),
                Deaths = heroStats.HeroDeaths.ToString(),
                HeroAssists = heroStats.HeroAssists.ToString(),
                TeamCreepKills = heroStats.TeamCreepKills.ToString(),
                Denies = heroStats.Denies.ToString(),
                Experience = heroStats.Experience.ToString(),
                Gold = heroStats.Gold.ToString(),
                Actions = heroStats.Actions.ToString(),
                TimeEarningExperience = heroStats.TimeEarningExperience.ToString()
            })];
        }

        List<CampaignCasualHeroStatistics> campaignCasualStats = [];

        // Build Campaign Casual Hero Statistics
        if (statisticsByType.TryGetValue(AccountStatisticsType.MatchmakingCasual, out AccountStatistics? campaignCasualStatisticsSource))
        {
            campaignCasualStats = [.. campaignCasualStatisticsSource.HeroStatistics.Heroes.Select(heroStats => new CampaignCasualHeroStatistics
            {
                HeroIdentifier = heroStats.HeroIdentifier,
                TimesUsed = heroStats.GamesPlayed.ToString(),
                Wins = heroStats.Wins.ToString(),
                Losses = heroStats.Losses.ToString(),
                HeroKills = heroStats.HeroKills.ToString(),
                Deaths = heroStats.HeroDeaths.ToString(),
                HeroAssists = heroStats.HeroAssists.ToString(),
                TeamCreepKills = heroStats.TeamCreepKills.ToString(),
                Denies = heroStats.Denies.ToString(),
                Experience = heroStats.Experience.ToString(),
                Gold = heroStats.Gold.ToString(),
                Actions = heroStats.Actions.ToString(),
                TimeEarningExperience = heroStats.TimeEarningExperience.ToString()
            })];
        }

        GetHeroStatisticsResponse response = new ()
        {
            AllHeroStatistics = new AllHeroStatistics
            {
                Ranked = rankedStats,
                Casual = casualStats,
                Campaign = campaignStats,
                CampaignCasual = campaignCasualStats
            }
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Returns the detailed statistics for a single hero across the ranked, casual, and player (public) game modes.
    ///     Each game mode contributes the full detailed field set, prefixed by "rnk_" for ranked, "cs_" for casual, and no prefix for player statistics.
    ///     Fields that are not currently tracked are returned as "0", mirroring the original API.
    /// </summary>
    private async Task<IActionResult> GetSelectedHeroStatistics()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        string? heroIdentifier = Request.Form["hero"];

        if (heroIdentifier is null)
            return BadRequest(@"Missing Value For Form Parameter ""hero""");

        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        Dictionary<AccountStatisticsType, AccountStatistics> statisticsByType = await MerrickContext.AccountStatistics
            .Where(statistics => statistics.AccountID == account.ID)
            .ToDictionaryAsync(statistics => statistics.Type);

        HeroStats? rankedHeroStatistics = RetrieveHeroStatistics(statisticsByType, AccountStatisticsType.Matchmaking, heroIdentifier);
        HeroStats? casualHeroStatistics = RetrieveHeroStatistics(statisticsByType, AccountStatisticsType.MatchmakingCasual, heroIdentifier);
        HeroStats? playerHeroStatistics = RetrieveHeroStatistics(statisticsByType, AccountStatisticsType.Public, heroIdentifier);

        OrderedDictionary response = new ();

        response.Add("success", 1);
        response.Add("errors", string.Empty);

        DetailedHeroStatisticsFields.Write(response, "rnk_", rankedHeroStatistics);
        DetailedHeroStatisticsFields.Write(response, "cs_", casualHeroStatistics);
        DetailedHeroStatisticsFields.Write(response, string.Empty, playerHeroStatistics);

        response.Add("vested_threshold", 5);
        response.Add(0, true);

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Returns the detailed statistics for a single hero in either the campaign normal or campaign casual game mode for the current season.
    ///     The "is_casual" form parameter selects the game mode: "1" for campaign casual (the "cam_cs_" field prefix) or "0" for campaign normal (the "cam_" field prefix).
    ///     When the account has no recorded statistics for the hero, only the trailing response metadata is returned, mirroring the original API.
    /// </summary>
    private async Task<IActionResult> GetCampaignHeroStatistics()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        string? heroIdentifier = Request.Form["hero_name"];

        if (heroIdentifier is null)
            return BadRequest(@"Missing Value For Form Parameter ""hero_name""");

        string? isCasualValue = Request.Form["is_casual"];

        if (isCasualValue is null)
            return BadRequest(@"Missing Value For Form Parameter ""is_casual""");

        bool isCasual = isCasualValue is "1";

        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        AccountStatisticsType statisticsType = isCasual ? AccountStatisticsType.MatchmakingCasual : AccountStatisticsType.Matchmaking;

        Dictionary<AccountStatisticsType, AccountStatistics> statisticsByType = await MerrickContext.AccountStatistics
            .Where(statistics => statistics.AccountID == account.ID)
            .ToDictionaryAsync(statistics => statistics.Type);

        HeroStats? heroStatistics = RetrieveHeroStatistics(statisticsByType, statisticsType, heroIdentifier);

        OrderedDictionary response = new ();

        // When No Statistics Exist For The Hero, The Original API Returns Only The Trailing Response Metadata
        if (heroStatistics is not null)
        {
            StoreItem? heroStoreItem = JSONConfiguration.StoreItemsConfiguration.GetEnabledItemsByType(StoreItemType.Hero)
                .SingleOrDefault(item => item.Code.Equals(heroIdentifier, StringComparison.OrdinalIgnoreCase));

            response.Add("season", SeasonInformation.CurrentSeasonIndex.ToString());
            response.Add("account_id", account.ID.ToString());
            response.Add("hero_id", (heroStoreItem?.ID ?? 0).ToString());

            DetailedHeroStatisticsFields.Write(response, isCasual ? "cam_cs_" : "cam_", heroStatistics);
        }

        response.Add("vested_threshold", 5);
        response.Add(0, true);

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Returns the global hero usage list, ranking every hero by pick rate, win rate, or loss rate across all recorded account statistics.
    ///     The "sort" form parameter selects the ranking: "use" (the default), "win", or "loss".
    ///     The underlying per-hero totals are aggregated and cached by <see cref="HeroUsageStatisticsService"/>.
    /// </summary>
    private async Task<IActionResult> GetHeroUsageList()
    {
        string sort = Request.Form["sort"].ToString();

        if (string.IsNullOrEmpty(sort))
            sort = "use";

        if (sort is not "use" and not "win" and not "loss")
            return BadRequest($@"Unsupported Value For Form Parameter ""sort"": ""{sort}""");

        IReadOnlyList<HeroUsageStatistic> heroUsageStatistics = await HeroUsageStatistics.GetHeroUsageStatistics();

        int totalUse = heroUsageStatistics.Sum(statistic => statistic.Wins + statistic.Losses);

        IEnumerable<HeroUsageEntry> entries = heroUsageStatistics
            .Select(statistic => new HeroUsageEntry(statistic.HeroIdentifier, statistic.Wins, statistic.Losses, totalUse));

        // The "use" Sort Breaks Ties On Win Count
        List<HeroUsageEntry> sortedEntries = sort switch
        {
            "win"  => [.. entries.OrderByDescending(entry => entry.WinPercentage)],
            "loss" => [.. entries.OrderByDescending(entry => entry.LossPercentage)],
            _      => [.. entries.OrderByDescending(entry => entry.UsageCount).ThenByDescending(entry => entry.WinCount)]
        };

        OrderedDictionary response = new ();

        response.Add("success", 1);
        response.Add("errors", string.Empty);
        response.Add("total_use", totalUse);
        response.Add("data", string.Join('`', sortedEntries.Select(entry => entry.Serialise())));
        response.Add("vested_threshold", 5);
        response.Add(0, true);

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Retrieves the per-hero statistics for the given game mode and hero identifier, or <see langword="null"/> if the account has no statistics recorded for that combination.
    /// </summary>
    private static HeroStats? RetrieveHeroStatistics(Dictionary<AccountStatisticsType, AccountStatistics> statisticsByType, AccountStatisticsType type, string heroIdentifier)
        => statisticsByType.TryGetValue(type, out AccountStatistics? statistics)
            ? statistics.HeroStatistics.Heroes.SingleOrDefault(hero => hero.HeroIdentifier.Equals(heroIdentifier, StringComparison.OrdinalIgnoreCase)) : null;

    private async Task<IActionResult> GetMatchStatistics()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? matchID = Request.Form["match_id"];

        if (matchID is null)
            return BadRequest(@"Missing Value For Form Parameter ""match_id""");

        MatchStatistics? matchStatistics = await MerrickContext.MatchStatistics.SingleOrDefaultAsync(matchStatistics => matchStatistics.MatchID == int.Parse(matchID));

        if (matchStatistics is null)
            return new NotFoundObjectResult("Match Stats Not Found");

        List<MatchParticipantStatistics> allPlayerStatistics = await MerrickContext.MatchParticipantStatistics.Where(playerStatistics => playerStatistics.MatchID == matchStatistics.MatchID).ToListAsync();

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return new NotFoundObjectResult("Session Not Found");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return new NotFoundObjectResult("Account Not Found");

        // The Database Snapshot Is The Single Source Of Truth For Match Information Once Stats Have Been Submitted
        MatchInformation? matchInformation = matchStatistics.MatchInformationSnapshot is not null
            ? JsonSerializer.Deserialize<MatchInformation>(matchStatistics.MatchInformationSnapshot) : null;

        if (matchInformation is null)
            return new NotFoundObjectResult("Match Information Not Found");

        MatchSummary matchSummary = new (matchStatistics, allPlayerStatistics, matchInformation);

        List<int> otherPlayerAccountIDs = [.. allPlayerStatistics.Select(statistics => statistics.AccountID).Where(id => id != account.ID)];

        List<Account> otherPlayerAccounts = await MerrickContext.Accounts
            .Include(playerAccount => playerAccount.User)
            .Include(playerAccount => playerAccount.Clan)
            .Where(playerAccount => otherPlayerAccountIDs.Contains(playerAccount.ID))
            .ToListAsync();

        List<Account> allPlayerAccounts = [account, .. otherPlayerAccounts];

        Dictionary<int, OneOf<MatchPlayerStatisticsWithMatchPerformanceData, MatchPlayerStatistics>> matchPlayerStatistics = [];
        Dictionary<int, MatchPlayerInventory> matchPlayerInventories = [];

        foreach (MatchParticipantStatistics playerStatistics in allPlayerStatistics)
        {
            Account playerAccount = allPlayerAccounts.Single(playerAccount => playerAccount.ID == playerStatistics.AccountID);

            List<AccountStatistics> accountStatistics = await MerrickContext.AccountStatistics.Where(statistics => statistics.AccountID == playerStatistics.AccountID).ToListAsync();

            AccountStatisticsType statisticsType = MatchCompletionRewardsHandler.ResolveAccountStatisticsType(matchInformation);

            AccountStatistics currentMatchTypeStatistics = accountStatistics.Single(statistics => statistics.Type == statisticsType);

            AccountStatistics publicMatchStatistics = accountStatistics.Single(statistics => statistics.Type == AccountStatisticsType.Public);

            AccountStatistics matchmakingStatistics = accountStatistics.Single(statistics => statistics.Type == AccountStatisticsType.Matchmaking);

            // Use PrimaryMatchPlayerStatistics With Additional Information For The Primary (Requesting) Player And MatchPlayerStatistics With The Standard Amount Of Information For Secondary Players
            matchPlayerStatistics[playerStatistics.AccountID] = playerStatistics.AccountID == account.ID
                ? new MatchPlayerStatisticsWithMatchPerformanceData(matchInformation, playerAccount, playerStatistics, currentMatchTypeStatistics, publicMatchStatistics, matchmakingStatistics)
                    { HeroIdentifier = playerStatistics.HeroIdentifier }
                : new MatchPlayerStatistics(matchInformation, playerAccount, playerStatistics, currentMatchTypeStatistics, publicMatchStatistics, matchmakingStatistics)
                    { HeroIdentifier = playerStatistics.HeroIdentifier };

            List<string> inventory = playerStatistics.Inventory ?? [];

            matchPlayerInventories[playerStatistics.AccountID] = new MatchPlayerInventory
            {
                AccountID = playerStatistics.AccountID,
                MatchID = playerStatistics.MatchID,

                Slot1 = inventory.ElementAtOrDefault(0),
                Slot2 = inventory.ElementAtOrDefault(1),
                Slot3 = inventory.ElementAtOrDefault(2),
                Slot4 = inventory.ElementAtOrDefault(3),
                Slot5 = inventory.ElementAtOrDefault(4),
                Slot6 = inventory.ElementAtOrDefault(5)
            };
        }

        MatchParticipantStatistics? requestingPlayerStatistics = allPlayerStatistics.SingleOrDefault(statistics => statistics.AccountID == account.ID);

        MatchMastery matchMastery;

        // Spectators And Players Viewing Somebody Else's Match From The Match History Are Not Match Participants
        // The Client Only Reads The Mastery Block Against The Requesting Player's Own Row, So An Empty Block Is Sent For Them
        if (requestingPlayerStatistics is null)
        {
            matchMastery = new MatchMastery(string.Empty, 0, 0, 0)
            {
                MasteryExperienceMaximumLevelHeroesCount = 0,
                MasteryExperienceBoostProductCount = 0,
                MasteryExperienceSuperBoostProductCount = 0,
                MasteryExperienceCanBoost = false,
                MasteryExperienceCanSuperBoost = false
            };
        }

        else
        {
            // The Mastery Row Is Created During Statistics Submission; A Transient All-Zero Row Is Used As A Fallback So That Reads Never Write To The Database
            Mastery mastery = await MerrickContext.Masteries.SingleOrDefaultAsync(record => record.AccountID == account.ID)
                ?? new Mastery { Account = account };

            AccountStatisticsType masteryStatisticsType = MatchCompletionRewardsHandler.ResolveAccountStatisticsType(matchInformation);

            int heroMatchExperience = mastery.CalculateMatchExperience(masteryStatisticsType, requestingPlayerStatistics.HeroLevel);
            int heroBonusExperience = mastery.CalculateBonusExperience(masteryStatisticsType, Heroes.TotalHeroCount);
            int heroCurrentExperience = mastery.GetHeroExperienceByHeroIdentifier(requestingPlayerStatistics.HeroIdentifier);

            MasteryBoostContext? masteryBoostContext = await DistributedCache.GetMasteryBoostContext(account.ID, matchStatistics.MatchID);

            // The Match, Bonus, And Boost Experience Are Accrued Into The Persisted Total During Statistics Submission And Boost Application
            // The Client Treats "mastery_exp_original" As The Pre-Match Starting Value And Adds The Match, Bonus, And Boost Experience On Top Of It, So The Accrued Amounts Are Subtracted Back Out Here
            int preMatchExperience = Math.Max(0, heroCurrentExperience - heroMatchExperience - heroBonusExperience - (masteryBoostContext?.Experience ?? 0));

            // A Mastery Boost May Only Be Applied Once, Only To The Account's Most Recent Match Before Another Game Is Started, And Only Within The Boost Application Window
            // The Boost Is Therefore Disabled When An Older Match Is Viewed In The Match History
            // Match IDs Are Not Chronological, So The Most Recent Match Is Resolved By The Recorded Timestamp Rather Than By The Largest Match ID
            int mostRecentMatchID = await MerrickContext.MatchParticipantStatistics
                .Where(statistics => statistics.AccountID == account.ID)
                .Join(MerrickContext.MatchStatistics, participant => participant.MatchID, match => match.MatchID, (participant, match) => match)
                .OrderByDescending(match => match.TimestampRecorded)
                .Select(match => match.MatchID)
                .FirstAsync();

            bool isMostRecentMatch = matchStatistics.MatchID == mostRecentMatchID;

            bool masteryCanBoost = isMostRecentMatch && heroMatchExperience > 0 && masteryBoostContext is null
                && matchStatistics.TimestampRecorded >= DateTimeOffset.UtcNow - MasteryBoost.ApplicationWindow
                && Mastery.GetLevelFromExperience(heroCurrentExperience) < Mastery.MaximumMasteryLevel;

            // The Applied Experience Is Reported In The Response Field Matching The Boost Type, Mirroring The Original API Contract
            matchMastery = new MatchMastery(requestingPlayerStatistics.HeroIdentifier, preMatchExperience, heroMatchExperience, heroBonusExperience)
            {
                MasteryExperienceBoost = masteryBoostContext is { IsSuperBoost: false } ? masteryBoostContext.Experience : 0,
                MasteryExperienceSuperBoost = masteryBoostContext is { IsSuperBoost: true } ? masteryBoostContext.Experience : 0,
                MasteryExperienceMaximumLevelHeroesCount = mastery.HeroesAtMaximumMasteryCount(),
                MasteryExperienceBoostProductCount = MasteryConsumables.MasteryBoostsOwned(account.User),
                MasteryExperienceSuperBoostProductCount = MasteryConsumables.SuperMasteryBoostsOwned(account.User),
                MasteryExperienceCanBoost = masteryCanBoost,
                MasteryExperienceCanSuperBoost = masteryCanBoost
            };
        }

        MatchStatsResponse response = new ()
        {
            GoldCoins = account.User.GoldCoins.ToString(),
            SilverCoins = account.User.SilverCoins.ToString(),
            MatchSummary = new Dictionary<int, MatchSummary> { { matchStatistics.MatchID, matchSummary } },
            MatchPlayerStatistics = new Dictionary<int, Dictionary<int, OneOf<MatchPlayerStatisticsWithMatchPerformanceData, MatchPlayerStatistics>>> { { matchStatistics.MatchID, matchPlayerStatistics } },
            MatchPlayerInventories = new Dictionary<int, Dictionary<int, MatchPlayerInventory>> { { matchStatistics.MatchID, matchPlayerInventories } },
            MatchMastery = matchMastery,
            OwnedStoreItems = account.User.OwnedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(account),
            SelectedStoreItems = account.SelectedStoreItems,
            CustomIconSlotID = SetCustomIconSlotID(account)
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Cached response for "get_products", lazily computed on first request.
    ///     The products list is static (derived from the store configuration loaded at startup) so it never changes at runtime.
    /// </summary>
    private static readonly Lazy<string> CachedGetProductsResponse = new (() =>
    {
        GetProductsResponse response = new (JSONConfiguration.StoreItemsConfiguration);

        return PhpSerialization.Serialize(response);
    });

    /// <summary>
    ///     Returns all enabled store products grouped by category.
    ///     Called by the client after authentication to populate the in-game store product catalogue.
    /// </summary>
    private IActionResult GetProducts()
    {
        return Ok(CachedGetProductsResponse.Value);
    }

    /// <summary>
    ///     Returns account field statistics, owned store items, selected store items, currency balances, and other metadata.
    ///     Called by the client during gameplay and on login to refresh the client's upgrades and account data.
    /// </summary>
    private async Task<IActionResult> GetUpgrades()
    {
        string cookie = Request.Form["cookie"].ToString();

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return Unauthorized($@"Unrecognised Cookie ""{cookie}""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        Dictionary<AccountStatisticsType, AccountStatistics> statisticsByType = await MerrickContext.AccountStatistics
            .Where(statistics => statistics.AccountID == account.ID)
            .ToDictionaryAsync(statistics => statistics.Type);

        AggregateStatistics aggregates = AggregateStatistics.FromStatistics(statisticsByType);

        FieldStatisticsEntry fieldStatisticsEntry = FieldStatisticsEntry.FromAccount(account, aggregates, statisticsByType);

        GetUpgradesResponse response = new ()
        {
            FieldStatistics = new Dictionary<int, FieldStatisticsEntry> { { account.ID, fieldStatisticsEntry } },
            OwnedStoreItems = account.User.OwnedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(account),
            SelectedStoreItems = account.SelectedStoreItems,
            GoldCoins = account.User.GoldCoins,
            SilverCoins = account.User.SilverCoins
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Returns initial account statistics used to refresh the client's account information after a match ends.
    ///     Contains level, experience, skill ratings, games played, and disconnections per game mode.
    /// </summary>
    private async Task<IActionResult> GetInitialStatistics()
    {
        string cookie = Request.Form["cookie"].ToString();

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return Unauthorized($@"Unrecognised Cookie ""{cookie}""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        Dictionary<AccountStatisticsType, AccountStatistics> statisticsByType = await MerrickContext.AccountStatistics
            .Where(statistics => statistics.AccountID == account.ID)
            .ToDictionaryAsync(statistics => statistics.Type);

        AggregateStatistics aggregates = AggregateStatistics.FromStatistics(statisticsByType);

        FieldStatisticsEntry fieldStatisticsEntry = FieldStatisticsEntry.FromAccount(account, aggregates, statisticsByType);

        GetInitialStatisticsResponse response = new ()
        {
            Information = new Dictionary<int, FieldStatisticsEntry> { { account.ID, fieldStatisticsEntry } }
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    private static string SetCustomIconSlotID(Account account)
        => account.SelectedStoreItems.Any(item => item.StartsWith("ai.custom_icon"))
            ? account.SelectedStoreItems.Single(item => item.StartsWith("ai.custom_icon")).Replace("ai.custom_icon:", string.Empty) : "0";

    private static Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> SetOwnedStoreItemsData(Account account)
    {
        Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> items = StatisticsResponseHelper.GetOwnedStoreItemsData(account);

        /*
            Dictionary<string, object> myUpgradesInfo = accountDetails.UnlockedUpgradeCodes
                .Where(upgrade => upgrade.StartsWith("ma.").Equals(false) && upgrade.StartsWith("cp.").Equals(false))
                .ToDictionary<string, string, object>(upgrade => upgrade, upgrade => new MyUpgradesInfoEntry());

            foreach (string boost in GameConsumables.GetOwnedMasteryBoostProducts(accountDetails.UnlockedUpgradeCodes))
                myUpgradesInfo.Add(boost, new MyUpgradesInfoEntry());

            foreach (KeyValuePair<string, Coupon> coupon in GameConsumables.GetOwnedCoupons(accountDetails.UnlockedUpgradeCodes))
                myUpgradesInfo.Add(coupon.Key, coupon.Value);

            return myUpgradesInfo;
         */

        return items;
    }
}

/// <summary>
///     A single hero entry in the "get_hero_usage_list" response.
///     Serialises to the pipe-delimited format "identifier|use%|win%|loss%|use_count|win_count|loss_count", where the percentages are formatted to one decimal place.
/// </summary>
file sealed class HeroUsageEntry(string heroIdentifier, int wins, int losses, int totalUse)
{
    /// <summary>
    ///     The number of wins with the hero across all resolved matches.
    /// </summary>
    public int WinCount { get; } = wins;

    /// <summary>
    ///     The number of losses with the hero across all resolved matches.
    /// </summary>
    public int LossCount { get; } = losses;

    /// <summary>
    ///     The number of resolved matches in which the hero was used (the sum of wins and losses).
    /// </summary>
    public int UsageCount { get; } = wins + losses;

    /// <summary>
    ///     The hero's share of all resolved matches, as a percentage.
    /// </summary>
    public double UsagePercentage => totalUse > 0 ? (double) UsageCount / totalUse * 100.0 : 0.0;

    /// <summary>
    ///     The hero's win rate across its resolved matches, as a percentage.
    /// </summary>
    public double WinPercentage => UsageCount > 0 ? (double) WinCount / UsageCount * 100.0 : 0.0;

    /// <summary>
    ///     The hero's loss rate across its resolved matches, as a percentage.
    /// </summary>
    public double LossPercentage => UsageCount > 0 ? (double) LossCount / UsageCount * 100.0 : 0.0;

    public string Serialise() => string.Join
    (
        '|', heroIdentifier,
        UsagePercentage.ToString("0.0", CultureInfo.InvariantCulture),
        WinPercentage.ToString("0.0", CultureInfo.InvariantCulture),
        LossPercentage.ToString("0.0", CultureInfo.InvariantCulture),
        UsageCount, WinCount, LossCount
    );
}
