namespace MERRICK.DatabaseContext.Interceptors;

/// <summary>
///     Seeds the system messages (see <see cref="SystemMessages.All"/>) into the inbox of each new <see cref="Account"/> when it is created.
///     Runs exactly once per account, at creation.
/// </summary>
public sealed class SystemMessageInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SeedSystemMessagesForNewAccounts(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SeedSystemMessagesForNewAccounts(eventData.Context);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void SeedSystemMessagesForNewAccounts(DbContext? context)
    {
        if (context is null)
            return;

        List<Account> newAccounts = context.ChangeTracker
            .Entries<Account>()
            .Where(entry => entry.State == EntityState.Added)
            .Select(entry => entry.Entity)
            .ToList();

        foreach (Account account in newAccounts)
            foreach (SystemMessageDefinition definition in SystemMessages.All)
                context.Add(definition.ToMessage(account));
    }
}
