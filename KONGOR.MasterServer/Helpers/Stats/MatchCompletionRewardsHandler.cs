namespace KONGOR.MasterServer.Helpers.Stats;

/// <summary>
///     Applies the match-completion side-effects for a single participant.
///     Currency rewards come from <see cref="EconomyConfiguration.MatchRewards"/>.
///     An additive bonus from <see cref="EconomyConfiguration.EventRewards"/> is also paid out while the player is still within the first <see cref="PostSignupBonus.MatchesCount"/> matches.
///     The corresponding <see cref="AccountStatistics"/> row for the match type is the single sink for every accumulated statistic. The aggregate counters, the per-hero summary, the award counts, and the skill rating adjusted by the rating change submitted by the match server.
/// </summary>
public static class MatchCompletionRewardsHandler
{
    /// <summary>
    ///     The floor below which a skill rating cannot drop.
    ///     Mirrors the minimum TMR protected by the chat server's pre-calculated loss values, catching the adjustments the chat server cannot pre-empt (for example the match server doubling the loss value for leavers).
    /// </summary>
    private const double MinimumSkillRating = 1000.0;

    /// <summary>
    ///     Applies match rewards, the post-signup bonus (if applicable), the aggregate and per-hero counters, the award counts, and the skill rating change for the given participant.
    ///     All of these accumulate into the single <see cref="AccountStatistics"/> row resolved for the match's game mode, so they are never split across game modes.
    ///     The caller is responsible for calling <see cref="MerrickContext.SaveChangesAsync"/> after all participants have been processed.
    /// </summary>
    public static async Task Apply(MerrickContext databaseContext, ILogger logger, Account account, MatchInformation? matchInformation, MatchStatistics matchStatistics, MatchParticipantStatistics matchParticipantStatistics)
    {
        MatchRewards matchRewards = JSONConfiguration.EconomyConfiguration.MatchRewards;

        (Win winPartition, Loss lossPartition) = SelectGroupPartitions(matchRewards, matchParticipantStatistics, logger);

        bool isWin = matchParticipantStatistics.Win is 1;

        int rewardGoldCoins     = isWin ? winPartition.GoldCoins     : lossPartition.GoldCoins;
        int rewardSilverCoins   = isWin ? winPartition.SilverCoins   : lossPartition.SilverCoins;
        int rewardPlinkoTickets = isWin ? winPartition.PlinkoTickets : lossPartition.PlinkoTickets;

        account.User.GoldCoins     += rewardGoldCoins;
        account.User.SilverCoins   += rewardSilverCoins;
        account.User.PlinkoTickets += rewardPlinkoTickets;

        if (account.IsMain)
        {
            int matchesPlayedBeforeThis = await databaseContext.MatchParticipantStatistics
                .CountAsync(stats => stats.AccountID == account.ID);

            PostSignupBonus postSignupBonus = JSONConfiguration.EconomyConfiguration.EventRewards.PostSignupBonus;

            if (matchesPlayedBeforeThis < postSignupBonus.MatchesCount)
            {
                account.User.GoldCoins     += postSignupBonus.GoldCoins;
                account.User.SilverCoins   += postSignupBonus.SilverCoins;
                account.User.PlinkoTickets += postSignupBonus.PlinkoTickets;
            }
        }

        AccountStatisticsType statisticsType = ResolveStatisticsType(matchInformation, matchStatistics, matchParticipantStatistics);

        AccountStatistics? statistics = await databaseContext.AccountStatistics
            .SingleOrDefaultAsync(record => record.AccountID == account.ID && record.Type == statisticsType);

        if (statistics is null)
        {
            // The "AccountStatisticsInterceptor" Seeds A Row For Every Type When An Account Is Created, So Absence Indicates Data Corruption
            logger.LogError($@"[BUG] AccountStatistics Row For Account ID {account.ID} And Type ""{statisticsType}"" Was Not Found");

            return;
        }

        statistics.MatchesPlayed++;

        if (matchParticipantStatistics.Win          is 1) statistics.MatchesWon++;
        if (matchParticipantStatistics.Loss         is 1) statistics.MatchesLost++;
        if (matchParticipantStatistics.Disconnected is 1) statistics.MatchesDisconnected++;
        if (matchParticipantStatistics.Conceded     is 1) statistics.MatchesConceded++;
        if (matchParticipantStatistics.Kicked       is 1) statistics.MatchesKicked++;

        statistics.HeroKills   += matchParticipantStatistics.HeroKills;
        statistics.HeroAssists += matchParticipantStatistics.HeroAssists;
        statistics.HeroDeaths  += matchParticipantStatistics.HeroDeaths;
        statistics.WardsPlaced += matchParticipantStatistics.WardsPlaced;
        statistics.Smackdowns  += matchParticipantStatistics.Smackdown;

        AccumulateHeroStatistics(statistics, matchParticipantStatistics);

        await AccumulateMasteryExperience(databaseContext, logger, account, statisticsType, matchParticipantStatistics);

        AccumulateAwardStatistics(statistics, matchStatistics, matchParticipantStatistics);

        RecordPlacementMatchResult(statistics, matchParticipantStatistics);

        ApplySkillRatingChange(statistics, matchParticipantStatistics);
    }

