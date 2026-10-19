namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOINING_GAME)]
public class JoiningMatch(IDatabase distributedCacheStore, MatchLockService matchLockService) : IAsynchronousCommandProcessor<ClientChatSession>
{
    public async Task Process(ClientChatSession session, ChatBuffer buffer)
    {
        JoiningMatchRequestData requestData = new (buffer);

        session.ReportedJoiningGame = true;

        await matchLockService.EndReturnOnJoiningGame(session, requestData.ServerAddress);

        await session
            .PrepareToJoinMatch(distributedCacheStore, requestData.ServerAddress);
    }
}

file class JoiningMatchRequestData
{
    public byte[] CommandBytes { get; init; }

    public string ServerAddress { get; init; }

    public JoiningMatchRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerAddress = buffer.ReadString();
    }
}
