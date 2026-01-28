using global::TRANSMUTANSTEIN.ChatServer.Internals;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Communication;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHAT_ROLL)]
public class ChatRoll(IChatContext chatContext) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        try
        {
            Console.WriteLine($"[ChatRoll] Processing Roll for Session {session.ID} Account {session.Account?.ID}");
            ChatRollRequestData requestData = new(buffer);
            Console.WriteLine($"[ChatRoll] Parsed Data: ChannelID={requestData.ChannelID} Params={requestData.Parameters}");

            ChatChannel? channel = ChatChannel.Get(chatContext, session, requestData.ChannelID);
            if (channel == null)
            {
                Console.WriteLine($"[ChatRoll] Channel {requestData.ChannelID} not found.");
                return;
            }

            Console.WriteLine($"[ChatRoll] Rolling in channel {channel.ID}");
            channel.Roll(session, requestData.Parameters);
            Console.WriteLine($"[ChatRoll] Roll executed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatRoll] CRASH: {ex}");
            throw;
        }
    }
}
