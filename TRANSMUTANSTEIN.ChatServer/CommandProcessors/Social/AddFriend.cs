namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles friend addition requests.
///     Adds the target account to the requester's friend list, and persists the changes to the database.
///     If the target has already made a friend addition request to the requester, immediately creates a bi-directional
///     friendship.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_ADD)]
public class AddFriend(MerrickContext merrick, IDatabase distributedCacheStore)
    : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        AddFriendRequestData requestData = new(buffer);

        await Friend
            .WithAccountName(requestData.FriendNickname)
            .Add(session, merrick, distributedCacheStore);
    }
}