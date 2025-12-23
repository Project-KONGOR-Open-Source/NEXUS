namespace KINESIS.Client;

public class ClanWhisperFailedResponse : ProtocolResponse
{
    public override CommandBuffer Encode()
    {
        CommandBuffer response = new();
        response.WriteInt16(ChatServerResponse.ClanWhisperFailed);
        return response;
    }
}
