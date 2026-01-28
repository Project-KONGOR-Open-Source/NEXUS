using Microsoft.EntityFrameworkCore;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_BLOCK_PHRASE)]
public class BlockPhrase(MerrickContext merrick) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        BlockPhraseRequestData requestData = new(buffer);

        if (session.Account == null)
        {
            return;
        }

        string phrase = requestData.Phrase;

        if (string.IsNullOrWhiteSpace(phrase))
        {
            return;
        }

        // Fetch account from DB to update
        Account? account = await merrick.Accounts
            .FirstOrDefaultAsync(a => a.ID == session.Account.ID);

        if (account != null)
        {
            // Ensure the list is initialized (it should be due to default)
            account.BlockedPhrases ??= [];

            if (!account.BlockedPhrases.Contains(phrase))
            {
                account.BlockedPhrases.Add(phrase);
                await merrick.SaveChangesAsync();

                // Update Session Cache
                if (!session.Account.BlockedPhrases.Contains(phrase))
                {
                    session.Account.BlockedPhrases.Add(phrase);
                }
            }
        }
    }
}
