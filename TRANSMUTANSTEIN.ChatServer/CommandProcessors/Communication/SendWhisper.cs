using TRANSMUTANSTEIN.ChatServer.Internals;
using TRANSMUTANSTEIN.ChatServer.Domain.Communication;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_WHISPER)]
public class SendWhisper(IChatContext chatContext) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SendWhisperRequestData requestData = new(buffer);

        Whisper
            .Create(requestData.Message)
            .Send(chatContext, session, requestData.TargetName);
    }
}