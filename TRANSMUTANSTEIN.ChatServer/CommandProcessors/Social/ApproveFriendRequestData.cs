namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

public class ApproveFriendRequestData
{
    public ApproveFriendRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        FriendNickname = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string FriendNickname { get; }
}
