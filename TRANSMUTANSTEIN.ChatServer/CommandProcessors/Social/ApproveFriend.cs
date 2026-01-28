namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

using global::TRANSMUTANSTEIN.ChatServer.Internals;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Communication;

/// <summary>
///     Handles friend approval requests.
///     Approves a pending friend request from another player, creating a bi-directional friendship.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_APPROVE)]
public class ApproveFriend(MerrickContext merrick, IDatabase distributedCacheStore, IChatContext chatContext)
    : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        ApproveFriendRequestData requestData = new(buffer);

        await Friend
            .WithAccountName(requestData.FriendNickname)
            .Approve(session, merrick, distributedCacheStore, chatContext);
    }
}