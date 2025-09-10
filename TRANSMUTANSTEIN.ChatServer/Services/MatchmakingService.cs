namespace TRANSMUTANSTEIN.ChatServer.Services;

public class MatchmakingService(IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    private ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<MatchmakingService>>();

    public static ConcurrentDictionary<int, MatchmakingGroup> Groups { get; set; } = [];

    public static MatchmakingGroup? GetMatchmakingGroup(OneOf<int, string> memberIdentifier)
        => memberIdentifier.Match(id => GetMatchmakingGroupByMemberID(id), name => GetMatchmakingGroupByMemberName(name));

    public static MatchmakingGroup? GetMatchmakingGroupByMemberID(int memberID)
        => Groups.Values.SingleOrDefault(group => group.Members.Any(member => member.Account.ID == memberID));

    public static MatchmakingGroup? GetMatchmakingGroupByMemberName(string memberName)
        => Groups.Values.SingleOrDefault(group => group.Members.Any(member => member.Account.Name.Equals(memberName)));

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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Matchmaking Service Has Started");

        await RunMatchBroker(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Matchmaking Service Has Stopped");

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Groups.Clear();

        GC.SuppressFinalize(this);
    }

    private async Task RunMatchBroker(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            // TODO: Implement Match Broker Logic Here

            # region Match Broker Logic Placeholder
            if (Groups.Count == 2 && Groups.Values.First().QueueDuration != TimeSpan.Zero && Groups.Values.Last().QueueDuration != TimeSpan.Zero)
            {
                List<MatchmakingGroup> team_1 = [Groups.Values.First()];
                List<MatchmakingGroup> team_2 = [Groups.Values.Last()];

                Parallel.ForEach([..team_1, ..team_2], group => group.QueueStartTime = null);

                ChatBuffer found = new ();

                found.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
                found.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_FOUND_SERVER)); // The Horn !!!

                // TODO: This Packet Can Be Sent With TMM_GROUP_QUEUE_UPDATE And A 4-Byte Integer To Update The Average Time In Queue (In Seconds)

                found.PrependBufferSize();

                Parallel.ForEach([..team_1, ..team_2], group => Parallel.ForEach(group.Members, member => member.Session.SendAsync(found.Data)));
            }
            # endregion
        }

        await Task.CompletedTask;
    }
}
