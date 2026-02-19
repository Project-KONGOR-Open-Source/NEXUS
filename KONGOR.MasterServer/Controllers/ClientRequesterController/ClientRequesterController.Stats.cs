namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

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

        int[] seasons = [ 666 ];

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
            .OrderByDescending(entry => entry.Match.MatchID)
            .Take(limit)
            .Select(entry => new ValueTuple<MatchParticipantStatistics, MatchStatistics>(entry.Participant, entry.Match))
            .ToListAsync();

        int totalEntries = matchEntries.Count + 2;

        // Build The PHP Serialised Response Manually To Avoid Untyped Dictionaries
        // The Response Is A Flat PHP Array With Dynamic String Keys ("m0", "m1", ...), A String Key ("vested_threshold"), And An Integer Key (0)

        StringBuilder serialised = new ();

        serialised.Append($"a:{totalEntries}:{{");

        for (int index = 0; index < matchEntries.Count; index++)
        {
            (MatchParticipantStatistics participant, MatchStatistics match) = matchEntries[index];

            MatchHistoryOverviewEntry entry = new ()
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
            };

            serialised.Append(PhpSerialization.Serialize($"m{index}"));
            serialised.Append(PhpSerialization.Serialize(entry));
        }

        serialised.Append(PhpSerialization.Serialize("vested_threshold"));
        serialised.Append(PhpSerialization.Serialize(5));
        serialised.Append(PhpSerialization.Serialize(0));
        serialised.Append(PhpSerialization.Serialize(true));

        serialised.Append('}');

        return Ok(serialised.ToString());
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
        int rankedLosses = statisticsByType.TryGetValue(AccountStatisticsType.Matchmaking, out _) ? rankedStatistics!.MatchesLost : 0;

        int casualWins = statisticsByType.TryGetValue(AccountStatisticsType.MatchmakingCasual, out AccountStatistics? casualStatistics) ? casualStatistics.MatchesWon : 0;
        int casualLosses = statisticsByType.TryGetValue(AccountStatisticsType.MatchmakingCasual, out _) ? casualStatistics!.MatchesLost : 0;

        ShowSimpleStatsResponse response = new ()
        {
            NameWithClanTag = account.NameWithClanTag,
            ID = account.ID.ToString(),
            Level = account.User.TotalLevel,
            LevelExperience = account.User.TotalExperience,
            NumberOfAvatarsOwned = account.User.OwnedStoreItems.Count(item => item.StartsWith("aa.")),
            TotalMatchesPlayed = aggregates.TotalGamesPlayed,
            CurrentSeason = 666,
            SimpleSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = rankedWins,
                RankedMatchesLost = rankedLosses,
                WinStreak = 0, // TODO: Implement Win Streak Tracking
                InPlacementPhase = 0, // TODO: Implement Placement Match Tracking
                LevelsGainedThisSeason = account.User.TotalLevel
            },
            SimpleCasualSeasonStats = new SimpleSeasonStats
            {
                RankedMatchesWon = casualWins,
                RankedMatchesLost = casualLosses,
                WinStreak = 0, // TODO: Implement Win Streak Tracking
                InPlacementPhase = 0, // TODO: Implement Placement Match Tracking
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
            .Include(account => account.User)
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

            // TODO: Populate MasteryInfo From Mastery System Once Re-Implemented
            // TODO: Populate MasteryRewards From Mastery System Once Re-Implemented (Only For Own Account)

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

        MatchInformation? matchInformation = await DistributedCache.GetMatchInformation(matchStatistics.MatchID);

        // Fall Back To The Database Snapshot If The Redis Cache Entry Has Been Evicted
        if (matchInformation is null && matchStatistics.MatchInformationSnapshot is not null)
            matchInformation = JsonSerializer.Deserialize<MatchInformation>(matchStatistics.MatchInformationSnapshot);

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

            // TODO: Figure Out How To Select Which Statistics To Use (Public Match, Matchmaking, etc.)
            // INFO: Currently, This Code Logic Assumes A Public Match
            // INFO: Potential Logic + Switch/Case On Map Name: bool isPublic = form.player_stats.First().Value.First().Value.pub_count == 1;

            AccountStatistics currentMatchTypeStatistics = accountStatistics.Single(statistics => statistics.Type == AccountStatisticsType.Public);

            // TODO: Increment Current Match Type Statistics With Current Match Data

            AccountStatistics publicMatchStatistics = accountStatistics.Single(statistics => statistics.Type == AccountStatisticsType.Public);

            // TODO: Increment Public Match Statistics With Current Match Data

            AccountStatistics matchmakingStatistics = accountStatistics.Single(statistics => statistics.Type == AccountStatisticsType.Matchmaking);

            // TODO: Increment Matchmaking Statistics With Current Match Data

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

        MatchParticipantStatistics requestingPlayerStatistics = allPlayerStatistics.Single(statistics => statistics.AccountID == account.ID);

        MatchMastery matchMastery = new
        (
            heroIdentifier: requestingPlayerStatistics.HeroIdentifier,
            currentMasteryExperience: 0, // TODO: Retrieve From Mastery System Once Re-Implemented
            matchMasteryExperience: 100, // TODO: Calculate Based On Match Duration And Result (Use Calculation That I Implemented In Legacy PK)
            bonusExperience: 10 // TODO: Calculate Based On Max-Level Heroes Owned
        )
        {
            MasteryExperienceMaximumLevelHeroesCount = 0, // TODO: Count Heroes At Max Mastery Level (+ Enable MatchMastery Constructor Once Masteries Are Re-Implemented)
            MasteryExperienceBoostProductCount = 0, // TODO: Count "ma.Mastery Boost" Items (+ Enable MatchMastery Constructor Once Masteries Are Re-Implemented)
            MasteryExperienceSuperBoostProductCount = 0 // TODO: Count "ma.Super Mastery Boost" Items (+ Enable MatchMastery Constructor Once Masteries Are Re-Implemented)
        };

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
        Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> items = account.User.OwnedStoreItems
            .Where(item => item.StartsWith("ma.").Equals(false) && item.StartsWith("cp.").Equals(false))
            .ToDictionary<string, string, OneOf<StoreItemData, StoreItemDiscountCoupon>>(upgrade => upgrade, upgrade => new StoreItemData());

        // TODO: Add Mastery Boosts And Coupons

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
