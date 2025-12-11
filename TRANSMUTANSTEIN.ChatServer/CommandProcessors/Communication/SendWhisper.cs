namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_WHISPER)]
public class SendWhisper : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        WhisperRequestData requestData = new (buffer);

        Whisper
            .Create(requestData.Message)
            .Send(session, requestData.TargetName);
    }
}

public class WhisperRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public string TargetName = buffer.ReadString();

    public string Message = buffer.ReadString();
}
