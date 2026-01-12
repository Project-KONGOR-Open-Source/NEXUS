using MERRICK.DatabaseContext.Extensions;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_IM)]
public class SendInstantMessage : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SendInstantMessageRequestData requestData = new(buffer);

        // Find Target Session
        ChatSession? targetSession = Context.ClientChatSessions.Values
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
        // Sender Name is now written inside the if (Flags != 0) block, or not at all if Flags == 0.
        // This is a change from the original code where it was always written.
        // The provided snippet implies this structure.

        if (requestData.Flags != 0)
        {
            message.WriteInt32(session.Account.ID);
            message.WriteInt8(Convert.ToByte(session.ClientMetadata.LastKnownClientState));
            // TODO: Implement Session Flags (IsOfficer, IsStaff etc). For now passing 0 or converting account type.
            byte flags = 0;
            if (session.Account.Type == AccountType.Staff)
            {
                flags |= 0x20; // Example flag mapping
            }

            message.WriteInt8(flags);
            message.WriteString(session.Account.GetNameWithClanTag()); // Sender Name
            message.WriteInt32(session.Account.ID); // Sender Account ID
            message.WriteInt8(0x00); // 0x00
            message.WriteString(session.Account.GetNameColourNoPrefixCode());
        }

        message.WriteString(requestData.Message);

        targetSession.Send(message);

        // If Flag is 1, Echo Back To Sender (Confirmation)
        // Legacy: Subject.SendResponse(new InstantMessageResponse(2, client, clientAccount, Message));
        if (requestData.Flags == 1)
        {
            ChatBuffer echo = new();
            echo.WriteCommand(ChatProtocol.Command.CHAT_CMD_IM);
            echo.WriteInt8(2); // Flags = 2 for echo? Legacy used 2.
            echo.WriteString(targetSession.Account.GetNameWithClanTag()); // In echo, we write Target Name

            // Flags != 0 check applies here too since we passed 2.
            echo.WriteInt32(targetSession.Account.ID); // Target Account ID
            echo.WriteInt8(0x00); // 0x00
            echo.WriteString(targetSession.Account.GetNameColourNoPrefixCode());
            echo.WriteInt8(Convert.ToByte(targetSession.ClientMetadata.LastKnownClientState));

            byte targetFlags = 0;
            if (targetSession.Account.Type == AccountType.Staff)
            {
                targetFlags |= 0x20;
            }

            echo.WriteInt8(targetFlags);
            echo.WriteString(targetSession.Account.GetNameColourNoPrefixCode());
            echo.WriteString(targetSession.Account.GetIconNoPrefixCode());
            echo.WriteInt32(targetSession.Account.AscensionLevel);

            echo.WriteString(requestData.Message);

            session.Send(echo);
        }
    }
}