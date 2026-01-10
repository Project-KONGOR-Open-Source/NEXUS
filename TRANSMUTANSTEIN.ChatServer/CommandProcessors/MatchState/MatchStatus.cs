namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ONGOING)]
public class MatchStatus : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        MatchStatusRequestData requestData = new (buffer);

        // TODO: Update Match Phase In Distributed Cache (In Progress, Paused, Resumed)
        // TODO: Update Player Availability States (Mark Players As InGame When Match Starts)
        // TODO: Track Match Duration For Statistics And Timeout Detection
    }
}

file class MatchStatusRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ServerID { get; init; }

    public int MatchID { get; init; }

    public int Phase { get; init; }

    public int CurrentGameTime { get; init; }

    public MatchStatusRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerID = buffer.ReadInt32();
        MatchID = buffer.ReadInt32();
        Phase = buffer.ReadInt32();
        CurrentGameTime = buffer.ReadInt32();
    }
}

