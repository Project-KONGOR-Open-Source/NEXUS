namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_DISCONNECT)]
public class ServerDisconnect(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        ServerDisconnectRequestData requestData = new(buffer);

        await session
            .TerminateMatchServer(distributedCacheStore);
    }
}

file class ServerDisconnectRequestData
{
    public ServerDisconnectRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }

    public byte[] CommandBytes { get; init; }
}