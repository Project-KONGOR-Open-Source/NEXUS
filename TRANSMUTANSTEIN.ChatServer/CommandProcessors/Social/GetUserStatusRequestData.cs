namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

public class GetUserStatusRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public int AccountID { get; } = buffer.ReadInt32();
}
