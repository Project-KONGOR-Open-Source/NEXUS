namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ENDED)]
public class MatchComplete : ISynchronousCommandProcessor<MatchServerChatSession>
{
    public void Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchCompleteRequestData requestData = new (buffer);

        // TODO: Update Player Availability States (Mark Players As Available After Match Ends)
        // TODO: Remove Match From Distributed Cache
        // TODO: Mark Server As Available For New Match Allocation
        // TODO: Notify Players That Match Has Ended
        // TODO: Clean Up Match-Related Session State

        // NOTE: Statistics submission and MMR/PSR updates are handled by KONGOR.MasterServer/Controllers/StatsRequesterController.
    }
}

file class MatchCompleteRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ServerID { get; init; }

    public int MatchID { get; init; }

    public int WinningTeam { get; init; }

    public int MatchDuration { get; init; }

    public MatchCompleteRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        
        // Safely partial read
        if (buffer.HasRemainingData()) ServerID = buffer.ReadInt32();
        if (buffer.HasRemainingData()) MatchID = buffer.ReadInt32();
        if (buffer.HasRemainingData()) WinningTeam = buffer.ReadInt32();
        if (buffer.HasRemainingData()) MatchDuration = buffer.ReadInt32();
    }
}
