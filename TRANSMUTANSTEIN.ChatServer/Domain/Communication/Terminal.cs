namespace TRANSMUTANSTEIN.ChatServer.Domain.Communication;

/// <summary>
///     Broadcasts operational log messages to the staff-only TERMINAL chat channel.
///     The messages are authored by a synthetic channel member, because the client only renders channel messages from senders it knows about.
/// </summary>
public static class Terminal
{
    /// <summary>
    ///     The account ID of the synthetic channel member which authors the operational log messages.
    ///     The value is outside the range of real account IDs, so it can never collide with an actual account.
    /// </summary>
    private const int SyntheticMemberAccountID = int.MaxValue;

    /// <summary>
    ///     Announces the synthetic TERMINAL channel member to a client which has just joined the TERMINAL channel.
    ///     The client drops channel messages from senders it does not know about, so the synthetic member needs to be announced before its messages can be rendered.
    /// </summary>
    public static void AnnounceSyntheticMember(ClientChatSession session)
    {
        ChatBuffer announce = new ();

        announce.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOINED_CHANNEL);
        announce.WriteInt32(ChatChannels.StaffChannel.GetDeterministicInt32Hash());                     // Channel ID
        announce.WriteString(ChatChannels.StaffChannel);                                                // Member Account Name
        announce.WriteInt32(SyntheticMemberAccountID);                                                  // Member Account ID
        announce.WriteInt8(Convert.ToByte(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED)); // Connection Status
        announce.WriteInt8(Convert.ToByte(ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_NONE));             // Channel Administrator Level
        announce.WriteString(string.Empty);                                                             // Chat Symbol
        announce.WriteString(string.Empty);                                                             // Name Colour
        announce.WriteString(string.Empty);                                                             // Account Icon
        announce.WriteInt32(0);                                                                         // Ascension Level

        session.Send(announce);
    }

    /// <summary>
    ///     Broadcasts the given lines to all members of the TERMINAL channel, as one channel message per line.
    ///     Does nothing when the channel does not exist or has no members.
    /// </summary>
    public static void Broadcast(params string[] lines)
    {
        if (Context.ChatChannels.TryGetValue(ChatChannels.StaffChannel, out ChatChannel? channel) is false)
            return;

        if (channel.Members.IsEmpty)
            return;

        foreach (string line in lines)
        {
            ChatBuffer message = new ();

            message.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG);
            message.WriteInt32(SyntheticMemberAccountID); // Sender Account ID
            message.WriteInt32(channel.ID);               // Channel ID
            message.WriteString(line);                    // Message Content

            channel.BroadcastMessage(message);
        }
    }

    /// <summary>
    ///     Summarises the connected match server managers per aggregated region.
    /// </summary>
    public static string ManagersPerRegion()
    {
        IEnumerable<string> aggregates = Context.MatchServerManagerChatSessions.Values
            .Select(session => GameRegions.NormaliseServerLocation(session.Metadata.Location ?? string.Empty));

        return $"Managers Per Region: {FormatCounts(aggregates)}";
    }

    /// <summary>
    ///     Summarises the connected match servers per aggregated region.
    /// </summary>
    public static string ServersPerRegion()
    {
        IEnumerable<string> aggregates = Context.MatchServerChatSessions.Values
            .Select(session => GameRegions.NormaliseServerLocation(session.Metadata.Location ?? string.Empty));

        return $"Servers Per Region: {FormatCounts(aggregates)}";
    }

    /// <summary>
    ///     Summarises the users in the matchmaking queue per aggregated region.
    ///     A group queued for regions in more than one aggregate counts its members towards each of those aggregates.
    /// </summary>
    public static string UsersInQueuePerRegion()
    {
        Dictionary<string, int> counts = new (StringComparer.OrdinalIgnoreCase);

        foreach (MatchmakingGroup group in MatchmakingService.Groups.Values.Where(group => group.IsQueued))
            foreach (string aggregate in group.Information.GameRegions.Select(GameRegions.GetAggregate).Distinct(StringComparer.OrdinalIgnoreCase))
                counts[aggregate] = counts.GetValueOrDefault(aggregate) + group.Members.Count;

        return $"Users In Queue Per Region: {FormatCounts(counts)}";
    }

    /// <summary>
    ///     Summarises the users currently in matches per aggregated region of the match server they are connected to.
    /// </summary>
    public static string UsersInMatchesPerRegion()
    {
        IEnumerable<string> aggregates = Context.ClientChatSessions.Values
            .Where(session => session.Metadata.LastKnownClientState is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
            .Select(session => GameRegions.NormaliseServerLocation(session.Metadata.MatchServerConnectedTo?.Location ?? string.Empty));

        return $"Users In Matches Per Region: {FormatCounts(aggregates)}";
    }

    private static string FormatCounts(IEnumerable<string> aggregates)
    {
        Dictionary<string, int> counts = new (StringComparer.OrdinalIgnoreCase);

        foreach (string aggregate in aggregates)
            counts[aggregate] = counts.GetValueOrDefault(aggregate) + 1;

        return FormatCounts(counts);
    }

    private static string FormatCounts(Dictionary<string, int> counts)
        => counts.Count is 0 ? "None" : string.Join(", ", counts.OrderBy(count => count.Key, StringComparer.OrdinalIgnoreCase).Select(count => $"{count.Key}: {count.Value}"));
}
