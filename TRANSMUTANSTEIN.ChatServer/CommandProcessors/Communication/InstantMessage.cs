namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

/// <summary>
///     Handles instant messages via the CC panel.
///     The response type byte indicates the message kind:
///     <list type="bullet">
///         <item>
///             <term>Type 0</term>
///             <description>
///                 Subsequent (short-form) message. Contains only the sender name and message text.
///                 Used after the first contact has already been established and both sides have each other's client information.
///                 The client displays this as a normal received IM.
///             </description>
///         </item>
///         <item>
///             <term>Type 1</term>
///             <description>
///                 First-contact message sent to the <b>recipient</b>. Contains the sender's full client information, followed by the message text.
///                 The recipient's client uses this to populate the sender's entry in its local user list and then displays the message as a received IM.
///             </description>
///         </item>
///         <item>
///             <term>Type 2</term>
///             <description>
///                 First-contact acknowledgement sent back to the <b>sender</b>.
///                 Contains the recipient's full client information, followed by the original message text.
///                 The sender's client uses this to populate the recipient's entry in its local user list and then displays the message as a sent IM (confirming delivery).
///             </description>
///         </item>
///     </list>
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

        // Treat Invisible And Offline The Same Way: Send IM Failed
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
            imToRecipient.WriteInt8(1);                     // Message Type 1: Client Information Exchange Request
            WriteClientInformation(imToRecipient, session); // Sender's Client Information
            imToRecipient.WriteString(truncatedMessage);    // Message Content

            recipientSession.Send(imToRecipient);

            // First Contact: Send Recipient's Client Info Back To Sender (Type 2)
            ChatBuffer imToSender = new ();

            imToSender.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM);
            imToSender.WriteInt8(2);                              // Message Type 2: Client Information Exchange Response
            WriteClientInformation(imToSender, recipientSession); // Recipient's Client Information
            imToSender.WriteString(truncatedMessage);             // Message Content

            session.Send(imToSender);
        }

        else
        {
            // Subsequent Messages: Short Form (Type 0)
            ChatBuffer im = new ();

            im.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM);
            im.WriteInt8(0);                      // Message Type 0: Short Form
            im.WriteString(session.Account.Name); // Sender Account Name
            im.WriteString(truncatedMessage);     // Message Content

            recipientSession.Send(im);
        }
    }

    /// <summary>
    ///     Writes the client info buffer for instant message exchange.
    /// </summary>
    private static void WriteClientInformation(ChatBuffer buffer, ClientChatSession session)
    {
        buffer.WriteString(session.Account.Name);                                // Account Name
        buffer.WriteInt32(session.Account.ID);                                   // Account ID
        buffer.WriteInt8(Convert.ToByte(session.Metadata.LastKnownClientState)); // Last Known Client State
        buffer.WriteInt8(session.Account.GetChatClientFlags());                  // Chat Client Flags
        buffer.WriteString(session.Account.NameColourNoPrefixCode);              // Chat Name Colour
        buffer.WriteString(session.Account.IconNoPrefixCode);                    // Account Icon
    }

    private static void SendFailure(ClientChatSession session, string targetName)
    {
        ChatBuffer failure = new ();

        failure.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM_FAILED);
        failure.WriteString(targetName); // Target Account Name

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
