namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Results;

public class ClientAuthenticationResultRequestData
{
    public ClientAuthenticationResultRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Result = (ChatProtocol.ClientAuthenticationResult) buffer.ReadInt8();
    }

    public byte[] CommandBytes { get; init; }

    public ChatProtocol.ClientAuthenticationResult Result { get; }
}
