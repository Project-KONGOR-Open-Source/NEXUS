namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

public class RemoveFriendRequestData
{
    public RemoveFriendRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        RemovedFriendAccountID = buffer.ReadInt32();
        RequesterNotificationID = buffer.ReadInt32();
        RemovedFriendNotificationID = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }

    public int RemovedFriendAccountID { get; init; }

    /// <summary>
    ///     The ID of the notification for removing another player from the client's friend list.
    ///     Used for managing notifications while the client is offline; to be received on next login.
    /// </summary>
    public int RequesterNotificationID { get; init; }

    /// <summary>
    ///     The ID of the notification for being removed from another player's friend list.
    ///     Used for managing notifications while the client is offline; to be received on next login.
    /// </summary>
    public int RemovedFriendNotificationID { get; init; }
}