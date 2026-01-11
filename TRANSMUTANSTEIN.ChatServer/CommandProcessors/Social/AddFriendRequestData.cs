namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

public class AddFriendRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string FriendNickname { get; } = buffer.ReadString();
}