    /// <summary>
    ///     Accumulates a single match's participant statistics into the running per-hero summary totals on the resolved statistics row, creating the per-hero entry on first use.
    /// </summary>
    private static void AccumulateHeroStatistics(AccountStatistics statistics, MatchParticipantStatistics match)
    {
        HeroStats? heroStats = statistics.HeroStatistics.Heroes
            .SingleOrDefault(hero => hero.HeroIdentifier == match.HeroIdentifier);

        if (heroStats is null)
        {
            heroStats = new HeroStats { HeroIdentifier = match.HeroIdentifier };

            statistics.HeroStatistics.Heroes.Add(heroStats);
        }

        heroStats.GamesPlayed++;
        heroStats.Wins += match.Win;
        heroStats.Losses += match.Loss;
        heroStats.Concedes += match.Conceded;
        heroStats.ConcedeVotes += match.ConcedeVotes;
        heroStats.Buybacks += match.Buybacks;
        heroStats.Disconnections += match.Disconnected;
        heroStats.Kicks += match.Kicked;
        heroStats.ScoreTotal += match.Score;
        heroStats.HeroKills += match.HeroKills;
        heroStats.HeroDamage += match.HeroDamage;
        heroStats.HeroExperience += match.HeroExperience;
        heroStats.GoldFromHeroKills += match.GoldFromHeroKills;
        heroStats.HeroAssists += match.HeroAssists;
        heroStats.HeroDeaths += match.HeroDeaths;
        heroStats.GoldLostToDeath += match.GoldLostToDeath;
        heroStats.SecondsDead += match.SecondsDead;
        heroStats.TeamCreepKills += match.TeamCreepKills;
        heroStats.TeamCreepDamage += match.TeamCreepDamage;
        heroStats.TeamCreepExperience += match.TeamCreepExperience;
        heroStats.TeamCreepGold += match.TeamCreepGold;
        heroStats.NeutralCreepKills += match.NeutralCreepKills;
        heroStats.NeutralCreepDamage += match.NeutralCreepDamage;
        heroStats.NeutralCreepExperience += match.NeutralCreepExperience;
        heroStats.NeutralCreepGold += match.NeutralCreepGold;
        heroStats.BuildingDamage += match.BuildingDamage;
        heroStats.ExperienceFromBuildings += match.ExperienceFromBuildings;
        heroStats.BuildingsRazed += match.BuildingsRazed;
        heroStats.GoldFromBuildings += match.GoldFromBuildings;
        heroStats.Denies += match.Denies;
        heroStats.ExperienceDenied += match.ExperienceDenied;
        heroStats.Gold += match.Gold;
        heroStats.GoldSpent += match.GoldSpent;
        heroStats.Experience += match.Experience;
        heroStats.Actions += match.Actions;
        heroStats.SecondsPlayed += match.SecondsPlayed;
        heroStats.ConsumablesPurchased += match.ConsumablesPurchased;
        heroStats.WardsPlaced += match.WardsPlaced;
        heroStats.TimeEarningExperience += match.TimeEarningExperience;
        heroStats.FirstBloods += match.FirstBlood;
        heroStats.DoubleKills += match.DoubleKill;
        heroStats.TripleKills += match.TripleKill;
        heroStats.QuadKills += match.QuadKill;
        heroStats.Annihilations += match.Annihilation;
        heroStats.KillStreak03 += match.KillStreak03;
        heroStats.KillStreak04 += match.KillStreak04;
        heroStats.KillStreak05 += match.KillStreak05;
        heroStats.KillStreak06 += match.KillStreak06;
        heroStats.KillStreak07 += match.KillStreak07;
        heroStats.KillStreak08 += match.KillStreak08;
        heroStats.KillStreak09 += match.KillStreak09;
        heroStats.KillStreak10 += match.KillStreak10;
        heroStats.KillStreak15 += match.KillStreak15;
        heroStats.Smackdowns += match.Smackdown;
        heroStats.Humiliations += match.Humiliation;
        heroStats.Nemeses += match.Nemesis;
        heroStats.Retributions += match.Retribution;
    }

