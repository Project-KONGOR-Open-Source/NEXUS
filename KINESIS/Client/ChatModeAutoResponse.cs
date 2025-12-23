namespace KINESIS.Client;

public class ChatModeAutoResponse : ProtocolResponse
{
    private readonly ChatMode _chatMode;
    public ChatMode ChatMode => _chatMode;
    private readonly string _nickname;
    public string Nickname => _nickname;
    private readonly string _description;
    public string Description => _description;

    public ChatModeAutoResponse(ChatMode chatMode, string nickname, string description)
    {
        _chatMode = chatMode;
        _nickname = nickname;
        _description = description;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.ChatModeAutoResponse);
        buffer.WriteInt8(Convert.ToByte(_chatMode));
        buffer.WriteString(_nickname);
        buffer.WriteString(_description);
        return buffer;
    }
}
