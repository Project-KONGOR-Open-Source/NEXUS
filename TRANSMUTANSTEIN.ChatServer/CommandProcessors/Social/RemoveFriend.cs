namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles friend removal notification.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_NOTIFY_BUDDY_REMOVE)]
public class RemoveFriend : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        RemoveFriendRequestData requestData = new(buffer);

        /*
            This is a NOOP (no operation) as per the implementation of the chat protocol on the side of the game client.
            The intention is to avoid notifying players when they have been removed from another player's friend list.
            The requesting player will still appear in the friend list of the removed player until they perform a logout/login cycle.
        */
    }
}

file class RemoveFriendRequestData
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