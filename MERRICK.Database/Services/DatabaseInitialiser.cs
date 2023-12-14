namespace MERRICK.Database.Services;

internal class DatabaseInitialiser(IServiceProvider serviceProvider, ILogger<DatabaseInitialiser> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";

    private readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        MerrickContext context = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        await InitializeDatabaseAsync(context, cancellationToken);
    }

    private async Task InitializeDatabaseAsync(MerrickContext context, CancellationToken cancellationToken)
    {
        using Activity? activity = _activitySource.StartActivity("Initialising MERRICK Database", ActivityKind.Client);

        Stopwatch stopwatch = Stopwatch.StartNew();

        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(context.Database.MigrateAsync /* Not Supported By NativeAOT */, cancellationToken);

        await SeedAsync(context, cancellationToken);

        logger.LogInformation("Database Initialisation Completed After {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
    }

    private async Task SeedAsync(MerrickContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Seeding Database");
    }
}