    /// <summary>
    ///     Accumulates the base and bonus mastery experience for the participant's hero into the account's mastery row, creating the row on first use, and issues the per-hero mastery level reward when the hero crosses a level.
    ///     Only the eligible game modes (ranked normal matchmaking, ranked casual matchmaking, and MidWars) award mastery experience. For any other mode the base experience is zero and no accrual occurs.
    /// </summary>
    private static async Task AccumulateMasteryExperience(MerrickContext databaseContext, ILogger logger, Account account, AccountStatisticsType statisticsType, MatchParticipantStatistics matchParticipantStatistics)
    {
        Mastery? mastery = await databaseContext.Masteries
            .SingleOrDefaultAsync(record => record.AccountID == account.ID);

        if (mastery is null)
        {
            mastery = new Mastery { Account = account };

            databaseContext.Masteries.Add(mastery);
        }

        int matchExperience = mastery.CalculateMatchExperience(statisticsType, matchParticipantStatistics.HeroLevel);

        // A Zero Match Experience Means The Game Mode Is Not Eligible For Mastery Progression
        if (matchExperience is 0)
            return;

        int bonusExperience = mastery.CalculateBonusExperience(statisticsType, Heroes.TotalHeroCount);

        string heroIdentifier = matchParticipantStatistics.HeroIdentifier;

        int currentExperience = mastery.GetHeroExperienceByHeroIdentifier(heroIdentifier);

        mastery.SetHeroExperienceByHeroIdentifier(heroIdentifier, currentExperience + matchExperience + bonusExperience);

        int previousLevel = Mastery.GetLevelFromExperience(currentExperience);
        int currentLevel = Mastery.GetLevelFromExperience(currentExperience + matchExperience + bonusExperience);

        // A Single Match Can Never Award Enough Experience To Cross More Than One Mastery Level
        if (currentLevel == previousLevel + 1)
            MasteryConsumables.IssueHeroMasteryLevelReward(account.User, currentLevel, heroIdentifier, logger);
    }

    /// <summary>
    ///     Increments the award counts on the resolved statistics row for each per-match award the participant won.
    /// </summary>
    private static void AccumulateAwardStatistics(AccountStatistics statistics, MatchStatistics matchStatistics, MatchParticipantStatistics match)
    {
        if (matchStatistics.MVPAccountID == match.AccountID)
            statistics.AwardStatistics.MVPAwards++;

        if (matchStatistics.AwardMostAnnihilations == match.AccountID)
            statistics.AwardStatistics.AnnihilationAwards++;

        if (matchStatistics.AwardMostQuadKills == match.AccountID)
            statistics.AwardStatistics.QuadKillAwards++;

        if (matchStatistics.AwardLargestKillStreak == match.AccountID)
            statistics.AwardStatistics.LongestKillStreakAwards++;

        if (matchStatistics.AwardMostSmackdowns == match.AccountID)
            statistics.AwardStatistics.SmackdownAwards++;

        if (matchStatistics.AwardMostKills == match.AccountID)
            statistics.AwardStatistics.MostKillsAwards++;

        if (matchStatistics.AwardMostAssists == match.AccountID)
            statistics.AwardStatistics.MostAssistsAwards++;

        if (matchStatistics.AwardLeastDeaths == match.AccountID)
            statistics.AwardStatistics.LeastDeathsAwards++;

        if (matchStatistics.AwardMostBuildingDamage == match.AccountID)
            statistics.AwardStatistics.MostBuildingDamageAwards++;

        if (matchStatistics.AwardMostWardsKilled == match.AccountID)
            statistics.AwardStatistics.MostWardsDestroyedAwards++;

        if (matchStatistics.AwardMostHeroDamageDealt == match.AccountID)
            statistics.AwardStatistics.MostHeroDamageDealtAwards++;

        if (matchStatistics.AwardHighestCreepScore == match.AccountID)
            statistics.AwardStatistics.HighestCreepScoreAwards++;
    }

