namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_WHISPER)]
public class SendWhisper : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        WhisperRequestData requestData = new (buffer);

        Whisper
            .Send(session, requestData.TargetName, requestData.Message);
    }
}

public class WhisperRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public string TargetName = buffer.ReadString();

    public string Message = buffer.ReadString();
}
