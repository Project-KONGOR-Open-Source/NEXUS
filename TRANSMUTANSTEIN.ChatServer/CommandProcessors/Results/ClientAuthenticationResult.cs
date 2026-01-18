namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Results;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_CLIENT_AUTH_RESULT)]
public class ClientAuthenticationResult : ISynchronousCommandProcessor<MatchServerChatSession>
{
    public void Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        ClientAuthenticationResultRequestData requestData = new (buffer);

        if (requestData.Result is not ChatProtocol.ClientAuthenticationResult.CAR_SUCCESS)
        {
            Log.Error(@"Client Authentication Failed On Match Server ID ""{ServerID}"" With Result ""{Result}""", session.Metadata.ServerID, requestData.Result);
        }
    }
}

file class ClientAuthenticationResultRequestData
{
    public byte[] CommandBytes { get; init; }

    public ChatProtocol.ClientAuthenticationResult Result { get; init; }

    public ClientAuthenticationResultRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Result = (ChatProtocol.ClientAuthenticationResult) buffer.ReadInt8();
    }
}
