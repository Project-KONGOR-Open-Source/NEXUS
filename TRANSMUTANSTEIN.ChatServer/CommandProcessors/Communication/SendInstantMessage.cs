using MERRICK.DatabaseContext.Extensions;
using TRANSMUTANSTEIN.ChatServer.Internals;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_IM)]
public class SendInstantMessage(IChatContext chatContext) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SendInstantMessageRequestData requestData = new(buffer);

        // Find Target Session
        ChatSession? targetSession = chatContext.ClientChatSessions.Values
            .FirstOrDefault(s => s.Account.Name.Equals(requestData.TargetName, StringComparison.OrdinalIgnoreCase)
                                 || s.Account.GetNameWithClanTag().Equals(requestData.TargetName,
                                     StringComparison.OrdinalIgnoreCase));

        // Check If Target Is Offline Or Invisible
        if (targetSession is null || targetSession.ClientMetadata.ClientChatModeState ==
            ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
        {
            // Send Failure Response To Sender
            ChatBuffer failure = new();
            failure.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM_FAILED);
            failure.WriteString(requestData.TargetName);

            session.Send(failure);
            return;
        }

        // Send Message To Target
        ChatBuffer message = new();
        message.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM);
        message.WriteInt8(requestData.Flags);
        message.WriteString(session.Account.GetNameWithClanTag()); // Sender Name (Always Sent)

        if (requestData.Flags != 0)
        {
            message.WriteInt32(session.Account.ID);
            message.WriteInt8(Convert.ToByte(session.ClientMetadata.LastKnownClientState)); // Status
            message.WriteInt8(session.Account.GetChatClientFlags()); // Client Flags (Staff, etc)
            message.WriteString(session.Account.GetNameColourNoPrefixCode());
            message.WriteString(session.Account.GetIconNoPrefixCode()); // Account Icon
            message.WriteInt32(session.Account.AscensionLevel); // Ascension Level
        }

        message.WriteString(requestData.Message);

        targetSession.Send(message);

        // If Flag is 1, Echo Back To Sender (Confirmation)
        if (requestData.Flags == 1)
        {
            ChatBuffer echo = new();
            echo.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM);
            echo.WriteInt8(2); // Flags = 2
            echo.WriteString(targetSession.Account.GetNameWithClanTag()); // Target Name

            // Because Flags != 0 (it is 2), we populate the extra fields for the target
            echo.WriteInt32(targetSession.Account.ID);
            echo.WriteInt8(Convert.ToByte(targetSession.ClientMetadata.LastKnownClientState)); // Status
            echo.WriteInt8(targetSession.Account.GetChatClientFlags()); // Target Flags
            echo.WriteString(targetSession.Account.GetNameColourNoPrefixCode());
            echo.WriteString(targetSession.Account.GetIconNoPrefixCode()); // Target Icon
            echo.WriteInt32(targetSession.Account.AscensionLevel); // Target Level

            echo.WriteString(requestData.Message);

            session.Send(echo);
        }
    }
}