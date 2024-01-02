namespace MERRICK.Database.Services;

internal class DatabaseInitializer(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger) : BackgroundService
{
    internal const string ActivitySourceName = "Migrations";

    private readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        MerrickContext context = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        await InitializeDatabaseAsync(context, cancellationToken);
    }

    private async Task InitializeDatabaseAsync(MerrickContext context, CancellationToken cancellationToken)
    {
        using Activity? activity = _activitySource.StartActivity("Initializing MERRICK Database", ActivityKind.Client);

        Stopwatch stopwatch = Stopwatch.StartNew();

        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(context.Database.MigrateAsync, cancellationToken);

        await SeedAsync(context, cancellationToken);

        logger.LogInformation("Database Initialization Completed After {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
    }

    private async Task SeedAsync(MerrickContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Seeding Database");
    }
}
