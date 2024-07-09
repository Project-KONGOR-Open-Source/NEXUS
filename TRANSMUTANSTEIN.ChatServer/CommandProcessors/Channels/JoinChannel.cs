namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL)]
public class JoinChannel(MerrickContext merrick, ILogger<JoinChannel> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<JoinChannel> Logger { get; set; } = logger;

    public async Task Process(TCPSession session, ChatBuffer buffer)
    {
        JoinChannelRequestData requestData = new(buffer);

        // TODO: Handle Max Channels

        // TODO: Handle Channel Passwords And Permissions
    }
}

public class JoinChannelRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string Channel = buffer.ReadString();
}
