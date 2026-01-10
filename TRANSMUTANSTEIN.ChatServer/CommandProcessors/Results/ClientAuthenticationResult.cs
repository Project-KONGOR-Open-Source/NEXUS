namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Results;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_CLIENT_AUTH_RESULT)]
public class ClientAuthenticationResult : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        ClientAuthenticationResultRequestData requestData = new(buffer);

        if (requestData.Result is not ChatProtocol.ClientAuthenticationResult.CAR_SUCCESS)
        {
            Log.Error(@"Client Authentication Failed On Match Server ID ""{ServerID}"" With Result ""{Result}""",
                session.ServerMetadata.ServerID, requestData.Result);
        }
    }
}