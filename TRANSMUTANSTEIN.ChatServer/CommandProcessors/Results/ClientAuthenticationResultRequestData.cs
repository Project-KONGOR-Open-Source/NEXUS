namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Results;

public class ClientAuthenticationResultRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public ChatProtocol.ClientAuthenticationResult Result { get; } = (ChatProtocol.ClientAuthenticationResult) buffer.ReadInt8();
}