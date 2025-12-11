namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles friend removal notification.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_NOTIFY_BUDDY_REMOVE)]
public class RemoveFriend : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        RemoveFriendNotificationData notificationData = new (buffer);

        /*
            This is a NOOP (no operation) as per the implementation of the chat protocol on the side of the game client.
            The intention is to avoid notifying players when they have been removed from another player's friend list.
            The requesting player will still appear in the friend list of the removed player until they perform a logout/login cycle.
        */
        return;
    }
}

public class RemoveFriendNotificationData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public int RemovedFriendAccountID = buffer.ReadInt32();

    /// <summary>
    ///     The ID of the notification for removing another player from the client's friend list.
    ///     Used for managing notifications while the client is offline; to be received on next login.
    /// </summary>
    public int RequesterNotificationID = buffer.ReadInt32();

    /// <summary>
    ///     The ID of the notification for being removed from another player's friend list.
    ///     Used for managing notifications while the client is offline; to be received on next login.
    /// </summary>
    public int RemovedFriendNotificationID = buffer.ReadInt32();
}
