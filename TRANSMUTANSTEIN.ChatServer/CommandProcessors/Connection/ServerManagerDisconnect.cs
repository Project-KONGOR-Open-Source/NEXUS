namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ServerManagerToChatServer.NET_CHAT_SM_DISCONNECT)]
public class ServerManagerDisconnect(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        ServerManagerDisconnectRequestData requestData = new(buffer);

        await session
            .TerminateMatchServerManager(distributedCacheStore);
    }
}