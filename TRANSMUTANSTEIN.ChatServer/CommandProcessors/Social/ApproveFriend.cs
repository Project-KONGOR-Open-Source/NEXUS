namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles friend approval requests.
///     Approves a pending friend request from another player, creating a bi-directional friendship.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_APPROVE)]
public class ApproveFriend(MerrickContext merrick, IDatabase distributedCacheStore) : IAsynchronousCommandProcessor
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        ApproveFriendRequestData requestData = new (buffer);

        await Friend
            .WithAccountName(requestData.FriendNickname)
            .Approve(session, merrick, distributedCacheStore);
    }
}

public class ApproveFriendRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public string FriendNickname = buffer.ReadString();
}
