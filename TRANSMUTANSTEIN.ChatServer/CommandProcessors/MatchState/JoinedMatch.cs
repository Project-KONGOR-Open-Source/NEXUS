namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOINED_GAME)]
public class JoinedMatch(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        JoinedMatchRequestData requestData = new (buffer);

        await session
            .JoinMatch(distributedCacheStore, requestData.MatchID);
    }
}

file class JoinedMatchRequestData
{
    public byte[] CommandBytes { get; init; }

    public string MatchName { get; init; }

    public int MatchID { get; init; }

    public bool JoinMatchChannel { get; init; }

    public JoinedMatchRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchName = buffer.ReadString();
        MatchID = buffer.ReadInt32();

        // Don't Add The Player To The Match Chat Channel If They Have Joined As A Spectator Or A Mentor
        JoinMatchChannel = buffer.ReadInt8() is not 0;
    }
}

