namespace MERRICK.DatabaseContext.Interceptors;

/// <summary>
///     Automatically creates <see cref="AccountStatistics"/> records for each <see cref="AccountStatisticsType"/> when a new <see cref="Account"/> is created.
/// </summary>
public sealed class AccountStatisticsInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CreateAccountStatisticsForNewAccounts(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        CreateAccountStatisticsForNewAccounts(eventData.Context);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void CreateAccountStatisticsForNewAccounts(DbContext? context)
    {
        if (context is null)
            return;

        IEnumerable<Account> newAccounts = context.ChangeTracker
            .Entries<Account>()
            .Where(entry => entry.State == EntityState.Added)
            .Select(entry => entry.Entity);

        foreach (Account account in newAccounts)
        {
            foreach (AccountStatisticsType type in Enum.GetValues<AccountStatisticsType>())
            {
                context.Add(new AccountStatistics
                {
                    AccountID = account.ID,
                    Type = type,
                    PlacementMatchesData = type is AccountStatisticsType.Matchmaking or AccountStatisticsType.MatchmakingCasual ? string.Empty : null
                });
            }
        }
    }
}
