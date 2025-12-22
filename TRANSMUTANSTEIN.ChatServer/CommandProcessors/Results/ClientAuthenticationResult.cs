namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Results;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_CLIENT_AUTH_RESULT)]
public class ClientAuthenticationResult : ISynchronousCommandProcessor<MatchServerChatSession>
{
    public void Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        ClientAuthResultData resultData = new (buffer);

        if (resultData.Result is not ChatProtocol.ClientAuthenticationResult.CAR_SUCCESS)
        {
            Log.Error(@"Client Authentication Failed On Match Server ID ""{ServerID}"" With Result ""{Result}""", session.Metadata.ServerID, resultData.Result);
        }
    }
}

file class ClientAuthResultData
{
    public byte[] CommandBytes { get; init; }

    public ChatProtocol.ClientAuthenticationResult Result { get; init; }

    public ClientAuthResultData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Result = (ChatProtocol.ClientAuthenticationResult) buffer.ReadInt8();
    }
}
