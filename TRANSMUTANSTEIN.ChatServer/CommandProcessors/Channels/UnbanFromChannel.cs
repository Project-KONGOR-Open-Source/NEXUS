using Microsoft.EntityFrameworkCore;

using TRANSMUTANSTEIN.ChatServer.Domain.Communication;
using TRANSMUTANSTEIN.ChatServer.Internals;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_UNBAN)]
public class UnbanFromChannel(MerrickContext merrick, IChatContext chatContext) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        UnbanFromChannelRequestData requestData = new(buffer);

        int targetAccountId;
        string targetName = requestData.TargetName;
        Log.Information($"[DEBUG] UnbanFromChannel: Processing Request. TargetName='{targetName}', ChannelID={requestData.ChannelID}");

        ChatSession? targetSession = chatContext.ClientChatSessions.Values
            .FirstOrDefault(chatSession =>
                chatSession.Account.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

        // Fallback: Try checking for name without clan tag
        if (targetSession == null && targetName.Contains(']'))
        {
             int closingBracketIndex = targetName.IndexOf(']');
             if (closingBracketIndex >= 0 && closingBracketIndex < targetName.Length - 1)
             {
                 string strippedName = targetName[(closingBracketIndex + 1)..];
                 targetSession = chatContext.ClientChatSessions.Values
                    .FirstOrDefault(chatSession =>
                        chatSession.Account.Name.Equals(strippedName, StringComparison.OrdinalIgnoreCase));
                 
                 if (targetSession != null)
                 {
                     targetName = strippedName;
                 }
             }
        }

        if (targetSession != null)
        {
            targetAccountId = targetSession.Account.ID;
            targetName = targetSession.Account.Name;
        }
        else
        {
            var account = await merrick.Accounts
                .Select(a => new { a.ID, a.Name })
                .FirstOrDefaultAsync(a => a.Name == targetName);

            // Fallback for DB: Try stripped name
            if (account == null && targetName.Contains(']'))
            {
                 int closingBracketIndex = targetName.IndexOf(']');
                 if (closingBracketIndex >= 0 && closingBracketIndex < targetName.Length - 1)
                 {
                     string strippedName = targetName[(closingBracketIndex + 1)..];
                     
                     account = await merrick.Accounts
                        .Select(a => new { a.ID, a.Name })
                        .FirstOrDefaultAsync(a => a.Name == strippedName);
                        
                     if (account != null)
                     {
                         targetName = strippedName;
                     }
                 }
            }

            if (account == null)
            {
                // TODO: Notify Requester That Target User Was Not Found
                return;
            }

            targetAccountId = account.ID;
            targetName = account.Name;
        }

        ChatChannel? channel = ChatChannel.Get(chatContext, session, requestData.ChannelID);

        channel?.Unban(session, targetAccountId, targetName);
    }
}
