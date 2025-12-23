namespace KINESIS.Client;

public class ClanWhisperResponse : ProtocolResponse
{
    private readonly int _accountId;
    public int AccountId => _accountId;
    private readonly string _message;
    public string Message => _message;

    public ClanWhisperResponse(int accountId, string message)
    {
        _accountId = accountId;
        _message = message;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer response = new();
        response.WriteInt16(ChatServerResponse.WhisperedToClanmates);
        response.WriteInt32(_accountId);
        response.WriteString(_message);
        return response;
    }
}
