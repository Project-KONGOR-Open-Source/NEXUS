namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_IM)]
public class SendInstantMessage : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        SendInstantMessageRequestData requestData = new (buffer);

        // Find Target Session
        ClientChatSession? targetSession = Context.ClientChatSessions.Values
            .FirstOrDefault(s => s.Account.Name.Equals(requestData.TargetName, StringComparison.OrdinalIgnoreCase) 
                             || s.Account.NameWithClanTag.Equals(requestData.TargetName, StringComparison.OrdinalIgnoreCase));

        // Check If Target Is Offline Or Invisible
        if (targetSession is null || targetSession.Metadata.ClientChatModeState == ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
        {
            // Send Failure Response To Sender
            ChatBuffer failure = new ();
            failure.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM_FAILED);
            failure.WriteString(requestData.TargetName);
            
            session.Send(failure);
            return;
        }

        // Send Message To Target
        ChatBuffer message = new ();
        message.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM);
        message.WriteInt8(requestData.Flags);
        message.WriteString(session.Account.NameWithClanTag); // Sender Name
        
        if (requestData.Flags != 0)
        {
            message.WriteInt32(session.Account.ID);
            message.WriteInt8(Convert.ToByte(session.Metadata.LastKnownClientState));
            // TODO: Implement Session Flags (IsOfficer, IsStaff etc). For now passing 0 or converting account type.
            byte flags = 0; 
            if (session.Account.Type == AccountType.Staff) flags |= 0x20; // Example flag mapping
            
            message.WriteInt8(flags); 
            message.WriteString(session.Account.NameColourNoPrefixCode);
            message.WriteString(session.Account.IconNoPrefixCode);
            message.WriteInt32(session.Account.AscensionLevel);
        }
        
        message.WriteString(requestData.Message);
        
        targetSession.Send(message);
        
        // If Flag is 1, Echo Back To Sender (Confirmation)
        // Legacy: Subject.SendResponse(new InstantMessageResponse(2, client, clientAccount, Message));
        if (requestData.Flags == 1)
        {
             ChatBuffer echo = new ();
             echo.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM);
             echo.WriteInt8(2); // Flags = 2 for echo? Legacy used 2.
             echo.WriteString(targetSession.Account.NameWithClanTag); // In echo, we write Target Name
             
             // Flags != 0 check applies here too since we passed 2.
             echo.WriteInt32(targetSession.Account.ID);
             echo.WriteInt8(Convert.ToByte(targetSession.Metadata.LastKnownClientState));
             
             byte targetFlags = 0;
             if (targetSession.Account.Type == AccountType.Staff) targetFlags |= 0x20;

             echo.WriteInt8(targetFlags);
             echo.WriteString(targetSession.Account.NameColourNoPrefixCode);
             echo.WriteString(targetSession.Account.IconNoPrefixCode);
             echo.WriteInt32(targetSession.Account.AscensionLevel);
             
             echo.WriteString(requestData.Message);
             
             session.Send(echo);
        }
    }
}

file class SendInstantMessageRequestData
{
    public byte[] CommandBytes { get; init; }
    public string TargetName { get; init; }
    public string Message { get; init; }
    public byte Flags { get; init; } // 1 = Request Echo/Saved History? 

    public SendInstantMessageRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetName = buffer.ReadString();
        Message = buffer.ReadString();
        Flags = buffer.ReadInt8();
    }
}
