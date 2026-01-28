using TRANSMUTANSTEIN.ChatServer.Domain.Communication;
using TRANSMUTANSTEIN.ChatServer.Extensions.Protocol;
using TRANSMUTANSTEIN.ChatServer.Internals;
using MERRICK.DatabaseContext.Extensions;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_GET_USER_STATUS)]
public class GetUserStatus(IChatContext chatContext) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GetUserStatusRequestData requestData = new(buffer);
        int targetAccountId = requestData.AccountID;

        ChatSession? targetSession = chatContext.ClientChatSessions.Values
            .FirstOrDefault(s => s.Account.ID == targetAccountId);

        ChatBuffer response = new();
        response.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_USER_STATUS);

        if (targetSession != null)
        {
            response.WriteInt32(targetAccountId);
            response.WriteInt8((byte) targetSession.ClientMetadata.LastKnownClientState);

            // Visuals
            response.WriteInt8(targetSession.Account.GetChatClientFlags());
            response.WriteString(targetSession.Account.GetChatSymbolNoPrefixCode());
            response.WriteString(targetSession.Account.GetNameColourNoPrefixCode());
            response.WriteString(targetSession.Account.GetIconNoPrefixCode());
            response.WriteInt32(targetSession.Account.AscensionLevel);

            // In Game? Server IP?
            if (targetSession.ClientMetadata.LastKnownClientState == ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
            {
                // Maybe send Match ID or Server IP?
                // But generic "User Status" usually just visual + online state.
                // Leaving as is.
            }
        }
        else
        {
            response.WriteInt32(targetAccountId);
            response.WriteInt8((byte) ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_DISCONNECTED);

            // Defaults to prevent partial read errors
            response.WriteInt8(0);
            response.WriteString(string.Empty);
            response.WriteString(string.Empty);
            response.WriteString(string.Empty);
            response.WriteInt32(0);
        }

        session.Send(response);
    }
}
