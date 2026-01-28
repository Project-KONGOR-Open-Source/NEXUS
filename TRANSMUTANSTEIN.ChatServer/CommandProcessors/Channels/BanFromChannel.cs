using Microsoft.EntityFrameworkCore;

using TRANSMUTANSTEIN.ChatServer.Domain.Communication;
using TRANSMUTANSTEIN.ChatServer.Internals;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_BAN)]
public class BanFromChannel(MerrickContext merrick, IChatContext chatContext) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        BanFromChannelRequestData requestData = new(buffer);
        Log.Information($"[DEBUG] BanFromChannel: Processing Request. TargetName='{requestData.TargetName}', ChannelID={requestData.ChannelID}");

        int targetAccountId;
        string targetName = requestData.TargetName;

        ChatSession? targetSession = chatContext.ClientChatSessions.Values
            .FirstOrDefault(chatSession =>
                chatSession.Account.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));
        
        Log.Information($"[DEBUG] BanFromChannel: TargetSession Found? {targetSession != null}");

        // Fallback: Try checking for name without clan tag (e.g. "[TAG]Name" -> "Name")
        if (targetSession == null && targetName.Contains(']'))
        {
             // Simple strip of everything up to the first ']'
             int closingBracketIndex = targetName.IndexOf(']');
             if (closingBracketIndex >= 0 && closingBracketIndex < targetName.Length - 1)
             {
                 string strippedName = targetName[(closingBracketIndex + 1)..];
                 
                 targetSession = chatContext.ClientChatSessions.Values
                    .FirstOrDefault(chatSession =>
                        chatSession.Account.Name.Equals(strippedName, StringComparison.OrdinalIgnoreCase));
                 
                 if (targetSession != null)
                 {
                     targetName = strippedName; // Update for DB lookup consistency if needed later? No, we have session.
                     Log.Information($"[DEBUG] BanFromChannel: TargetSession found via Stripped Name '{strippedName}'");
                 }
             }
        }

        if (targetSession != null)
        {
            targetAccountId = targetSession.Account.ID;
            targetName = targetSession.Account.Name;
            Log.Information($"[DEBUG] BanFromChannel: Resolved TargetID={targetAccountId}, Name='{targetName}'");
        }
        else
        {
            Log.Information("[DEBUG] BanFromChannel: TargetSession NULL. Looking up in DB...");
            // Lookup in DB for offline users
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
                Log.Error($"[DEBUG] BanFromChannel: Target User '{requestData.TargetName}' Not Found in Session OR DB. Exiting.");
                // TODO: Notify Requester That Target User Was Not Found
                return;
            }

            targetAccountId = account.ID;
            targetName = account.Name;
        }

        ChatChannel? channel = ChatChannel.Get(chatContext, session, requestData.ChannelID);
        Log.Information($"[DEBUG] BanFromChannel: Channel Found? {channel != null}. Calling Ban...");

        channel?.Ban(chatContext, session, targetAccountId, targetName);
    }
}
