namespace KONGOR.MasterServer.Helpers.Stats;

/// <summary>
///     Applies the match-completion side-effects for a single participant.
///     Currency rewards come from <see cref="EconomyConfiguration.MatchRewards"/>.
///     An additive bonus from <see cref="EconomyConfiguration.EventRewards"/> is also paid out while the player is still within the first <see cref="PostSignupBonus.MatchesCount"/> matches.
///     The corresponding <see cref="AccountStatistics"/> row is incremented for the match type.
/// </summary>
public static class MatchCompletionRewardsHandler
{
    /// <summary>
    ///     Applies match rewards, the post-signup bonus (if applicable), and match-counter increments for the given participant.
    ///     The caller is responsible for calling <see cref="MerrickContext.SaveChangesAsync"/> after all participants have been processed.
    /// </summary>
    public static async Task Apply(MerrickContext databaseContext, ILogger logger, Account account, MatchInformation? matchInformation, MatchParticipantStatistics matchParticipantStatistics)
    {
        MatchRewards matchRewards = JSONConfiguration.EconomyConfiguration.MatchRewards;

        (Win winBucket, Loss lossBucket) = SelectGroupBuckets(matchRewards, matchParticipantStatistics.GroupNumber, logger);

        bool isWin = matchParticipantStatistics.Win is 1;

        int rewardGoldCoins     = isWin ? winBucket.GoldCoins     : lossBucket.GoldCoins;
        int rewardSilverCoins   = isWin ? winBucket.SilverCoins   : lossBucket.SilverCoins;
        int rewardPlinkoTickets = isWin ? winBucket.PlinkoTickets : lossBucket.PlinkoTickets;

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

        AccountStatisticsType statisticsType = ResolveAccountStatisticsType(matchInformation);

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
    }

    /// <summary>
    ///     Resolves the <see cref="AccountStatisticsType"/> that the match-count increment should target, based on the cached <see cref="MatchInformation"/>.
    ///     Falls back to <see cref="AccountStatisticsType.Public"/> when no match information is available (e.g. during a resubmission after the Redis entry has been purged).
    /// </summary>
    public static AccountStatisticsType ResolveAccountStatisticsType(MatchInformation? matchInformation)
    {
        if (matchInformation is null)
            return AccountStatisticsType.Public;

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

    private static (Win Win, Loss Loss) SelectGroupBuckets(MatchRewards matchRewards, int groupNumber, ILogger logger)
    {
        return groupNumber switch
        {
            1 => (matchRewards.Solo.Win,             matchRewards.Solo.Loss),
            2 => (matchRewards.TwoPersonGroup.Win,   matchRewards.TwoPersonGroup.Loss),
            3 => (matchRewards.ThreePersonGroup.Win, matchRewards.ThreePersonGroup.Loss),
            4 => (matchRewards.FourPersonGroup.Win,  matchRewards.FourPersonGroup.Loss),
            5 => (matchRewards.FivePersonGroup.Win,  matchRewards.FivePersonGroup.Loss),
            _ => LogAndFallBackToSolo(matchRewards, groupNumber, logger)
        };
    }

    private static (Win Win, Loss Loss) LogAndFallBackToSolo(MatchRewards matchRewards, int groupNumber, ILogger logger)
    {
        logger.LogWarning(@"Unexpected GroupNumber ""{GroupNumber}"" On Match Participant; Falling Back To Solo Rewards", groupNumber);

        return (matchRewards.Solo.Win, matchRewards.Solo.Loss);
    }
}
