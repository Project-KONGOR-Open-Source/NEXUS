namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOINING_GAME)]
public class JoiningMatch(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        JoiningMatchRequestData requestData = new(buffer);

        // TODO: Attempt To Reconnect Client To Their Last Unfinished Game
        // INFO: Check If Client Has A Valid LastPlayedMatchID In Session Or Database
        // INFO: If Yes, Find Game Servers Hosting That Match
        // INFO: Validate Game Is In Progress (Phase 2-6, Not Lobby/Inactive/Finished)
        // INFO: If Valid, Send AutoMatchConnectBroadcast With Server Details And Return Early

        await session
            .PrepareToJoinMatch(distributedCacheStore, requestData.ServerAddress);
    }
}

file class JoiningMatchRequestData
{
    public JoiningMatchRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerAddress = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string ServerAddress { get; }
}