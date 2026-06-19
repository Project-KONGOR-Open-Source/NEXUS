namespace KONGOR.MasterServer.Services;

/// <summary>
///     Aggregates global per-hero win and loss totals across every account's recorded statistics, used to build the hero usage list.
///     Because computing the aggregate scans the stored statistics of every account, the result is cached in the distributed cache for a short period so that it is shared across all instances and is not recomputed on every request.
/// </summary>
public class HeroUsageStatisticsService(MerrickContext databaseContext, IDatabase distributedCache)
{
    private const string CacheKey = "HeroUsageStatistics";

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private MerrickContext MerrickContext { get; } = databaseContext;

    private IDatabase DistributedCache { get; } = distributedCache;

    /// <summary>
    ///     Returns the global per-hero win and loss totals, aggregated across every account's recorded statistics.
    ///     The result is served from the distributed cache when available, and otherwise computed, cached, and returned.
    /// </summary>
    public async Task<IReadOnlyList<HeroUsageStatistic>> GetHeroUsageStatistics()
    {
        RedisValue cachedStatistics = await DistributedCache.StringGetAsync(CacheKey);

        if (cachedStatistics.HasValue)
            return JsonSerializer.Deserialize<List<HeroUsageStatistic>>(cachedStatistics.ToString()) ?? [];

        List<AccountStatistics> allAccountStatistics = await MerrickContext.AccountStatistics
            .AsNoTracking()
            .ToListAsync();

        Dictionary<string, (int Wins, int Losses)> aggregatedByHero = new (StringComparer.OrdinalIgnoreCase);

        foreach (AccountStatistics accountStatistics in allAccountStatistics)
        {
            foreach (HeroStats heroStatistics in accountStatistics.HeroStatistics.Heroes)
            {
                aggregatedByHero.TryGetValue(heroStatistics.HeroIdentifier, out (int Wins, int Losses) totals);

                aggregatedByHero[heroStatistics.HeroIdentifier] = (totals.Wins + heroStatistics.Wins, totals.Losses + heroStatistics.Losses);
            }
        }

        List<HeroUsageStatistic> statistics =
            [.. aggregatedByHero.Select(entry => new HeroUsageStatistic(entry.Key, entry.Value.Wins, entry.Value.Losses))];

        await DistributedCache.StringSetAsync(CacheKey, JsonSerializer.Serialize(statistics), CacheDuration);

        return statistics;
    }
}

/// <summary>
///     The aggregated win and loss totals for a single hero across all accounts.
/// </summary>
public record HeroUsageStatistic(string HeroIdentifier, int Wins, int Losses);
