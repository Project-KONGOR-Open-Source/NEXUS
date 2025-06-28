namespace TRANSMUTANSTEIN.ChatServer.Services;

public class MatchmakingService(IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    private ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<MatchmakingService>>();

    public static ConcurrentDictionary<int, MatchmakingGroup> SoloPlayerGroups { get; set; } = [];
    public static ConcurrentDictionary<int, MatchmakingGroup> TwoPlayerGroups { get; set; } = [];
    public static ConcurrentDictionary<int, MatchmakingGroup> ThreePlayerGroups { get; set; } = [];
    public static ConcurrentDictionary<int, MatchmakingGroup> FourPlayerGroups { get; set; } = [];
    public static ConcurrentDictionary<int, MatchmakingGroup> FivePlayerGroups { get; set; } = [];

    public static ConcurrentDictionary<int, MatchmakingGroup> Groups =>
        new (new[] { SoloPlayerGroups, TwoPlayerGroups, ThreePlayerGroups, FourPlayerGroups, FivePlayerGroups }
            .SelectMany(dictionary => dictionary).ToDictionary(entry => entry.Key, entry => entry.Value));

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Matchmaking Service Has Started");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Matchmaking Service Has Stopped");

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        SoloPlayerGroups.Clear();
        TwoPlayerGroups.Clear();
        ThreePlayerGroups.Clear();
        FourPlayerGroups.Clear();
        FivePlayerGroups.Clear();
    }
}
