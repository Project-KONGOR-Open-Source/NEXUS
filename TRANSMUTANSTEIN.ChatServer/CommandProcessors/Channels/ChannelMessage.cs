namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG)]
public class ChannelMessage : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        ChannelMessageRequestData requestData = new (buffer);

        ChatChannel channel = ChatChannel.Get(session, requestData.ChannelID);

        // Check If The Sender Is Silenced In This Channel
        if (channel.IsSilenced(session))
        {
            ChatBuffer response = new ();

            response.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCED);
            response.WriteInt32(requestData.ChannelID); // Channel ID

            session.Send(response);

            return;
        }

        // Broadcast The Message To All Channel Members
        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG);
        broadcast.WriteInt32(session.Account.ID);    // Sender Account ID
        broadcast.WriteInt32(requestData.ChannelID); // Channel ID
        broadcast.WriteString(requestData.Message);  // Message Content

        channel.BroadcastMessage(broadcast);
    }
}

public class ChannelMessageRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public string Message = buffer.ReadString();

    public int ChannelID = buffer.ReadInt32();
}
