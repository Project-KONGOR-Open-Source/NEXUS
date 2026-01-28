using TRANSMUTANSTEIN.ChatServer.Domain.Communication;
using TRANSMUTANSTEIN.ChatServer.Internals;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_WHISPER_BUDDIES)]
public class WhisperBuddies(IChatContext chatContext) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        WhisperBuddiesRequestData requestData = new(buffer);

        if (session.Account == null)
        {
            return;
        }

        Whisper whisper = Whisper.Create(requestData.Message);

        foreach (FriendedPeer friend in session.Account.FriendedPeers)
        {
            // Find friend session
            ChatSession? friendSession = chatContext.ClientChatSessions.Values
                .FirstOrDefault(s => s.Account.ID == friend.ID);

            if (friendSession != null)
            {
                whisper.Send(session, friendSession);
            }
        }
    }
}
