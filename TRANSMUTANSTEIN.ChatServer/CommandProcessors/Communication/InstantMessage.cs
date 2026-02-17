namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

/// <summary>
///     Handles instant messages via the CC panel.
///     Supports first-contact client info exchange (type 1/2) and subsequent short-form messages (type 0).
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_IM)]
public class InstantMessage : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        InstantMessageRequestData requestData = new (buffer);

        if (string.IsNullOrEmpty(requestData.Message))
            return;

        // Find Recipient
        ClientChatSession? recipientSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(requestData.TargetName, StringComparison.OrdinalIgnoreCase));

        // Treat Invisible And Offline The Same Way — Send IM Failed
        if (recipientSession is null || recipientSession.Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
        {
            SendFailure(session, requestData.TargetName);

            return;
        }

        string truncatedMessage = requestData.Message.Length > ChatProtocol.CHAT_MESSAGE_MAX_LENGTH
            ? requestData.Message[..ChatProtocol.CHAT_MESSAGE_MAX_LENGTH]
            : requestData.Message;

        if (requestData.SendClientInformation)
        {
            // First Contact: Send Full Client Info To Recipient (Type 1)
            ChatBuffer imToRecipient = new ();

            imToRecipient.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM);
            imToRecipient.WriteInt8(1);
            WriteClientInformation(imToRecipient, session);
            imToRecipient.WriteString(truncatedMessage);

            recipientSession.Send(imToRecipient);

            // First Contact: Send Recipient's Client Info Back To Sender (Type 2)
            ChatBuffer imToSender = new ();

            imToSender.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM);
            imToSender.WriteInt8(2);
            WriteClientInformation(imToSender, recipientSession);
            imToSender.WriteString(truncatedMessage);

            session.Send(imToSender);
        }
        else
        {
            // Subsequent Messages: Short Form (Type 0)
            ChatBuffer im = new ();

            im.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM);
            im.WriteInt8(0);
            im.WriteString(session.Account.Name);
            im.WriteString(truncatedMessage);

            recipientSession.Send(im);
        }
    }

    /// <summary>
    ///     Writes the client info buffer for instant message exchange.
    ///     Fields: name, account ID, status, flags, chat name colour, account icon.
    /// </summary>
    private static void WriteClientInformation(ChatBuffer buffer, ClientChatSession session)
    {
        buffer.WriteString(session.Account.Name);
        buffer.WriteInt32(session.Account.ID);
        buffer.WriteInt8(Convert.ToByte(session.Metadata.LastKnownClientState));
        buffer.WriteInt8(session.Account.GetChatClientFlags());
        buffer.WriteString(session.Account.NameColourNoPrefixCode);
        buffer.WriteString(session.Account.IconNoPrefixCode);
    }

    private static void SendFailure(ClientChatSession session, string targetName)
    {
        ChatBuffer failure = new ();

        failure.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM_FAILED);
        failure.WriteString(targetName);

        session.Send(failure);
    }
}

file class InstantMessageRequestData
{
    public byte[] CommandBytes { get; init; }

    public string TargetName { get; init; }

    public string Message { get; init; }

    /// <summary>
    ///     When TRUE, the sender is requesting a client info exchange (first contact).
    /// </summary>
    public bool SendClientInformation { get; init; }

    public InstantMessageRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetName = buffer.ReadString();
        Message = buffer.ReadString();
        SendClientInformation = buffer.ReadInt8() == 1;
    }
}