    /// <summary>
    ///     Records the participant's win or loss in the statistics row's placement data while the placement phase is incomplete.
    ///     Placement data is only maintained for queues that track placements (the row's data is <see langword="null"/> otherwise), and a disconnected participant does not consume a placement match.
    /// </summary>
    private static void RecordPlacementMatchResult(AccountStatistics statistics, MatchParticipantStatistics matchParticipantStatistics)
    {
        if (statistics.PlacementMatchesData is null || statistics.PlacementMatchesData.Length >= AccountStatistics.ExpectedPlacementMatchCount)
            return;

        if (matchParticipantStatistics.Disconnected is 1)
            return;

        statistics.PlacementMatchesData += matchParticipantStatistics.Win is 1 ? "1" : "0";
    }

    /// <summary>
    ///     Applies the rating change submitted by the match server to the resolved statistics row.
    ///     The match server submits a ranked rating change for arranged matches and a public one for public matches, and the change is only applied when it targets the same rating family as the resolved row, so that a fallback resolution (for example a resubmission without match information) cannot mutate an unrelated rating.
    ///     A rating loss never takes the rating below <see cref="MinimumSkillRating"/>.
    /// </summary>
    private static void ApplySkillRatingChange(AccountStatistics statistics, MatchParticipantStatistics matchParticipantStatistics)
    {
        double ratingChange;

        if (statistics.Type is AccountStatisticsType.Public)
            ratingChange = matchParticipantStatistics.PublicMatch is 1 ? matchParticipantStatistics.PublicSkillRatingChange : 0.0;

        else
            ratingChange = matchParticipantStatistics.RankedMatch is 1 ? matchParticipantStatistics.RankedSkillRatingChange : 0.0;

        if (ratingChange is 0.0)
            return;

        statistics.SkillRating = Math.Max(statistics.SkillRating + ratingChange, MinimumSkillRating);
    }

    /// <summary>
    ///     Resolves the single <see cref="AccountStatisticsType"/> that every statistic for this match should accumulate into.
    ///     The cached <see cref="MatchInformation"/> is the authoritative signal when present.
    ///     During a resubmission (after the distributed cache entry has been purged) it is unavailable, so the type is derived from the submitted match details instead.
    /// </summary>
    private static AccountStatisticsType ResolveStatisticsType(MatchInformation? matchInformation, MatchStatistics matchStatistics, MatchParticipantStatistics matchParticipantStatistics)
    {
        if (matchInformation is not null)
            return ResolveAccountStatisticsType(matchInformation);

        return ResolveStatisticsTypeFromSubmission(matchStatistics.Map, matchParticipantStatistics);
    }

    /// <summary>
    ///     Derives the <see cref="AccountStatisticsType"/> from the submitted match details when no <see cref="MatchInformation"/> snapshot is available.
    ///     Mirrors the original API's classification (the public/ranked flags plus the map name) and never throws, defaulting to <see cref="AccountStatisticsType.Public"/>.
    ///     The cooperative and reborn distinctions live only in the <see cref="MatchInformation.MatchType"/> enumeration, so a resubmission of one of those collapses to its closest flag-based equivalent.
    /// </summary>
    private static AccountStatisticsType ResolveStatisticsTypeFromSubmission(string map, MatchParticipantStatistics matchParticipantStatistics)
    {
        if (map.Equals("midwars", StringComparison.OrdinalIgnoreCase))
            return AccountStatisticsType.MidWars;

        if (map.Equals("riftwars", StringComparison.OrdinalIgnoreCase))
            return AccountStatisticsType.RiftWars;

        if (matchParticipantStatistics.RankedMatch is 1)
        {
            // Ranked Matches On "caldavar" Are Normal Matchmaking, While "caldavar_old" Is Casual Matchmaking
            if (map.Equals("caldavar", StringComparison.OrdinalIgnoreCase))
                return AccountStatisticsType.Matchmaking;

            if (map.Equals("caldavar_old", StringComparison.OrdinalIgnoreCase))
                return AccountStatisticsType.MatchmakingCasual;

            // Any Other Ranked Map Falls Back To Normal Matchmaking Rather Than Throwing
            return AccountStatisticsType.Matchmaking;
        }

        return AccountStatisticsType.Public;
    }

