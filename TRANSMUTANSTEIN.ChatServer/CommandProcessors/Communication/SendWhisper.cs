namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_WHISPER)]
public class SendWhisper : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SendWhisperRequestData requestData = new(buffer);

        Whisper
            .Create(requestData.Message)
            .Send(session, requestData.TargetName);
    }
}