namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOINING_GAME)]
public class JoiningMatch(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<ClientChatSession>
{
    public async Task Process(ClientChatSession session, ChatBuffer buffer)
    {
        JoiningGameData requestData = new (buffer);

        // TODO: Attempt To Reconnect Client To Their Last Unfinished Game
        // INFO: Check If Client Has A Valid LastPlayedMatchID In Session Or Database
        // INFO: If Yes, Find Game Servers Hosting That Match
        // INFO: Validate Game Is In Progress (Phase 2-6, Not Lobby/Inactive/Finished)
        // INFO: If Valid, Send AutoMatchConnectBroadcast With Server Details And Return Early

        await session
            .JoinMatch(distributedCacheStore, requestData.ServerAddress);
    }
}

file class JoiningGameData
{
    public byte[] CommandBytes { get; init; }

    public string ServerAddress { get; init; }

    public JoiningGameData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerAddress = buffer.ReadString();
    }
}
