namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ServerManagerToChatServer.NET_CHAT_SM_DISCONNECT)]
public class ServerManagerDisconnect(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerManagerChatSession>
{
    public async Task Process(MatchServerManagerChatSession session, ChatBuffer buffer)
    {
        ServerManagerDisconnectRequestData requestData = new (buffer);

        await session
            .Terminate(distributedCacheStore);
    }
}

file class ServerManagerDisconnectRequestData
{
    public byte[] CommandBytes { get; init; }

    public ServerManagerDisconnectRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}
