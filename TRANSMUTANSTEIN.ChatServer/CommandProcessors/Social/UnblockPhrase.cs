using Microsoft.EntityFrameworkCore;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_UNBLOCK_PHRASE)]
public class UnblockPhrase(MerrickContext merrick) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        UnblockPhraseRequestData requestData = new(buffer);

        if (session.Account == null)
        {
            return;
        }

        string phrase = requestData.Phrase;

        // Fetch account from DB to update
        Account? account = await merrick.Accounts
            .FirstOrDefaultAsync(a => a.ID == session.Account.ID);

        if (account != null && account.BlockedPhrases != null)
        {
            if (account.BlockedPhrases.Remove(phrase))
            {
                await merrick.SaveChangesAsync();

                // Update Session Cache
                session.Account.BlockedPhrases.Remove(phrase);
            }
        }
    }
}
