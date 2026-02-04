namespace TRANSMUTANSTEIN.ChatServer.Services;

public class MatchmakingService : BackgroundService, IMatchmakingService
{
    public ConcurrentDictionary<int, MatchmakingGroup> Groups { get; } = [];

    public ConcurrentDictionary<int, MatchmakingGroup> SoloPlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 1));

    public ConcurrentDictionary<int, MatchmakingGroup> TwoPlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 2));

    public ConcurrentDictionary<int, MatchmakingGroup> ThreePlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 3));

    public ConcurrentDictionary<int, MatchmakingGroup> FourPlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 4));

    public ConcurrentDictionary<int, MatchmakingGroup> FivePlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 5));

    public override void Dispose()
    {
        base.Dispose();

        GC.SuppressFinalize(this);
    }

    public MatchmakingGroup? GetMatchmakingGroup(OneOf<int, string> memberIdentifier)
    {
        return memberIdentifier.Match(id => GetMatchmakingGroupByMemberID(id),
            name => GetMatchmakingGroupByMemberName(name));
    }

    public MatchmakingGroup? GetMatchmakingGroupByMemberID(int memberID)
    {
        return Groups.Values.SingleOrDefault(group => group.Members.Any(member => member.Account.ID == memberID));
    }

    public MatchmakingGroup? GetMatchmakingGroupByMemberName(string memberName)
    {
        return Groups.Values.SingleOrDefault(group =>
            group.Members.Any(member => member.Account.Name.Equals(memberName)));
    }

    /// <summary>
    ///     Finds a Matchmaking Group where the specified user is listed in <see cref="MatchmakingGroup.PendingInvites"/>.
    ///     <para>
    ///         CRITICAL: This iterates ALL active groups (O(N)). Use as a fallback only when primary lookup by Member Name fails.
    ///         This supports the scenario where a client attempts to join using their OWN name (as an invitee) rather than the Leader's name.
    ///     </para>
    /// </summary>
    public MatchmakingGroup? GetMatchmakingGroupByInvitedUser(string accountName)
    {
        // Try to find a group where the user is in the PendingInvites list
        return Groups.Values.SingleOrDefault(group => 
            group.PendingInvites.ContainsKey(accountName));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Matchmaking Service Is Starting");

        await RunMatchBroker(stoppingToken);

        Log.Information("Matchmaking Service Has Started");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information("Matchmaking Service Is Stopping");

        await base.StopAsync(cancellationToken);

        Log.Information("Matchmaking Service Has Stopped");
    }

    private async Task RunMatchBroker(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            // TODO: Implement Match Broker Logic Here

            # region Match Broker Logic Placeholder

            if (Groups.Count == 2 && Groups.Values.First().QueueDuration != TimeSpan.Zero &&
                Groups.Values.Last().QueueDuration != TimeSpan.Zero)
            {
                List<MatchmakingGroup> team_1 = [Groups.Values.First()];
                List<MatchmakingGroup> team_2 = [Groups.Values.Last()];

                List<MatchmakingGroup> groups = [.. team_1, .. team_2];

                foreach (MatchmakingGroup group in groups)
                {
                    group.QueueStartTime = null;
                }

                ChatBuffer found = new();

                found.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
                found.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType
                    .TMM_GROUP_FOUND_SERVER)); // Sound The Horn !!!

                // TODO: This Packet Can Be Sent With TMM_GROUP_QUEUE_UPDATE And A 4-Byte Integer To Update The Average Time In Queue (In Seconds)

                foreach (MatchmakingGroup group in groups)
                    foreach (MatchmakingGroupMember member in group.Members)
                    {
                        member.Session.Send(found);
                    }
            }

            # endregion

            await Task.Delay(100, cancellationToken);
        }

        await Task.CompletedTask;
    }
}