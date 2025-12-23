
using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Gaming;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_CLIENT_AUTH_RESULT)]
public class ClientAuthResult : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        ClientAuthResultData resultData = new (buffer);

        if (resultData.Result != ChatProtocol.ClientAuthResult.CAR_SUCCESS)
        {
            Log.Warning(@"Client Authentication Failed For Account ID ""{AccountID}"" On Match Server ID ""{ServerID}"" - Result: {Result}", resultData.AccountID, session.Metadata.ServerID, resultData.Result);
        }
        else
        {
            Log.Debug(@"Client Account ID ""{AccountID}"" Authenticated Successfully On Match Server ID ""{ServerID}""", resultData.AccountID, session.Metadata.ServerID);
        }

        return Task.CompletedTask;
    }
}

file class ClientAuthResultData
{
    public byte[] CommandBytes { get; init; }

    public int AccountID { get; init; }

    public ChatProtocol.ClientAuthResult Result { get; init; }

    public ClientAuthResultData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();

        try
        {
            AccountID = buffer.ReadInt32();
            Result = (ChatProtocol.ClientAuthResult)buffer.ReadInt8();
        }
        catch (InvalidDataException)
        {
            AccountID = -1;
            Result = (ChatProtocol.ClientAuthResult)buffer.ReadInt8();
        }
    }
}
