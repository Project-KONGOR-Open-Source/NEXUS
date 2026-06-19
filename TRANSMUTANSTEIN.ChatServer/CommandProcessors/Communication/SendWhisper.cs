namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_WHISPER)]
public class SendWhisper(FloodPreventionService floodPreventionService) : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        SendWhisperRequestData requestData = new (buffer);

        // If The Flood Prevention Service Returns False, The Session Has Been Notified That The Client Has Exceeded The Flood Threshold; In This Case, The Command Is Aborted
        if (floodPreventionService.CheckAndHandleFloodPrevention(session) is false)
            return;

        Whisper
            .Create(requestData.Message)
            .Send(session, requestData.TargetName);
    }
}

file class SendWhisperRequestData
{
    public byte[] CommandBytes { get; init; }

    public string TargetName { get; init; }

    public string Message { get; init; }

    public SendWhisperRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetName = buffer.ReadString();
        Message = buffer.ReadString();
    }
}
