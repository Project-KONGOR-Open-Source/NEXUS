namespace MERRICK.Database.Services;

public class DatabaseInitializer(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";

    private readonly ActivitySource _activitySource = new (ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        MerrickContext context = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        await InitializeDatabaseAsync(context, cancellationToken);
    }

    private async Task InitializeDatabaseAsync(MerrickContext context, CancellationToken cancellationToken)
    {
        const string activityName = "Initializing MERRICK Database";

        using Activity? activity = _activitySource.StartActivity(activityName, ActivityKind.Client);

        Stopwatch stopwatch = Stopwatch.StartNew();

        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(context.Database.MigrateAsync, cancellationToken);

        await SeedAsync(context, cancellationToken);

        logger.LogInformation("Database Initialization Completed After {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
    }

    private async Task SeedAsync(MerrickContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Seeding Database");

        await context.Database.EnsureCreatedAsync(cancellationToken);

        await SeedDataHandlers.SeedUsers(context, cancellationToken, logger);
        await SeedDataHandlers.SeedClans(context, cancellationToken, logger);
        await SeedDataHandlers.SeedAccounts(context, cancellationToken, logger);
        await SeedDataHandlers.SeedHeroGuides(context, cancellationToken, logger);
    }
}
