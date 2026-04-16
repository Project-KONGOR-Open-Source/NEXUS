namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

/// <summary>
///     Handles disconnect reason tracking data sent by match servers.
///     The match server reports a count for each disconnect reason type, which is used for tracking patterns of match failures caused by disconnected players.
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_SAVE_DISCONNECT_REASON)]
public class SaveDisconnectReason : ISynchronousCommandProcessor<MatchServerChatSession>
{
    public void Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        SaveDisconnectReasonRequestData requestData = new (buffer);

        // TODO: Aggregate And Track Disconnect Reason Counts Per Server For Monitoring And Reporting
        Log.Debug(@"Received Disconnect Reason Report From Server ID {ServerID} With {ReasonCount} Reason Categories",
            session.Metadata.ServerID, requestData.ReasonCounts.Length);
    }
}

file class SaveDisconnectReasonRequestData
{
    public byte[] CommandBytes { get; init; }

    /// <summary>
    ///     An array of disconnect counts indexed by <see cref="ChatProtocol.DisconnectReason"/>.
    ///     Each element represents the number of times a particular disconnect reason occurred on the match server.
    /// </summary>
    public int[] ReasonCounts { get; init; }

    public SaveDisconnectReasonRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();

        int reasonCount = (int) ChatProtocol.DisconnectReason.NUM_DISCONNECT_REASONS;

        ReasonCounts = new int[reasonCount];

        for (int i = 0; i < reasonCount; i++)
        {
            ReasonCounts[i] = buffer.ReadInt32();
        }
    }
}
