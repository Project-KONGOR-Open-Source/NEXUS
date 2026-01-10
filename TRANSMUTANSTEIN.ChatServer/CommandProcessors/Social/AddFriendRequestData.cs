namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

public class AddFriendRequestData
{
    public AddFriendRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        FriendNickname = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string FriendNickname { get; }
}
