namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

using global::TRANSMUTANSTEIN.ChatServer.Internals;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG)]
public class SendChannelMessage(FloodPreventionService floodPreventionService, IChatContext chatContext)
    : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SendChannelMessageRequestData requestData = new(buffer);

        ChatChannel? channel = ChatChannel.Get(chatContext, session, requestData.ChannelID);

        if (channel is null)
        {
            return;
        }

        // Check Flood Prevention (Service Handles Both Check And Response)
        if (floodPreventionService.CheckAndHandleFloodPrevention(session) is false)
        {
            return;
        }

        // Check If The Sender Is Silenced In This Channel
        if (channel.IsSilenced(session))
        {
            Console.WriteLine($"[DEBUG] SendChannelMessage: User {session.Account.Name} Silenced.");
            ChatBuffer response = new();

            response.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCED);
            response.WriteInt32(requestData.ChannelID); // Channel ID

            session.Send(response);

            return;
        }

        string messageContent = requestData.Message;
        Console.WriteLine($"[DEBUG] SendChannelMessage: Processing message '{messageContent}' from {session.Account.Name} to Channel {channel.Name} ({channel.ID}).");

        // Enforce Message Content Length Limit
        // Staff Accounts Are Exempt From Message Length Restrictions, For Moderation And Administration Purposes
        if (session.Account.Type is not AccountType.Staff &&
            messageContent.Length > ChatProtocol.CHAT_MESSAGE_MAX_LENGTH)
        {
            messageContent = messageContent[..ChatProtocol.CHAT_MESSAGE_MAX_LENGTH];

            // TODO: Notify The Sender That Their Message Was Truncated
        }

        // Broadcast The Message To All Channel Members
        ChatBuffer broadcast = new();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG);
        broadcast.WriteInt32(session.Account.ID); // Sender Account ID
        broadcast.WriteInt32(requestData.ChannelID); // Channel ID
        broadcast.WriteString(messageContent); // Message Content (Potentially Truncated)

        Console.WriteLine($"[DEBUG] SendChannelMessage: Broadcasting to {channel.Members.Count} members.");
        channel.BroadcastMessage(broadcast, session.Account.ID);
    }
}