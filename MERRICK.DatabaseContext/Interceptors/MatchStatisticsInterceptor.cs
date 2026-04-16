namespace MERRICK.DatabaseContext.Interceptors;

/// <summary>
///     Automatically updates <see cref="AccountStatistics"/> hero and award statistics when <see cref="MatchParticipantStatistics"/> are recorded.
/// </summary>
public sealed class MatchStatisticsInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAccountStatisticsFromMatches(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await UpdateAccountStatisticsFromMatchesAsync(eventData.Context, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAccountStatisticsFromMatches(DbContext? context)
    {
        if (context is null)
            return;

        List<MatchParticipantStatistics> newMatches = context.ChangeTracker
            .Entries<MatchParticipantStatistics>()
            .Where(entry => entry.State == EntityState.Added)
            .Select(entry => entry.Entity)
            .ToList();

        foreach (MatchParticipantStatistics match in newMatches)
        {
            MatchStatistics? matchStatistics = context.Set<MatchStatistics>()
                .SingleOrDefault(matchStats => matchStats.MatchID == match.MatchID);

            if (matchStatistics is null)
                continue;

            UpdateHeroStatistics(context, match, matchStatistics.Map);
            UpdateAwardStatistics(context, match, matchStatistics);
        }
    }

    private static async Task UpdateAccountStatisticsFromMatchesAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null)
            return;

        List<MatchParticipantStatistics> newMatches = context.ChangeTracker
            .Entries<MatchParticipantStatistics>()
            .Where(entry => entry.State == EntityState.Added)
            .Select(entry => entry.Entity)
            .ToList();

        foreach (MatchParticipantStatistics match in newMatches)
        {
            MatchStatistics? matchStatistics = await context.Set<MatchStatistics>()
                .SingleOrDefaultAsync(matchStats => matchStats.MatchID == match.MatchID, cancellationToken);

            if (matchStatistics is null)
                continue;

            await UpdateHeroStatisticsAsync(context, match, matchStatistics.Map, cancellationToken);
            await UpdateAwardStatisticsAsync(context, match, matchStatistics, cancellationToken);
        }
    }

    private static void UpdateHeroStatistics(DbContext context, MatchParticipantStatistics match, string map)
    {
        AccountStatisticsType statisticsType = DetermineStatisticsType(match, map);

        AccountStatistics? accountStatistics = context.Set<AccountStatistics>()
            .SingleOrDefault(statistics => statistics.AccountID == match.AccountID && statistics.Type == statisticsType);

        if (accountStatistics is null)
            return;

        HeroStats? heroStats = accountStatistics.HeroStatistics.Heroes
            .SingleOrDefault(hero => hero.HeroIdentifier == match.HeroIdentifier);

        if (heroStats is null)
        {
            heroStats = new HeroStats { HeroIdentifier = match.HeroIdentifier };
            accountStatistics.HeroStatistics.Heroes.Add(heroStats);
        }

        heroStats.GamesPlayed++;
        heroStats.Wins += match.Win;
        heroStats.Losses += match.Loss;
        heroStats.HeroKills += match.HeroKills;
        heroStats.HeroDeaths += match.HeroDeaths;
        heroStats.HeroAssists += match.HeroAssists;
        heroStats.TeamCreepKills += match.TeamCreepKills;
        heroStats.Denies += match.Denies;
        heroStats.Experience += match.Experience;
        heroStats.Gold += match.Gold;
        heroStats.Actions += match.Actions;
        heroStats.TimeEarningExperience += match.TimeEarningExperience;
    }

    private static async Task UpdateHeroStatisticsAsync(DbContext context, MatchParticipantStatistics match, string map, CancellationToken cancellationToken)
    {
        AccountStatisticsType statisticsType = DetermineStatisticsType(match, map);

        AccountStatistics? accountStatistics = await context.Set<AccountStatistics>()
            .SingleOrDefaultAsync(statistics => statistics.AccountID == match.AccountID && statistics.Type == statisticsType, cancellationToken);

        if (accountStatistics is null)
            return;

        HeroStats? heroStats = accountStatistics.HeroStatistics.Heroes
            .SingleOrDefault(hero => hero.HeroIdentifier == match.HeroIdentifier);

        if (heroStats is null)
        {
            heroStats = new HeroStats { HeroIdentifier = match.HeroIdentifier };
            accountStatistics.HeroStatistics.Heroes.Add(heroStats);
        }

        heroStats.GamesPlayed++;
        heroStats.Wins += match.Win;
        heroStats.Losses += match.Loss;
        heroStats.HeroKills += match.HeroKills;
        heroStats.HeroDeaths += match.HeroDeaths;
        heroStats.HeroAssists += match.HeroAssists;
        heroStats.TeamCreepKills += match.TeamCreepKills;
        heroStats.Denies += match.Denies;
        heroStats.Experience += match.Experience;
        heroStats.Gold += match.Gold;
        heroStats.Actions += match.Actions;
        heroStats.TimeEarningExperience += match.TimeEarningExperience;
    }

    private static void UpdateAwardStatistics(DbContext context, MatchParticipantStatistics match, MatchStatistics matchStatistics)
    {
        AccountStatisticsType statisticsType = DetermineStatisticsType(match, matchStatistics.Map);

        AccountStatistics? accountStatistics = context.Set<AccountStatistics>()
            .SingleOrDefault(statistics => statistics.AccountID == match.AccountID && statistics.Type == statisticsType);

        if (accountStatistics is null)
            return;

        if (matchStatistics.MVPAccountID == match.AccountID)
            accountStatistics.AwardStatistics.MVPAwards++;

        if (matchStatistics.AwardMostAnnihilations == match.AccountID)
            accountStatistics.AwardStatistics.AnnihilationAwards++;

        if (matchStatistics.AwardMostQuadKills == match.AccountID)
            accountStatistics.AwardStatistics.QuadKillAwards++;

        if (matchStatistics.AwardLargestKillStreak == match.AccountID)
            accountStatistics.AwardStatistics.LongestKillStreakAwards++;

        if (matchStatistics.AwardMostSmackdowns == match.AccountID)
            accountStatistics.AwardStatistics.SmackdownAwards++;

        if (matchStatistics.AwardMostKills == match.AccountID)
            accountStatistics.AwardStatistics.MostKillsAwards++;

        if (matchStatistics.AwardMostAssists == match.AccountID)
            accountStatistics.AwardStatistics.MostAssistsAwards++;

        if (matchStatistics.AwardLeastDeaths == match.AccountID)
            accountStatistics.AwardStatistics.LeastDeathsAwards++;

        if (matchStatistics.AwardMostBuildingDamage == match.AccountID)
            accountStatistics.AwardStatistics.MostBuildingDamageAwards++;

        if (matchStatistics.AwardMostWardsKilled == match.AccountID)
            accountStatistics.AwardStatistics.MostWardsDestroyedAwards++;

        if (matchStatistics.AwardMostHeroDamageDealt == match.AccountID)
            accountStatistics.AwardStatistics.MostHeroDamageDealtAwards++;

        if (matchStatistics.AwardHighestCreepScore == match.AccountID)
            accountStatistics.AwardStatistics.HighestCreepScoreAwards++;
    }

    private static async Task UpdateAwardStatisticsAsync(DbContext context, MatchParticipantStatistics match, MatchStatistics matchStatistics, CancellationToken cancellationToken)
    {
        AccountStatisticsType statisticsType = DetermineStatisticsType(match, matchStatistics.Map);

        AccountStatistics? accountStatistics = await context.Set<AccountStatistics>()
            .SingleOrDefaultAsync(statistics => statistics.AccountID == match.AccountID && statistics.Type == statisticsType, cancellationToken);

        if (accountStatistics is null)
            return;

        if (matchStatistics.MVPAccountID == match.AccountID)
            accountStatistics.AwardStatistics.MVPAwards++;

        if (matchStatistics.AwardMostAnnihilations == match.AccountID)
            accountStatistics.AwardStatistics.AnnihilationAwards++;

        if (matchStatistics.AwardMostQuadKills == match.AccountID)
            accountStatistics.AwardStatistics.QuadKillAwards++;

        if (matchStatistics.AwardLargestKillStreak == match.AccountID)
            accountStatistics.AwardStatistics.LongestKillStreakAwards++;

        if (matchStatistics.AwardMostSmackdowns == match.AccountID)
            accountStatistics.AwardStatistics.SmackdownAwards++;

        if (matchStatistics.AwardMostKills == match.AccountID)
            accountStatistics.AwardStatistics.MostKillsAwards++;

        if (matchStatistics.AwardMostAssists == match.AccountID)
            accountStatistics.AwardStatistics.MostAssistsAwards++;

        if (matchStatistics.AwardLeastDeaths == match.AccountID)
            accountStatistics.AwardStatistics.LeastDeathsAwards++;

        if (matchStatistics.AwardMostBuildingDamage == match.AccountID)
            accountStatistics.AwardStatistics.MostBuildingDamageAwards++;

        if (matchStatistics.AwardMostWardsKilled == match.AccountID)
            accountStatistics.AwardStatistics.MostWardsDestroyedAwards++;

        if (matchStatistics.AwardMostHeroDamageDealt == match.AccountID)
            accountStatistics.AwardStatistics.MostHeroDamageDealtAwards++;

        if (matchStatistics.AwardHighestCreepScore == match.AccountID)
            accountStatistics.AwardStatistics.HighestCreepScoreAwards++;
    }

    private static AccountStatisticsType DetermineStatisticsType(MatchParticipantStatistics match, string map)
    {
        // MidWars Has Dedicated Statistics Regardless Of Whether The Match Is Ranked Or Public
        if (map.Equals("midwars", StringComparison.OrdinalIgnoreCase))
            return AccountStatisticsType.MidWars;

        // RiftWars Has Dedicated Statistics Regardless Of Whether The Match Is Ranked Or Public
        if (map.Equals("riftwars", StringComparison.OrdinalIgnoreCase))
            return AccountStatisticsType.RiftWars;

        // Ranked Matches On "caldavar" Are Normal Matchmaking, While "caldavar_old" Is Casual Matchmaking
        if (match.RankedMatch is 1)
        {
            if (map.Equals("caldavar", StringComparison.OrdinalIgnoreCase))
                return AccountStatisticsType.Matchmaking;

            else if (map.Equals("caldavar_old", StringComparison.OrdinalIgnoreCase))
                return AccountStatisticsType.MatchmakingCasual;

            else throw new ArgumentOutOfRangeException(nameof(map), $@"Unexpected Map ""{map}"" For Ranked Match {match.MatchID}");
        }

        // Public And Unranked Matches (Including "caldavar_reborn") Are Classified As Public
        if (match.PublicMatch is 1)
            return AccountStatisticsType.Public;

        throw new InvalidOperationException($@"Unable To Determine Account Statistics Type For Match {match.MatchID} On Map ""{map}""");
    }
}
