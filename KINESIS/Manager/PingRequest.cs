namespace KINESIS.Manager;

public class PingRequest : ProtocolRequest
{
    private static PingReceivedResponse _pingReceivedResponse = new PingReceivedResponse();

    public static PingRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        updatedOffset = offset;
        return new();
    }

}

