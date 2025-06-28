namespace TRANSMUTANSTEIN.ChatServer.Services;

public class MatchmakingService(IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    private ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<MatchmakingService>>();

    public static ConcurrentDictionary<int, MatchmakingGroup> Groups { get; set; } = [];

    public static ConcurrentDictionary<int, MatchmakingGroup> SoloPlayerGroups
        => new (Groups.Where(group => group.Value.Members.Count == 1));

    public static ConcurrentDictionary<int, MatchmakingGroup> TwoPlayerGroups
        => new (Groups.Where(group => group.Value.Members.Count == 2));

    public static ConcurrentDictionary<int, MatchmakingGroup> ThreePlayerGroups
        => new (Groups.Where(group => group.Value.Members.Count == 3));

    public static ConcurrentDictionary<int, MatchmakingGroup> FourPlayerGroups
        => new (Groups.Where(group => group.Value.Members.Count == 4));

    public static ConcurrentDictionary<int, MatchmakingGroup> FivePlayerGroups
        => new (Groups.Where(group => group.Value.Members.Count == 5));

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
        Groups.Clear();

        GC.SuppressFinalize(this);
    }
}
