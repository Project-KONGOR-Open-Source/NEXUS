using KONGOR.MasterServer.Extensions.Cache;

namespace KONGOR.MasterServer.Services;

/// <summary>
///     A hosted service that initializes the Match ID counter in Redis at application startup.
///     This ensures the counter is synchronized with the maximum existing Match ID in the database,
///     preserving uniqueness and continuity.
/// </summary>
public class MatchIdInitializerService(IServiceProvider serviceProvider, ILogger<MatchIdInitializerService> logger)
    : IHostedService
{
    private IServiceProvider ServiceProvider { get; } = serviceProvider;
    private ILogger Logger { get; } = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Initializing Match ID Counter...");

        using IServiceScope scope = ServiceProvider.CreateScope();
        try
        {
            MerrickContext merrickContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
            IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

            await distributedCache.InitializeMatchIdCounter(merrickContext);

            Logger.LogInformation("Match ID Counter Initialized Successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Failed to initialize Match ID Counter. The application may generate duplicate Match IDs.");
            throw; // Fail startup if this critical initialization fails
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
