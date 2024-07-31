namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_WHISPER)]
public class WhisperMessage(MerrickContext merrick, ILogger<WhisperMessage> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<WhisperMessage> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        WhisperMessagelRequestData requestData = new(buffer);

        ChatSession? recipientSession = ChatSession.ActiveSessions.Values
                .FirstOrDefault(s => s.ClientInformation.Account.Name.Equals(requestData.RecipientName, StringComparison.OrdinalIgnoreCase) ||
                                     s.ClientInformation.Account.NameWithClanTag.Equals(requestData.RecipientName, StringComparison.OrdinalIgnoreCase));
        if(recipientSession is not null)
        {
            Response.Clear();

            Response.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER);

            Response.WriteString(session.ClientInformation.Account.Name);

            Response.WriteString(requestData.Message);

            Response.PrependBufferSize();

            recipientSession.SendAsync(Response.Data);
        }
    }
}

public class WhisperMessagelRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string RecipientName = buffer.ReadString();
    public string Message = buffer.ReadString();
}
