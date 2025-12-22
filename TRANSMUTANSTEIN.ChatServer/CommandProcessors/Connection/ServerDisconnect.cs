namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_DISCONNECT)]
public class ServerDisconnect(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        ServerDisconnectRequestData requestData = new (buffer);

        await session
            .Terminate(distributedCacheStore);
    }
}

file class ServerDisconnectRequestData
{
    public byte[] CommandBytes { get; init; }

    public ServerDisconnectRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}
