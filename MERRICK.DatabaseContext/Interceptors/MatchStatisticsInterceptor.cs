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
            UpdateHeroStatistics(context, match);
            UpdateAwardStatistics(context, match);
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
            await UpdateHeroStatisticsAsync(context, match, cancellationToken);
            await UpdateAwardStatisticsAsync(context, match, cancellationToken);
        }
    }

    private static void UpdateHeroStatistics(DbContext context, MatchParticipantStatistics match)
    {
        AccountStatisticsType statisticsType = DetermineStatisticsType(match);

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

        // Mark The JSON Property As Modified To Ensure EF Core Persists The Changes
        context.Entry(accountStatistics).Property(statistics => statistics.HeroStatistics).IsModified = true;
    }

    private static async Task UpdateHeroStatisticsAsync(DbContext context, MatchParticipantStatistics match, CancellationToken cancellationToken)
    {
        AccountStatisticsType statisticsType = DetermineStatisticsType(match);

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

        // Mark The JSON Property As Modified To Ensure EF Core Persists The Changes
        context.Entry(accountStatistics).Property(statistics => statistics.HeroStatistics).IsModified = true;
    }

    private static void UpdateAwardStatistics(DbContext context, MatchParticipantStatistics match)
    {
        AccountStatisticsType statisticsType = DetermineStatisticsType(match);

        AccountStatistics? accountStatistics = context.Set<AccountStatistics>()
            .SingleOrDefault(statistics => statistics.AccountID == match.AccountID && statistics.Type == statisticsType);

        if (accountStatistics is null)
            return;

        MatchStatistics? matchStatistics = context.Set<MatchStatistics>()
            .SingleOrDefault(matchStats => matchStats.MatchID == match.MatchID);

        if (matchStatistics is null)
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

        // Mark The JSON Property As Modified To Ensure EF Core Persists The Changes
        context.Entry(accountStatistics).Property(statistics => statistics.AwardStatistics).IsModified = true;
    }

    private static async Task UpdateAwardStatisticsAsync(DbContext context, MatchParticipantStatistics match, CancellationToken cancellationToken)
    {
        AccountStatisticsType statisticsType = DetermineStatisticsType(match);

        AccountStatistics? accountStatistics = await context.Set<AccountStatistics>()
            .SingleOrDefaultAsync(statistics => statistics.AccountID == match.AccountID && statistics.Type == statisticsType, cancellationToken);

        if (accountStatistics is null)
            return;

        MatchStatistics? matchStatistics = await context.Set<MatchStatistics>()
            .SingleOrDefaultAsync(matchStats => matchStats.MatchID == match.MatchID, cancellationToken);

        if (matchStatistics is null)
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

        // Mark The JSON Property As Modified To Ensure EF Core Persists The Changes
        context.Entry(accountStatistics).Property(statistics => statistics.AwardStatistics).IsModified = true;
    }

    private static AccountStatisticsType DetermineStatisticsType(MatchParticipantStatistics match)
    {
        // If It Has Ranked Match Flag, It's Normal Matchmaking
        if (match.RankedMatch is 1)
        {
            return AccountStatisticsType.Matchmaking;
        }

        // If Not Ranked But Has Ranked Skill Rating Change, It's Casual Matchmaking
        // if (match.RankedMatch is not 1 && match.RankedSkillRatingChange is not 0)
        // {
        //     return AccountStatisticsType.MatchmakingCasual;
        // }
        // TODO: Confirm That RankedSkillRatingChange Is Only Non-Zero For Casual Matchmaking Matches And Not For Other Modes Like MidWars And RiftWars

        // If It Has Public Match Flag, It's Public
        if (match.PublicMatch is 1)
        {
            return AccountStatisticsType.Public;
        }

        // TODO: Add More Specific Logic For MidWars And RiftWars When We Get More Data On Those Modes, Probably Based On Map Identifier Or Something Similar

        throw new InvalidOperationException("Unable To Determine Account Statistics Type");
    }
}
