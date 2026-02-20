namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ENDED)]
public class MatchComplete : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchCompleteRequestData requestData = new (buffer);

        Log.Information(@"Match {MatchID} Ended: Reason={Reason}, WinningTeam={WinningTeam}, PlayerCount={PlayerCount}",
            requestData.MatchID, requestData.Reason, requestData.WinningTeam, requestData.PlayerCount);

        /*
            Match Information Is Not Removed From The Distributed Cache Here
            The Match Server Submits Statistics To The Master Server After This Message, And The Client Requests Match Statistics Shortly After
            Both The Stat Submission And The Match Stats Response Require The Cached Match Information
        */

        // TODO: Update Player Availability States (Mark Players As Available After Match Ends)
        // TODO: Mark Server As Available For New Match Allocation
        // TODO: Notify Players That Match Has Ended
        // TODO: Clean Up Match-Related Session State

        return Task.CompletedTask;
    }
}

file class MatchCompleteRequestData
{
    public byte[] CommandBytes { get; init; }

    public int MatchID { get; init; }

    public ChatProtocol.MatchEndedReason Reason { get; init; }

    public byte WinningTeam { get; init; }

    public byte PlayerCount { get; init; }

    public int[] AccountIDs { get; init; }

    public MatchCompleteRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchID = buffer.ReadInt32();
        Reason = (ChatProtocol.MatchEndedReason) buffer.ReadInt8();
        WinningTeam = buffer.ReadInt8();
        PlayerCount = buffer.ReadInt8();
        AccountIDs = new int[PlayerCount];

        for (int i = 0; i < PlayerCount; i++)
        {
            AccountIDs[i] = buffer.ReadInt32();
        }
    }
}
