namespace KINESIS.Client;

public class WhisperFailedResponse : ProtocolResponse
{
    public override CommandBuffer Encode()
    {
        CommandBuffer response = new();
        response.WriteInt16(ChatServerResponse.WhisperFailed);
        response.WriteInt8(0);
        response.WriteInt8(0);
        return response;
    }
}
