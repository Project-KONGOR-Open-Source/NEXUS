namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG)]
public class SendChannelMessage(FloodPreventionService floodPreventionService)
    : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SendChannelMessageRequestData requestData = new(buffer);

        ChatChannel channel = ChatChannel.Get(session, requestData.ChannelID);

        // Check Flood Prevention (Service Handles Both Check And Response)
        if (floodPreventionService.CheckAndHandleFloodPrevention(session) is false)
        {
            return;
        }

        // Check If The Sender Is Silenced In This Channel
        if (channel.IsSilenced(session))
        {
            ChatBuffer response = new();

            response.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCED);
            response.WriteInt32(requestData.ChannelID); // Channel ID

            session.Send(response);

            return;
        }

        string messageContent = requestData.Message;

        // Enforce Message Content Length Limit
        // Staff Accounts Are Exempt From Message Length Restrictions, For Moderation And Administration Purposes
        if (session.Account.Type is not AccountType.Staff &&
            messageContent.Length > ChatProtocol.CHAT_MESSAGE_MAX_LENGTH)
        {
            messageContent = messageContent[.. ChatProtocol.CHAT_MESSAGE_MAX_LENGTH];

            // TODO: Notify The Sender That Their Message Was Truncated
        }

        // Broadcast The Message To All Channel Members
        ChatBuffer broadcast = new();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG);
        broadcast.WriteInt32(session.Account.ID); // Sender Account ID
        broadcast.WriteInt32(requestData.ChannelID); // Channel ID
        broadcast.WriteString(messageContent); // Message Content (Potentially Truncated)

        channel.BroadcastMessage(broadcast, session.Account.ID);
    }
}