    /// <summary>
    ///     Resolves the <see cref="AccountStatisticsType"/> from the cached <see cref="MatchInformation"/>, used both during stat accumulation and when reading back the statistics row for the match summary.
    ///     Falls back to <see cref="AccountStatisticsType.Public"/> when no match information is available.
    /// </summary>
    public static AccountStatisticsType ResolveAccountStatisticsType(MatchInformation? matchInformation)
    {
        if (matchInformation is null)
            return AccountStatisticsType.Public;

        // Reborn Matches Share The MIDWARS Arranged Match Type Purely For The Match Server Binary's Benefit, So The Map Name Distinguishes Actual MidWars Matches
        if (matchInformation.MatchType is MatchType.AM_MATCHMAKING_MIDWARS && matchInformation.Map.Equals("midwars", StringComparison.OrdinalIgnoreCase) is false)
            return matchInformation.IsCasual ? AccountStatisticsType.MatchmakingCasual : AccountStatisticsType.Matchmaking;

        return matchInformation.MatchType switch
        {
            MatchType.AM_MATCHMAKING_BOTMATCH => AccountStatisticsType.Cooperative,
            MatchType.AM_MATCHMAKING_MIDWARS  => AccountStatisticsType.MidWars,
            MatchType.AM_MATCHMAKING_RIFTWARS => AccountStatisticsType.RiftWars,

            MatchType.AM_MATCHMAKING
         or MatchType.AM_MATCHMAKING_CAMPAIGN
         or MatchType.AM_MATCHMAKING_CUSTOM   => matchInformation.IsCasual ? AccountStatisticsType.MatchmakingCasual : AccountStatisticsType.Matchmaking,

            MatchType.AM_UNRANKED_MATCHMAKING => AccountStatisticsType.MatchmakingCasual,

            _ => AccountStatisticsType.Public
        };
    }

    private static (Win Win, Loss Loss) SelectGroupPartitions(MatchRewards matchRewards, MatchParticipantStatistics matchParticipantStatistics, ILogger logger)
    {
        return matchParticipantStatistics.GroupNumber switch
        {
            // A Group Number Of "-1" Is The Match Server's Sentinel For A Participant With No Arranged-Match Roster (A Public Match Participant), Which Is Correctly Rewarded As Solo
            -1 => (matchRewards.Solo.Win,             matchRewards.Solo.Loss),

            // Group Numbers Are 1-5 For Arranged Matches, Corresponding To The Number Of Participants On The Player's Roster (e.g. "2" For A Duo, "3" For A Trio, etc.)
            +1 => (matchRewards.Solo.Win,             matchRewards.Solo.Loss),
            +2 => (matchRewards.TwoPersonGroup.Win,   matchRewards.TwoPersonGroup.Loss),
            +3 => (matchRewards.ThreePersonGroup.Win, matchRewards.ThreePersonGroup.Loss),
            +4 => (matchRewards.FourPersonGroup.Win,  matchRewards.FourPersonGroup.Loss),
            +5 => (matchRewards.FivePersonGroup.Win,  matchRewards.FivePersonGroup.Loss),

            // Any Other Group Number Is Unexpected, So Log A Warning And Fall Back To Solo Rewards (Rather Than Throwing An Exception And Risk Leaving The Player Without Rewards)
            _ => LogAndFallBackToSolo(matchRewards,  matchParticipantStatistics, logger)
        };
    }

    private static (Win Win, Loss Loss) LogAndFallBackToSolo(MatchRewards matchRewards, MatchParticipantStatistics matchParticipantStatistics, ILogger logger)
    {
        logger.LogWarning(@"Unexpected Group Number ""{GroupNumber}"" Encountered For Match Participant With Account ID ""{AccountID}"" (Match ID ""{MatchID}""); Falling Back To Solo Rewards",
            matchParticipantStatistics.GroupNumber, matchParticipantStatistics.AccountID, matchParticipantStatistics.MatchID);

        return (matchRewards.Solo.Win, matchRewards.Solo.Loss);
    }
}
